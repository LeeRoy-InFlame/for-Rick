using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private bool _isPlayer = true;
    [SerializeField] private UnitAnimationController _animationController;

    // ИЗМЕНЕНО: Теперь храним ссылку на конкретного члена отряда (с его уровнем и статами)
    private UnitMember _unitMember;

    private Vector3Int _currentCell;
    private bool _isMoving = false;
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Stats")]
    // Эти поля хранят текущие значения во время боя (взяты из UnitMember при инициализации)
    private int _maxHealth;
    private int _currentHealth;
    private int _maxMoveDistance;
    private int _maxAttackRange;
    private int _maxActions;

    [Header("Actions")]
    private int _currentActions;
    private bool _hasMoved = false;
    private bool _hasShot = false;

    [Header("Visuals")]
    public Renderer[] HighlightRenderers;
    private MaterialPropertyBlock _propertyBlock;
    [SerializeField] private float _pulseSpeed = 3f;
    [SerializeField] private float _minBrightness = 0.5f;
    [SerializeField] private float _maxBrightness = 1.0f;
    private bool _isSelected = false;
    private bool _isInitialized = false;

    [Header("Health Bar")]
    [SerializeField] private HealthBar _healthBar;

    [Header("Combat")]
    public AttackBase AttackType;

    [Header("Death")]
    [SerializeField] private GameObject _deathRemainsPrefab;

    private Vector3Int _pendingAttackTarget;

    // --- Свойства (Getters) ---
    public Vector3Int CurrentCell { get { return _currentCell; } }
    public bool IsMoving { get { return _isMoving; } }
    public bool IsPlayer { get { return _isPlayer; } }
    public int MaxMoveDistance { get { return _maxMoveDistance; } }
    public int MaxAttackRange { get { return _maxAttackRange; } }
    public int MaxActions { get { return _maxActions; } }
    public int CurrentActions { get { return _currentActions; } }
    public bool HasMoved { get { return _hasMoved; } }
    public bool HasShot { get { return _hasShot; } }
    public int CurrentHealth { get { return _currentHealth; } }

    // НОВОЕ: Позволяет другим скриптам получить доступ к прокачанным статам этого конкретного юнита
    public UnitMember Member { get { return _unitMember; } }

    // УДОБНОЕ: Позволяет получить доступ к базовому шаблону (например, для префаба или атаки)
    public UnitData Data { get { return _unitMember != null ? _unitMember.Data : null; } }

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (_isSelected && !_isMoving)
        {
            ApplyPulseEffect();
        }
    }

    // ИЗМЕНЕНО: Метод теперь принимает UnitMember, а не UnitData
    // ИЗМЕНЕНО: Метод теперь принимает UnitMember, а не UnitData
    public void Initialize(Vector3Int startCell, UnitMember unitMember)
    {
        // 1. Сохраняем ссылку на конкретного члена отряда
        _unitMember = unitMember;

        // 2. Применяем статы ИЗ ЮНИТА (с учетом его уровня, прокачки и экипировки!)
        _maxHealth = unitMember.MaxHealth;
        _currentHealth = _maxHealth;
        _maxMoveDistance = unitMember.MaxMoveDistance;
        _maxAttackRange = unitMember.MaxAttackRange;
        _maxActions = unitMember.MaxActions;

        // 3. Если в базовых данных указана атака по умолчанию, назначаем её
        if (AttackType == null && unitMember.Data.DefaultAttackType != null)
        {
            AttackType = unitMember.Data.DefaultAttackType;
        }

        // 4. ЖЕСТКИЙ СБРОС ПОЗИЦИИ: Сначала убираем из старой клетки
        GridManager.Instance.UnregisterUnit(this);

        // 5. Устанавливаем новые координаты
        _currentCell = startCell;
        transform.position = GridManager.Instance.GetWorldPositionFromCell(_currentCell);

        // 6. Регистрируем в правильной клетке
        GridManager.Instance.RegisterUnit(this, _currentCell);

        _isInitialized = true;

        Debug.Log($"[Unit] {unitMember.Name} инициализирован на клетке {_currentCell} с HP: {_maxHealth}");

        ResetActions();
        SetSelected(false);

        // Инициализируем полоску здоровья
        if (_healthBar != null)
        {
            _healthBar.Initialize(_maxHealth);
        }

        // ПРОВЕРКА ВИЗУАЛИЗАТОРА
        EquipmentVisualizer visualizer = GetComponent<EquipmentVisualizer>();
        if (visualizer != null)
        {
            visualizer.ApplyEquipment(unitMember);
            Debug.Log($"[Unit] ✅ Визуал экипировки применён для {unitMember.Name}");
        }
        else
        {
            // ВОТ ЭТОТ ЛОГ ДОЛЖЕН ПОЯВИТЬСЯ, ЕСЛИ ТЫ ЗАБЫЛ ДОБАВИТЬ КОМПОНЕНТ
            Debug.LogError($"[Unit] ❌ ОШИБКА: На префабе '{gameObject.name}' ОТСУТСТВУЕТ компонент EquipmentVisualizer! Добавь его в префаб!");
        }
    }

    // --- ЛОГИКА ---

    public bool CanMoveTo(Vector3Int targetCell)
    {
        if (_hasMoved || _currentActions <= 0) return false;
        List<Vector3Int> path = Pathfinding.FindPath(_currentCell, targetCell, _maxMoveDistance);
        return path.Count > 0;
    }

    public bool CanAttack(Vector3Int targetCell)
    {
        if (_hasShot || _currentActions <= 0) return false;
        int distance = Mathf.Abs(targetCell.x - _currentCell.x) + Mathf.Abs(targetCell.y - _currentCell.y);
        return distance <= _maxAttackRange && distance > 0;
    }

    public void RequestMove(Vector3Int targetCell, List<Vector3Int> path)
    {
        if (!CanMoveTo(targetCell)) return;
        MoveAlongPath(path);
        UseMoveAction();
    }

    public void RequestAttack(Vector3Int targetCell)
    {
        if (!CanAttack(targetCell)) return;

        FaceTargetCell(targetCell);

        _pendingAttackTarget = targetCell;
        UseShootAction();

        // 1. СНАЧАЛА ВИЗУАЛИЗИРУЕМ АТАКУ
        AttackType.VisualizeAttack(this, targetCell);

        // 2. ЗАТЕМ ЗАПУСКАЕМ АНИМАЦИЮ ПЕРСОНАЖА
        _animationController.PlayShoot();
    }

    public void TriggerAttackExecution()
    {
        if (AttackType == null)
        {
            Debug.LogWarning($"{gameObject.name} не имеет типа атаки!");
            _pendingAttackTarget = Vector3Int.zero;
            return;
        }

        Unit target = GridManager.Instance.GetUnitAtCell(_pendingAttackTarget);
        bool isLethal = false;

        if (target != null)
        {
            isLethal = target.CurrentHealth <= AttackType.GetDamage();
        }

        if (isLethal && target != null)
        {
            CinematicStrikeManager.Instance.TriggerLethalStrike(this, target, () =>
            {
                CombatSystem.Instance.PerformAttack(this, _pendingAttackTarget, AttackType);
                _pendingAttackTarget = Vector3Int.zero;
            });
        }
        else
        {
            CombatSystem.Instance.PerformAttack(this, _pendingAttackTarget, AttackType);
            _pendingAttackTarget = Vector3Int.zero;
        }
    }

    private void UseMoveAction() { _hasMoved = true; _currentActions--; }
    private void UseShootAction() { _hasShot = true; _currentActions--; }

    public void ResetActions()
    {
        _currentActions = _maxActions;
        _hasMoved = false;
        _hasShot = false;
    }

    private void MoveAlongPath(List<Vector3Int> path)
    {
        _animationController.PlayMove();
        if (_isMoving || path.Count == 0) return;
        StartCoroutine(MoveCoroutine(path));
    }

    private IEnumerator MoveCoroutine(List<Vector3Int> path)
    {
        _isMoving = true;
        foreach (Vector3Int targetCell in path)
        {
            FaceTargetCell(targetCell);

            Vector3 startPos = transform.position;
            Vector3 targetPos = GridManager.Instance.GetWorldPositionFromCell(targetCell);

            float moveDuration = 1f / _moveSpeed;
            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;

            GridManager.Instance.UnregisterUnit(this);
            _currentCell = targetCell;
            GridManager.Instance.RegisterUnit(this, _currentCell);
        }
        _isMoving = false;
        _animationController.PlayIdle();
    }

    // ИЗМЕНЕНО: Добавили параметр attacker (по умолчанию null, если урон от ловушки/яд)
    public void TakeDamage(int damageAmount, Unit attacker = null)
    {
        _currentHealth -= damageAmount;
        Debug.Log($"{Member.Name} получил {damageAmount} урона! Осталось: {_currentHealth}");

        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar(_currentHealth);
        }

        if (_currentHealth <= 0)
        {
            // ГЛАВНОЕ: Если юнит умирает, и мы знаем, кто его убил
            if (attacker != null && attacker.IsPlayer && attacker.Member != null)
            {
                // Рассчитываем и начисляем опыт убийце
                int expReward = CalculateExpReward(attacker);
                attacker.Member.AddExp(expReward);

                Debug.Log($"✅ {attacker.Member.Name} получил {expReward} опыта за убийство {Member.Name}!");
            }

            Die();
        }
    }

    // НОВЫЙ МЕТОД: Формула расчета опыта
    private int CalculateExpReward(Unit killer)
    {
        // Простая формула: Базовые 20 очков + 5 очков за каждый уровень убийцы
        // (Чтобы высокоуровневым юнитам было сложнее качаться, или наоборот, можно брать уровень убитого)
        int baseExp = 20;
        return baseExp + (killer.Member.Level * 5);
    }

    public void StartForceMoveTo(Vector3Int newCell, float duration = 0.3f)
    {
        StartCoroutine(ForceMoveCoroutine(newCell, duration));
    }

    private IEnumerator ForceMoveCoroutine(Vector3Int newCell, float duration)
    {
        if (_currentHealth <= 0) yield break;

        Vector3 startPos = transform.position;
        Vector3 targetPos = GridManager.Instance.GetWorldPositionFromCell(newCell);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if (_currentHealth <= 0) yield break;

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            t = 1f - Mathf.Pow(1f - t, 3);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;

        GridManager.Instance.UnregisterUnit(this);
        _currentCell = newCell;
        GridManager.Instance.RegisterUnit(this, _currentCell);

        Debug.Log($"[Unit] {Member.Name} плавно перемещен на {_currentCell}");
    }

    private void Die()
    {
        Debug.Log($"{Member.Name} уничтожен!");

        GridManager.Instance.UnregisterUnit(this);

        if (GameManager.Instance.GetSelectedUnit() == this)
        {
            GameManager.Instance.ClearSelection();
        }

        _animationController.PlayDie();
    }

    public void ProcessDeathSequence()
    {
        if (_deathRemainsPrefab != null)
        {
            Instantiate(_deathRemainsPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning($"У {Member.Name} не назначен префаб останков!");
        }

        Destroy(gameObject);
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        if (HighlightRenderers == null || HighlightRenderers.Length == 0) return;

        if (_isSelected)
        {
            ApplyPulseEffect();
        }
        else
        {
            _propertyBlock.Clear();
            foreach (Renderer renderer in HighlightRenderers)
            {
                if (renderer != null) renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }

    private void ApplyPulseEffect()
    {
        if (HighlightRenderers == null || HighlightRenderers.Length == 0) return;
        float pulseValue = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f;
        float currentBrightness = Mathf.Lerp(_minBrightness, _maxBrightness, pulseValue);
        Color pulseColor = new Color(1f, 1f, 1f, currentBrightness);

        _propertyBlock.SetColor("_Color", pulseColor);
        foreach (Renderer renderer in HighlightRenderers)
        {
            if (renderer != null) renderer.SetPropertyBlock(_propertyBlock);
        }
    }

    private void FaceTargetCell(Vector3Int targetCell)
    {
        Vector3Int directionInt = targetCell - _currentCell;
        if (directionInt == Vector3Int.zero) return;

        bool isFacingLeft = directionInt.x < 0;
        Quaternion cameraRotation = Camera.main.transform.rotation;

        if (isFacingLeft)
            transform.rotation = cameraRotation * Quaternion.Euler(0, 180, 0);
        else
            transform.rotation = cameraRotation;
    }
}