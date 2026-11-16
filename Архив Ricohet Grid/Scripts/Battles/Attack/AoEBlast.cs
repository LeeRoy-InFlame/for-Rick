using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEBlast : AttackBase
{
    [SerializeField] private int _damage = 2;
    [SerializeField] private int _maxRange = 3; // Дальность броска гранаты
    [SerializeField] private LayerMask _targetLayerMask;

    public override bool CanHitTarget(Unit attacker, Vector3Int targetCell)
    {
        int distance = Mathf.Abs(targetCell.x - attacker.CurrentCell.x) +
                       Mathf.Abs(targetCell.y - attacker.CurrentCell.y);
        return distance <= _maxRange && distance > 0;
    }

    public override void ExecuteAttack(Unit attacker, Vector3Int targetCell)
    {
        Debug.Log($"→ Взрыв в зоне 3x3 на клетке {targetCell}");

        List<Vector3Int> blastZone = GetBlastZone(targetCell);

        foreach (Vector3Int cell in blastZone)
        {
            Unit target = GetUnitAtCell(cell);
            if (target != null)
            {
                CombatSystem.Instance.ApplyDamage(attacker, target, _damage);
                Debug.Log($"→ {target.name} в зоне взрыва получил {_damage} урона.");
            }
            // Логика разрушения стен может быть добавлена здесь позже
        }
    }

    public override List<Vector3Int> GetValidTargetCells(Unit attacker)
    {
        List<Vector3Int> validCells = new List<Vector3Int>();
        Vector3Int center = attacker.CurrentCell;

        for (int x = -_maxRange; x <= _maxRange; x++)
        {
            for (int y = -_maxRange; y <= _maxRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > _maxRange) continue;
                if (x == 0 && y == 0) continue;

                Vector3Int targetCell = center + new Vector3Int(x, y, 0);
                if (CanHitTarget(attacker, targetCell))
                {
                    validCells.Add(targetCell);
                }
            }
        }
        return validCells;
    }

    private List<Vector3Int> GetBlastZone(Vector3Int center)
    {
        List<Vector3Int> zone = new List<Vector3Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                zone.Add(center + new Vector3Int(x, y, 0));
            }
        }
        return zone;
    }

    private Unit GetUnitAtCell(Vector3Int cell)
    {
        Vector3 worldPos = GridManager.Instance.GetWorldPositionFromCell(cell);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.5f, _targetLayerMask);

        foreach (Collider2D collider in colliders)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null) return unit;
        }
        return null;
    }

    public override int GetDamage()
    {
        return _damage;
    }

    // --- НОВАЯ ВИЗУАЛИЗАЦИЯ ДЛЯ ВЗРЫВА ---
    public override void VisualizeAttack(Unit attacker, Vector3Int targetCell)
    {
        // Для взрыва мы рисуем не линию, а контур зоны поражения 3x3
        StartCoroutine(DrawAoEZoneCoroutine(targetCell, 0.8f));
    }

    private IEnumerator DrawAoEZoneCoroutine(Vector3Int centerCell, float duration)
    {
        GameObject lineObj = new GameObject("AoE_Visualizer");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.positionCount = 5; // 4 угла + 1 для замыкания квадрата
        lr.useWorldSpace = true;

        // Огненный цвет для взрыва (оранжево-красный с легкой прозрачностью)
        Color blastColor = new Color(1f, 0.4f, 0f, 0.8f);
        lr.startColor = blastColor;
        lr.endColor = blastColor;

        // Создаем простой материал для линии
        Material mat = new Material(Shader.Find("Sprites/Default"));
        lr.material = mat;

        // Вычисляем 4 угла зоны 3x3 в мировых координатах
        // Относительно центра: (-1,-1), (1,-1), (1,1), (-1,1)
        Vector3Int[] corners = new Vector3Int[5];
        corners[0] = centerCell + new Vector3Int(-1, -1, 0); // Левый нижний
        corners[1] = centerCell + new Vector3Int(1, -1, 0);  // Правый нижний
        corners[2] = centerCell + new Vector3Int(1, 1, 0);   // Правый верхний
        corners[3] = centerCell + new Vector3Int(-1, 1, 0);  // Левый верхний
        corners[4] = corners[0]; // Возвращаемся в начало, чтобы замкнуть квадрат

        // Преобразуем клетки сетки в мировые координаты и задаем точки линии
        for (int i = 0; i < 5; i++)
        {
            lr.SetPosition(i, GridManager.Instance.GetWorldPositionFromCell(corners[i]));
        }

        // Ждем и уничтожаем объект, чтобы не засорять память
        yield return new WaitForSeconds(duration);
        Destroy(lineObj);
    }
}