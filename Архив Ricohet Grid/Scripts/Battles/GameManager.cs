using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject CellHighlightPrefab;
    public GameObject MoveHighlightPrefab;
    public GameObject AttackHighlightPrefab;
    public GameObject PathHighlightPrefab;

    [Header("UI References")]
    [SerializeField] private UnitContextMenu _unitContextMenu;
    [SerializeField] private Button _endTurnButton;

    private GameObject _currentHighlight;
    private List<GameObject> _areaHighlights = new List<GameObject>();
    private List<GameObject> _pathHighlights = new List<GameObject>();

    private Unit _selectedUnit;
    private List<EnemyAI> _enemies = new List<EnemyAI>();

    private enum InputMode { Idle, AwaitingMove, AwaitingAttack, Deployment, EnemyTurn }
    private InputMode _currentMode = InputMode.Deployment;

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
        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        _enemies = new List<EnemyAI>(allEnemies);
        Debug.Log($"[GameManager] Найдено {_enemies.Count} врагов. Начало фазы расстановки.");

        // Убеждаемся, что кнопка конца хода активна в начале
        if (_endTurnButton != null) _endTurnButton.interactable = true;
    }

    private void Update()
    {
        UpdateCursorHighlight();

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void UpdateCursorHighlight()
    {
        Vector3Int cellUnderMouse = GridManager.Instance.GetCellFromMouse();

        if (_currentHighlight == null)
        {
            _currentHighlight = Instantiate(CellHighlightPrefab);
        }

        Vector3 worldPos = GridManager.Instance.GetWorldPositionFromCell(cellUnderMouse);
        _currentHighlight.transform.position = worldPos;

        if (_currentMode == InputMode.Deployment)
        {
            bool isValid = DeploymentManager.Instance != null && DeploymentManager.Instance.IsValidDeploymentCell(cellUnderMouse);
            HighlightSingleCell(cellUnderMouse, isValid ? MoveHighlightPrefab : null);
        }
        else if (_currentMode == InputMode.AwaitingMove && _selectedUnit != null)
        {
            HighlightSingleCell(cellUnderMouse, _selectedUnit.CanMoveTo(cellUnderMouse) ? MoveHighlightPrefab : null);
        }
        else if (_currentMode == InputMode.AwaitingAttack && _selectedUnit != null)
        {
            HighlightSingleCell(cellUnderMouse, _selectedUnit.CanAttack(cellUnderMouse) ? AttackHighlightPrefab : null);
        }
    }

    private void HandleMouseClick()
    {
        // НОВОЕ: Если сейчас ход врагов - БЛОКИРУЕМ ВСЕ КЛИКИ ПО ПОЛЮ
        if (_currentMode == InputMode.EnemyTurn)
        {
            return;
        }

        // НОВОЕ: Если клик был по UI (например, по кнопке контекстного меню), 
        // мы НЕМЕДЛЕННО выходим, чтобы не обрабатывать клик по игровому полю!
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }


        Vector3Int clickedCell = GridManager.Instance.GetCellFromMouse();

        if (Mathf.Abs(clickedCell.x) > 50 || Mathf.Abs(clickedCell.y) > 50) return;

        // 1. Фаза расстановки (работает ТОЛЬКО если она активна)
        if (_currentMode == InputMode.Deployment)
        {
            DeploymentManager.Instance.TryDeployUnit(clickedCell);
            return;
        }

        // 2. Логика выбора юнита (работает в фазе боя Idle, AwaitingMove, AwaitingAttack)
        Unit clickedUnit = GridManager.Instance.GetUnitAtCell(clickedCell);

        if (clickedUnit != null)
        {
            if (clickedUnit.IsPlayer)
            {
                // Если кликнули на того же юнита и уже в режиме действия -> сбрасываем режим, показываем меню
                if (_selectedUnit == clickedUnit && (_currentMode == InputMode.AwaitingMove || _currentMode == InputMode.AwaitingAttack))
                {
                    ExitActionMode(); // Скрывает подсветку клеток
                    SelectUnit(_selectedUnit); // Показывает меню заново
                    return;
                }

                // Иначе просто выбираем/перевыбираем юнита
                SelectUnit(clickedUnit);
                return;
            }
            else // Клик по врагу
            {
                if (_currentMode == InputMode.AwaitingAttack && _selectedUnit != null)
                {
                    if (_selectedUnit.CanAttack(clickedCell))
                    {
                        _selectedUnit.RequestAttack(clickedCell);
                        ExitActionMode();
                        return;
                    }
                }
                return; // Если не в режиме атаки, клик по врагу ничего не делает
            }
        }

        // 3. Клик по ПУСТОЙ клетке
        if (_selectedUnit == null || _selectedUnit.IsMoving) return;

        if (_currentMode == InputMode.Idle)
        {
            // Клик в пустоту без выбранного действия -> снимаем выделение
            ClearSelection();
        }
        else if (_currentMode == InputMode.AwaitingMove)
        {
            if (_selectedUnit.CanMoveTo(clickedCell))
            {
                List<Vector3Int> path = Pathfinding.FindPath(_selectedUnit.CurrentCell, clickedCell, _selectedUnit.MaxMoveDistance);
                _selectedUnit.RequestMove(clickedCell, path);
                ExitActionMode();
            }
        }
        else if (_currentMode == InputMode.AwaitingAttack)
        {
            if (_selectedUnit.CanAttack(clickedCell))
            {
                _selectedUnit.RequestAttack(clickedCell);
                ExitActionMode();
            }
        }
    }

    public void EnterMoveMode()
    {
        if (_selectedUnit == null || _selectedUnit.HasMoved || _selectedUnit.CurrentActions <= 0) return;

        _currentMode = InputMode.AwaitingMove;
        ClearHighlights();
        ShowActionArea(_selectedUnit.CurrentCell, _selectedUnit.MaxMoveDistance, MoveHighlightPrefab);
    }

    public void EnterAttackMode()
    {
        if (_selectedUnit == null || _selectedUnit.HasShot || _selectedUnit.AttackType == null) return;

        _currentMode = InputMode.AwaitingAttack;
        ClearHighlights();

        List<Vector3Int> validCells = _selectedUnit.AttackType.GetValidTargetCells(_selectedUnit);
        foreach (Vector3Int cell in validCells)
        {
            Vector3 worldPos = GridManager.Instance.GetWorldPositionFromCell(cell);
            GameObject highlight = Instantiate(AttackHighlightPrefab, worldPos, Quaternion.identity);
            _areaHighlights.Add(highlight);
        }
    }

    public void EndTurn()
    {
        // НОВОЕ: Защита от повторного нажатия во время хода врагов
        if (_currentMode == InputMode.EnemyTurn) return;

        if (_selectedUnit != null)
        {
            _selectedUnit.ResetActions();
            _selectedUnit.SetSelected(false);
        }
        _selectedUnit = null;
        _currentMode = InputMode.Idle;
        ClearHighlights();
        if (_unitContextMenu != null) _unitContextMenu.Hide();

        Debug.Log("Ход игрока завершен. Начинается ход врагов...");

        // Блокируем кнопку конца хода на время хода врагов
        if (_endTurnButton != null) _endTurnButton.interactable = false;

        StartCoroutine(ExecuteEnemyTurns());
    }

    private IEnumerator ExecuteEnemyTurns()
    {
        // НОВОЕ: Снимаем выделение с текущего юнита и скрываем меню
        ClearSelection();

        // НОВОЕ: Переключаем в режим хода врагов
        _currentMode = InputMode.EnemyTurn;

        yield return new WaitForSeconds(0.5f);

        foreach (EnemyAI enemy in _enemies)
        {
            if (enemy != null)
            {
                Unit enemyUnit = enemy.GetComponent<Unit>();
                if (enemyUnit != null) enemyUnit.ResetActions();

                yield return StartCoroutine(enemy.ExecuteTurn());
            }
        }

        Unit[] allPlayerUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit u in allPlayerUnits)
        {
            if (u.IsPlayer) u.ResetActions();
        }

        Debug.Log("Все враги походили. Ход возвращается к игроку.");

        // НОВОЕ: Возвращаем режим Idle и разблокируем управление
        _currentMode = InputMode.Idle;

        // Разблокируем кнопку конца хода
        if (_endTurnButton != null) _endTurnButton.interactable = true;
    }

    private void SelectUnit(Unit unit)
    {
        if (_selectedUnit != null && _selectedUnit != unit)
        {
            _selectedUnit.SetSelected(false);
        }

        _selectedUnit = unit;
        _selectedUnit.SetSelected(true);
        _currentMode = InputMode.Idle;
        ClearHighlights();

        // Показываем контекстное меню рядом с юнитом
        if (_unitContextMenu != null)
        {
            _unitContextMenu.SetupAndShow(_selectedUnit);
        }
    }

    private void ExitActionMode()
    {
        _currentMode = InputMode.Idle;
        ClearHighlights();
    }

    private void ShowActionArea(Vector3Int centerCell, int range, GameObject highlightPrefab)
    {
        List<Vector3Int> reachableCells = Pathfinding.GetReachableCells(centerCell, range);
        foreach (Vector3Int cell in reachableCells)
        {
            Vector3 worldPos = GridManager.Instance.GetWorldPositionFromCell(cell);
            GameObject highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity);
            _areaHighlights.Add(highlight);
        }
    }

    private void HighlightSingleCell(Vector3Int cell, GameObject prefab)
    {
        foreach (GameObject h in _areaHighlights)
        {
            if (h.transform.position == GridManager.Instance.GetWorldPositionFromCell(cell)) return;
        }
    }

    private void ClearHighlights()
    {
        foreach (GameObject highlight in _areaHighlights) Destroy(highlight);
        foreach (GameObject highlight in _pathHighlights) Destroy(highlight);
        _areaHighlights.Clear();
        _pathHighlights.Clear();
    }

    public Unit GetSelectedUnit() { return _selectedUnit; }

    public void ClearSelection()
    {
        if (_selectedUnit != null) _selectedUnit.SetSelected(false);
        _selectedUnit = null;
        _currentMode = InputMode.Idle;
        ClearHighlights();
        if (_unitContextMenu != null) _unitContextMenu.Hide();
    }

    // --- ДОБАВЬ ЭТОТ МЕТОД В КОНЕЦ КЛАССА GameManager ---

    public void StartCombat()
    {
        // Небольшая проверка: расставил ли игрок хотя бы кого-то?
        if (DeploymentManager.Instance != null && DeploymentManager.Instance.GetDeployedCount() == 0)
        {
            Debug.LogWarning("Сначала расставь хотя бы одного юнита, Морти!");
            return;
        }

        // Сообщаем менеджеру расстановки, что фаза завершена
        if (DeploymentManager.Instance != null)
        {
            DeploymentManager.Instance.FinishDeployment();
        }

        // ГЛАВНОЕ: Переключаем режим игры на боевой (Idle)
        _currentMode = InputMode.Idle;

        // Очищаем старую подсветку зоны расстановки, чтобы не мешала
        ClearHighlights();

        Debug.Log("=== БОЙ НАЧАЛСЯ ===");
    }
}