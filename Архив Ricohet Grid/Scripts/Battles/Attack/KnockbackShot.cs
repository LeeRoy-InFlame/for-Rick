using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackShot : AttackBase
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private int _maxRange = 4;
    [SerializeField] private int _pushDistance = 1;
    [SerializeField] private int _collisionDamage = 1;

    public override bool CanHitTarget(Unit attacker, Vector3Int targetCell)
    {
        int distance = Mathf.Abs(targetCell.x - attacker.CurrentCell.x) +
                       Mathf.Abs(targetCell.y - attacker.CurrentCell.y);

        if (distance > _maxRange || distance == 0) return false;

        Vector3Int hitCell;
        return HasStrictLineOfSight(attacker.CurrentCell, targetCell, out hitCell);
    }

    public override void ExecuteAttack(Unit attacker, Vector3Int targetCell)
    {
        Debug.Log($"→ Отталкивающий выстрел по {targetCell}");

        Unit target = GridManager.Instance.GetUnitAtCell(targetCell);

        if (target != null)
        {
            // 1. Наносим базовый урон от самого выстрела (attacker уже передан!)
            CombatSystem.Instance.ApplyDamage(attacker, target, _damage);

            // 2. Если юнит умер, отталкивать нечего
            if (target.CurrentHealth <= 0)
            {
                Debug.Log($"→ {target.Member.Name} уничтожен выстрелом и не может быть отброшен.");
                return;
            }

            // 3. Вычисляем направление и делегируем работу KnockbackSystem
            int deltaX = targetCell.x - attacker.CurrentCell.x;
            int deltaY = targetCell.y - attacker.CurrentCell.y;
            Vector3Int pushDirection = new Vector3Int(GetSign(deltaX), GetSign(deltaY), 0);

            // ИЗМЕНЕНО: Добавляем 'attacker' последним аргументом!
            KnockbackSystem.Instance.ResolveKnockback(
                target,
                targetCell,
                pushDirection,
                _pushDistance,
                _collisionDamage,
                attacker // <-- ВОТ ОН, ГЕРОЙ ДНЯ
            );
        }
        else
        {
            Debug.Log($"→ Промах! На клетке {targetCell} никого нет.");
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
                Vector3Int hitCell;
                if (HasStrictLineOfSight(center, targetCell, out hitCell))
                {
                    validCells.Add(targetCell);
                }
            }
        }
        return validCells;
    }

    public bool HasStrictLineOfSight(Vector3Int start, Vector3Int end, out Vector3Int hitCell)
    {
        hitCell = end;
        int steps = Mathf.Max(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y));
        if (steps == 0) return false;

        for (int i = 1; i < steps; i++)
        {
            float t = (float)i / steps;
            int checkX = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
            int checkY = Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t));
            Vector3Int checkCell = new Vector3Int(checkX, checkY, 0);

            if (IsWall(checkCell))
            {
                hitCell = checkCell;
                return false;
            }

            Unit unitInPath = GridManager.Instance.GetUnitAtCell(checkCell);
            if (unitInPath != null)
            {
                hitCell = checkCell;
                return false;
            }
        }
        return true;
    }

    private bool IsWall(Vector3Int cell)
    {
        Vector3 worldPos = GridManager.Instance.GetWorldPositionFromCell(cell);
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

    private int GetSign(int value)
    {
        if (value > 0) return 1;
        if (value < 0) return -1;
        return 0;
    }

    public override int GetDamage() { return _damage; }

    public override void VisualizeAttack(Unit attacker, Vector3Int targetCell)
    {
        Vector3Int hitCell;
        bool hasLOS = HasStrictLineOfSight(attacker.CurrentCell, targetCell, out hitCell);

        Vector3 endPos = hasLOS ?
            GridManager.Instance.GetWorldPositionFromCell(targetCell) :
            GridManager.Instance.GetWorldPositionFromCell(hitCell);

        Color lineColor = hasLOS ? new Color(1f, 0.5f, 0f) : Color.yellow;
        StartCoroutine(DrawLineCoroutine(attacker.transform.position, endPos, lineColor, 0.5f));
    }

    private IEnumerator DrawLineCoroutine(Vector3 start, Vector3 end, Color color, float duration)
    {
        GameObject lineObj = new GameObject("AttackLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startColor = color;
        lr.endColor = color;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        yield return new WaitForSeconds(duration);
        Destroy(lineObj);
    }
}