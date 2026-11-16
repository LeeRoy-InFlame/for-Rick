using System.Collections.Generic;
using UnityEngine;

public class DeploymentManager : MonoBehaviour
{
    public static DeploymentManager Instance { get; private set; }

    [Header("Deployment Zone Settings")]
    [SerializeField] private int _leftEdge = 0;
    [SerializeField] private int _depth = 2;
    [SerializeField] private int _bottomEdge = -6;
    [SerializeField] private int _topEdge = 5;

    [Header("Prefabs")]
    [SerializeField] private GameObject _cellHighlightPrefab;

    [Header("State")]
    [SerializeField] private List<Unit> _deployedUnits = new List<Unit>();
    [SerializeField] private UnitMember _currentlySelectedMember;

    private bool _isDeploymentPhase = true;
    private List<GameObject> _zoneHighlights = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ПРЯМОЙ ЗАПРОС К PlayerRoster
        int availableCount = PlayerRoster.Instance != null ? PlayerRoster.Instance.GetRosterMembers().Count : 0;
        Debug.Log($"Фаза расстановки началась. Доступно юнитов в ростере: {availableCount}");
        CreateZoneHighlights();
    }

    private void CreateZoneHighlights()
    {
        for (int x = _leftEdge; x < _leftEdge + _depth; x++)
        {
            for (int y = _bottomEdge; y <= _topEdge; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector3 worldPos = GridManager.Instance.GetWorldPositionFromCell(cell);
                GameObject highlight = Instantiate(_cellHighlightPrefab, worldPos, Quaternion.identity);
                _zoneHighlights.Add(highlight);
            }
        }
        Debug.Log($"Создано {_zoneHighlights.Count} клеток зоны расстановки");
    }

    private void UpdateZoneHighlights()
    {
        foreach (GameObject highlight in _zoneHighlights)
        {
            if (highlight != null) highlight.SetActive(_isDeploymentPhase);
        }
    }

    // НОВОЕ: Метод для UI, чтобы узнать, кого можно выбрать для расстановки
    // Он гарантированно возвращает актуальный список из PlayerRoster
    public List<UnitMember> GetAvailableUnitsForDeployment()
    {
        if (PlayerRoster.Instance == null)
        {
            Debug.LogError("DeploymentManager: PlayerRoster не найден!");
            return new List<UnitMember>();
        }
        return PlayerRoster.Instance.GetRosterMembers();
    }

    // ИЗМЕНЕНО: Теперь мы проверяем, действительно ли этот юнит есть в PlayerRoster
    public void SelectUnitForDeployment(UnitMember member)
    {
        if (PlayerRoster.Instance == null) return;

        // Проверяем, есть ли этот юнит в официальном ростере
        if (!PlayerRoster.Instance.GetRosterMembers().Contains(member))
        {
            Debug.LogWarning("Попытка выбрать юнита, которого нет в PlayerRoster!");
            return;
        }

        if (_deployedUnits.Count >= PlayerRoster.Instance.GetMaxDeploymentCount())
        {
            Debug.Log("Достигнут лимит юнитов для расстановки! Нельзя выбрать нового.");
            _currentlySelectedMember = null;
            return;
        }

        _currentlySelectedMember = member;
        Debug.Log($"Выбран для расстановки: {member.Name}");
    }

    public bool TryDeployUnit(Vector3Int targetCell)
    {
        if (!_isDeploymentPhase) return false;

        // НОВОЕ: "УБИЙСТВО ЗОМБИ". 
        // Если в поле застрял пустой объект (созданный Unity в Инспекторе), мы его очищаем!
        if (_currentlySelectedMember != null && _currentlySelectedMember.Data == null)
        {
            Debug.LogWarning(" Обнаружен 'зомби' (пустой UnitMember без данных). Очищаем и ищем нормального...");
            _currentlySelectedMember = null;
        }

        // Теперь, если он был зомби, он стал null, и мы вызываем авто-выбор
        if (_currentlySelectedMember == null)
        {
            AutoSelectNextUnit();
        }

        if (_currentlySelectedMember == null)
        {
            Debug.Log("⚠️ Все юниты из ростера уже расставлены или ростер пуст!");
            return false;
        }

        // Дальше идет твой обычный код спавна...
        if (_currentlySelectedMember.Data.UnitPrefab == null)
        {
            Debug.LogError($"❌ ОШИБКА: В UnitData '{_currentlySelectedMember.Data.name}' нет префаба!");
            return false;
        }

        if (_deployedUnits.Count >= PlayerRoster.Instance.GetMaxDeploymentCount())
        {
            Debug.Log("Достигнут максимальный лимит юнитов на поле!");
            _currentlySelectedMember = null;
            return false;
        }

        if (!IsValidDeploymentCell(targetCell))
        {
            Debug.Log("Эта клетка вне зоны расстановки или занята!");
            return false;
        }

        // Спавн юнита
        GameObject newUnitObj = Instantiate(_currentlySelectedMember.Data.UnitPrefab);
        Unit newUnit = newUnitObj.GetComponent<Unit>();

        if (newUnit != null)
        {
            newUnit.Initialize(targetCell, _currentlySelectedMember);
            _deployedUnits.Add(newUnit);

            Debug.Log($"✅ Юнит '{_currentlySelectedMember.Name}' размещен. ({_deployedUnits.Count}/{PlayerRoster.Instance.GetMaxDeploymentCount()})");

            _currentlySelectedMember = null;
            return true;
        }

        Debug.LogError($"Ошибка: у префаба нет компонента Unit!");
        Destroy(newUnitObj);
        return false;
    }

    // НОВЫЙ МЕТОД: Сам ищет первого нерасставленного юнита в PlayerRoster
    private void AutoSelectNextUnit()
    {
        if (PlayerRoster.Instance == null)
        {
            Debug.LogError("PlayerRoster.Instance равен null! Где он?");
            return;
        }

        List<UnitMember> roster = PlayerRoster.Instance.GetRosterMembers();
        Debug.Log($"[AutoSelect] Сканируем ростер. Всего элементов: {roster.Count}");

        foreach (UnitMember member in roster)
        {
            // РЕНТГЕН: Печатаем имя и наличие данных
            string dataStatus = member.Data != null ? $"OK ({member.Data.name})" : "ПУСТО (NULL)";
            Debug.Log($"[AutoSelect] Проверяем: '{member.Name}' | Data: {dataStatus}");

            // Проверяем, нет ли уже этого юнита на поле
            bool isAlreadyDeployed = false;
            foreach (Unit deployedUnit in _deployedUnits)
            {
                if (deployedUnit.Member == member)
                {
                    isAlreadyDeployed = true;
                    break;
                }
            }

            if (!isAlreadyDeployed)
            {
                // Если юнит есть в ростере, но у него НЕТ данных — пропускаем его и ругаемся
                if (member.Data == null)
                {
                    Debug.LogError($"[AutoSelect] НАЙДЕН МУСОР! Юнит '{member.Name}' не имеет UnitData. Пропускаем его.");
                    continue; // Идем к следующему элементу списка
                }

                // Если данные есть — берем его!
                _currentlySelectedMember = member;
                Debug.Log($"[AutoSelect] УСПЕХ! Выбран юнит: {member.Name}");
                return;
            }
        }

        Debug.LogWarning("[AutoSelect] Подходящих нерасставленных юнитов с корректными данными не найдено.");
    }

    public bool IsValidDeploymentCell(Vector3Int cell)
    {
        if (cell.x < _leftEdge || cell.x >= _leftEdge + _depth) return false;
        if (cell.y < _bottomEdge || cell.y > _topEdge) return false;
        if (GridManager.Instance.GetUnitAtCell(cell) != null) return false;
        if (GridManager.Instance.IsWall(cell)) return false;
        return true;
    }

    public void FinishDeployment()
    {
        _isDeploymentPhase = false;
        _currentlySelectedMember = null;
        UpdateZoneHighlights();
        Debug.Log("Расстановка завершена! Начинается битва.");
    }

    public int GetDeployedCount()
    {
        return _deployedUnits.Count;
    }
}