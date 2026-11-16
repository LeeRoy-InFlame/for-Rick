using System.Collections.Generic;
using UnityEngine;

public class LevelSetup : MonoBehaviour
{
    [Header("Enemy Deployment")]
    // Список данных врагов (чертежей), которых нужно заспавнить на этом уровне
    [SerializeField] private List<UnitData> _enemyUnitsToSpawn = new List<UnitData>();

    // Точки спавна для врагов
    [SerializeField] private List<Vector3Int> _enemySpawnPoints = new List<Vector3Int>();

    private void Start()
    {
        SetupLevel();
    }

    private void SetupLevel()
    {
        Debug.Log("=== Инициализация уровня ===");

        // 1. Спавним врагов
        SpawnEnemies();

        // 2. Здесь можно инициализировать фазу расстановки игрока
        Debug.Log("Уровень готов. Ожидание расстановки игрока...");
    }

    private void SpawnEnemies()
    {
        int count = Mathf.Min(_enemyUnitsToSpawn.Count, _enemySpawnPoints.Count);

        for (int i = 0; i < count; i++)
        {
            UnitData enemyData = _enemyUnitsToSpawn[i];
            Vector3Int spawnCell = _enemySpawnPoints[i];

            // Проверка: не занята ли клетка
            if (GridManager.Instance.GetUnitAtCell(spawnCell) != null)
            {
                Debug.LogWarning($"Клетка {spawnCell} занята! Пропускаем спавн.");
                continue;
            }

            // Создаем врага из префаба, указанного в ScriptableObject
            GameObject enemyObj = Instantiate(enemyData.UnitPrefab);
            Unit enemyUnit = enemyObj.GetComponent<Unit>();

            if (enemyUnit != null)
            {
                // ГЕНИАЛЬНЫЙ ХАК: Создаем временного UnitMember для врага на лету!
                UnitMember enemyMember = new UnitMember();

                // Инициализируем его базовыми статами из чертежа (как новобранца 1-го уровня)
                enemyMember.Initialize(enemyData);

                // Теперь передаем этого временного члена отряда в строгий метод Initialize юнита
                enemyUnit.Initialize(spawnCell, enemyMember);

                Debug.Log($"Враг '{enemyMember.Name}' успешно размещен на клетке {spawnCell}");
            }
            else
            {
                Debug.LogError($"У префаба {enemyData.UnitPrefab.name} нет компонента Unit! Уничтожаем.");
                Destroy(enemyObj);
            }
        }
    }
}