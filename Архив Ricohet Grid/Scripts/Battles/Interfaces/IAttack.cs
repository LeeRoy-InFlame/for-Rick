using System.Collections.Generic;
using UnityEngine;

// Интерфейс описывает контракт для любого типа атаки
public interface IAttack
{
    bool CanHitTarget(Unit attacker, Vector3Int targetCell);
    void ExecuteAttack(Unit attacker, Vector3Int targetCell);
    List<Vector3Int> GetValidTargetCells(Unit attacker);
    int GetDamage();

    // НОВОЕ: Каждая атака сама решает, как себя визуализировать
    void VisualizeAttack(Unit attacker, Vector3Int targetCell);
}