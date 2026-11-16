using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Публичная статическая переменная.
    public static GridManager Instance;

    // Приватная переменная.
    private Grid _grid;

    // РЕЕСТР: Хранит связь "Клетка -> Юнит, который на ней стоит"
    private Dictionary<Vector3Int, Unit> _cellOccupants = new Dictionary<Vector3Int, Unit>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _grid = GetComponent<Grid>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Математический Raycast. Самый надежный способ для изометрии/сеток.
    public Vector3Int GetCellFromMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // ИСПРАВЛЕНИЕ: Vector3.forward вместо Vector3.up!
        // Мы создаем плоскость, которая лежит в XY (как лист бумаги на столе), 
        // а не XZ (как пол в комнате).
        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            return _grid.WorldToCell(hitPoint);
        }

        return Vector3Int.zero;
    }

    public Vector3 GetWorldPositionFromCell(Vector3Int cell)
    {
        return _grid.GetCellCenterWorld(cell);
    }

    // ВОТ ОН, ВОЗВРАЩЕННЫЙ МЕТОД!
    public Vector3Int GetCellFromWorldPosition(Vector3 worldPosition)
    {
        return _grid.WorldToCell(worldPosition);
    }

    // --- МЕТОДЫ РЕЕСТРА ---

    // Вызывается, когда юнит появляется или перемещается
    public void RegisterUnit(Unit unit, Vector3Int cell)
    {
        // Сначала убираем его со старой позиции, если он там был
        UnregisterUnit(unit);

        // Записываем новую позицию
        _cellOccupants[cell] = unit;
    }

    // Вызывается, когда юнит умирает или нам нужно просто очистить клетку
    public void UnregisterUnit(Unit unit)
    {
        var keyToRemove = new Vector3Int();
        bool found = false;

        foreach (var kvp in _cellOccupants)
        {
            if (kvp.Value == unit)
            {
                keyToRemove = kvp.Key;
                found = true;
                break;
            }
        }

        if (found)
        {
            _cellOccupants.Remove(keyToRemove);
        }
    }

    // Главный метод: "Кто стоит на этой клетке?"
    public Unit GetUnitAtCell(Vector3Int cell)
    {
        bool found = _cellOccupants.TryGetValue(cell, out Unit unit);

        if (found)
        {
            Debug.Log($"[GridManager] УСПЕХ: На клетке {cell} найден юнит '{unit.name}'");
        }
        else
        {
            Debug.LogWarning($"[GridManager] ПРОВАЛ: На клетке {cell} никого нет! Всего в реестре юнитов: {_cellOccupants.Count}");

            // Выведем всех, кто ЕСТЬ в реестре, чтобы ты увидел правду
            foreach (var kvp in _cellOccupants)
            {
                Debug.Log($"[GridManager] -> В реестре есть: Юнит '{kvp.Value.name}' на клетке {kvp.Key}");
            }
        }

        return unit;
    }

    // Проверяет, можно ли стоять на этой клетке
    public bool IsCellWalkable(Vector3Int cell)
    {
        // 1. Если на клетке уже кто-то стоит - нельзя
        if (_cellOccupants.ContainsKey(cell)) return false;

        // 2. Проверяем стены (через 2D физику)
        Vector3 worldPos = GetWorldPositionFromCell(cell);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Wall") || collider.CompareTag("Wall"))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsWall(Vector3Int cell)
    {
        Vector3 worldPos = GetWorldPositionFromCell(cell);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Wall") || collider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsHazard(Vector3Int cell)
    {
        // Здесь ты можешь проверять тег "Hazard", "Water", "Lava" и т.д.
        Vector3 worldPos = GetWorldPositionFromCell(cell);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.4f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Hazard") || collider.CompareTag("Water"))
            {
                return true;
            }
        }
        return false;
    }
}