using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Добавлено для подстраховки загрузки сцены

public class PartyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private WorldMapNode _defaultStartNode; // Страховка для первого запуска

    [Header("Random Encounters")] // НОВОЕ: Настройки случайных боев
    [SerializeField, Range(0f, 1f)] private float _encounterChance = 0.3f; // 30% шанс при проверке
    [SerializeField] private float _checkInterval = 2.0f; // Проверять каждые 2 секунды
    [SerializeField] private string _battleSceneName = "BattleScene"; // Имя сцены боя (из Build Settings)

    private Queue<Vector3> _pathQueue = new Queue<Vector3>();
    private bool _isMoving = false;
    private WorldMapNode _targetDestinationNode; // Запоминаем, куда именно идем

    [SerializeField] private WorldMapNode _currentNode;

    // НОВОЕ: Таймер для отслеживания времени между проверками
    private float _timeSinceLastCheck = 0f;

    public bool IsMoving { get { return _isMoving; } }
    public WorldMapNode CurrentNode { get { return _currentNode; } }

    private void Start()
    {
        RestoreWorldMapState();
    }

    private void Update()
    {
        if (_isMoving && _pathQueue.Count > 0)
        {
            MoveAlongPath();

            // НОВОЕ: Проверяем на случайную встречу ТОЛЬКО во время движения
            CheckForRandomEncounter();
        }
    }

    // НОВОЕ: Логика броска кубика
    private void CheckForRandomEncounter()
    {
        _timeSinceLastCheck += Time.deltaTime;

        if (_timeSinceLastCheck >= _checkInterval)
        {
            _timeSinceLastCheck = 0f; // Сбрасываем таймер

            // Random.value выдает число от 0.0 до 1.0. Если оно меньше шанса, встречаем врага!
            if (Random.value < _encounterChance)
            {
                TriggerRandomEncounter();
            }
        }
    }

    // НОВОЕ: Действия при встрече
    private void TriggerRandomEncounter()
    {
        Debug.Log("⚠️ СЛУЧАЙНАЯ ВСТРЕЧА! Подготовка к бою...");

        // 1. Сохраняем текущее состояние (чтобы вернуться ровно на эту точку тропы)
        SaveWorldMapState();

        // 2. Используем наш шикарный SceneLoader для плавного перехода
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(_battleSceneName);
        }
        else
        {
            Debug.LogError("SceneLoader не найден! Используем стандартную загрузку Unity.");
            // Fallback на случай, если загрузчик сломан или еще не создан
            SceneManager.LoadScene(_battleSceneName);
        }
    }

    public void SaveWorldMapState()
    {
        if (WorldMapStateManager.Instance == null) return;

        WorldMapStateManager.Instance.LastKnownPosition = transform.position;

        if (_currentNode != null && Vector3.Distance(transform.position, _currentNode.transform.position) < 0.5f)
        {
            WorldMapStateManager.Instance.CurrentNodeName = _currentNode.NodeName;
            WorldMapStateManager.Instance.IsExactlyOnNode = true;
        }
        else
        {
            WorldMapStateManager.Instance.IsExactlyOnNode = false;
        }

        Debug.Log($"[Save] Состояние сохранено. Позиция: {transform.position}, Узел: {_currentNode?.NodeName ?? "В пути"}");
    }

    public void RestoreWorldMapState()
    {
        if (WorldMapStateManager.Instance == null) return;

        if (WorldMapStateManager.Instance.IsExactlyOnNode && !string.IsNullOrEmpty(WorldMapStateManager.Instance.CurrentNodeName))
        {
            WorldMapNode[] allNodes = FindObjectsByType<WorldMapNode>(FindObjectsSortMode.None);

            foreach (WorldMapNode node in allNodes)
            {
                if (node.NodeName == WorldMapStateManager.Instance.CurrentNodeName)
                {
                    SetInitialNode(node);
                    Debug.Log($"[Restore] Возврат на узел: {node.NodeName}");
                    return;
                }
            }
        }

        if (string.IsNullOrEmpty(WorldMapStateManager.Instance.CurrentNodeName) && _defaultStartNode != null)
        {
            SetInitialNode(_defaultStartNode);
            Debug.Log("[Restore] Первый запуск. Установка на стартовый узел.");
            return;
        }

        transform.position = WorldMapStateManager.Instance.LastKnownPosition;
        _currentNode = null;
        Debug.Log($"[Restore] Возврат на точную позицию в пути: {transform.position}");
    }

    public void SetInitialNode(WorldMapNode startNode)
    {
        _currentNode = startNode;
        transform.position = _currentNode.transform.position;
    }

    public void TravelTo(List<Vector3> pathPoints, WorldMapNode destinationNode)
    {
        if (pathPoints == null || pathPoints.Count == 0) return;

        _targetDestinationNode = destinationNode;
        _pathQueue.Clear();
        foreach (Vector3 point in pathPoints)
        {
            _pathQueue.Enqueue(point);
        }
        _isMoving = true;
    }

    private void MoveAlongPath()
    {
        Vector3 targetPoint = _pathQueue.Peek();

        transform.position = Vector3.MoveTowards(transform.position, targetPoint, _moveSpeed * Time.deltaTime);

        Vector3 direction = targetPoint - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
            else if (direction.x < 0)
            {
                transform.localScale = new Vector3(
                    -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }

        if (Vector3.Distance(transform.position, targetPoint) < 0.1f)
        {
            _pathQueue.Dequeue();

            if (_pathQueue.Count == 0)
            {
                _isMoving = false;
                _currentNode = _targetDestinationNode;
                TriggerNodeEvents();
            }
        }
    }

    private void TriggerNodeEvents()
    {
        if (_currentNode != null)
        {
            Debug.Log($"Кролик дошел до цели: {_currentNode.NodeName}!");
            _currentNode.OnNodeReached();
        }
    }
}