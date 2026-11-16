using System.Collections.Generic;
using UnityEngine;

// Абстрактный базовый класс для всех типов атак
public abstract class AttackBase : MonoBehaviour, IAttack
{
    public abstract bool CanHitTarget(Unit attacker, Vector3Int targetCell);
    public abstract void ExecuteAttack(Unit attacker, Vector3Int targetCell);
    public abstract List<Vector3Int> GetValidTargetCells(Unit attacker);
    public abstract int GetDamage();
    public abstract void VisualizeAttack(Unit attacker, Vector3Int targetCell);
}