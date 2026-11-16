using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance { get; private set; }

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

    public void PerformAttack(Unit attacker, Vector3Int targetCell, IAttack attackType)
    {
        if (!attackType.CanHitTarget(attacker, targetCell))
        {
            Debug.Log($"{attacker.Member.Name} не может использовать {attackType.GetType().Name} на клетку {targetCell}");
            return;
        }

        Debug.Log($"{attacker.Member.Name} использует {attackType.GetType().Name} на клетку {targetCell}!");
        attackType.ExecuteAttack(attacker, targetCell);
    }

    public List<Vector3Int> GetAttackPreview(Unit attacker, IAttack attackType)
    {
        return attackType.GetValidTargetCells(attacker);
    }

    // ИЗМЕНЕНО: Добавлен параметр attacker
    public void ApplyDamage(Unit attacker, Unit target, int damageAmount)
    {
        if (target != null)
        {
            // Передаем attacker в Unit.cs, чтобы тот мог начислить опыт!
            target.TakeDamage(damageAmount, attacker);
        }
    }

    public bool HasLineOfSight(Vector3Int startCell, Vector3Int endCell, out List<Vector3Int> path)
    {
        path = Pathfinding.FindPath(startCell, endCell, 100, allowTargetOccupied: true);
        return path.Count > 0;
    }
}