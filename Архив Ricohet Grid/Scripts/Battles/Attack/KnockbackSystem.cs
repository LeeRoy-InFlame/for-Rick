using UnityEngine;

public class KnockbackSystem : MonoBehaviour
{
    public static KnockbackSystem Instance { get; private set; }

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

    public enum KnockbackResult { Moved, HitWall, HitUnit, HitHazard }

    /// <summary>
    /// Универсальный метод для расчета и применения отталкивания.
    /// </summary>
    // ИЗМЕНЕНО: Добавлен параметр attacker в конец
    public KnockbackResult ResolveKnockback(Unit pushedUnit, Vector3Int startCell, Vector3Int direction, int distance, int collisionDamage, Unit attacker)
    {
        if (pushedUnit == null || pushedUnit.CurrentHealth <= 0) return KnockbackResult.HitHazard;

        Vector3Int landingCell = startCell + (direction * distance);

        // 1. Проверка на ловушку/пропасть
        if (GridManager.Instance.IsHazard(landingCell))
        {
            Debug.Log($"→ {pushedUnit.Member.Name} отброшен в ловушку на клетке {landingCell} и уничтожен!");
            // ИЗМЕНЕНО: Передаем attacker, чтобы исходный стрелок получил опыт за убийство ловушкой
            pushedUnit.TakeDamage(999, attacker);
            return KnockbackResult.HitHazard;
        }

        // 2. Проверка на стену
        if (GridManager.Instance.IsWall(landingCell))
        {
            int actualDamage = collisionDamage;
            Debug.Log($"→ {pushedUnit.Member.Name} впечатан в стену! Дополнительный урон: {actualDamage}");
            // ИЗМЕНЕНО: Передаем attacker
            pushedUnit.TakeDamage(actualDamage, attacker);
            return KnockbackResult.HitWall;
        }

        // 3. Проверка на столкновение с ДРУГИМ юнитом
        Unit blockingUnit = GridManager.Instance.GetUnitAtCell(landingCell);
        if (blockingUnit != null)
        {
            Debug.Log($"→ {pushedUnit.Member.Name} столкнулся с {blockingUnit.Member.Name} на клетке {landingCell}!");

            // Урон получают ОБА юнита, но виновник (attacker) один - тот, кто выстрелил!
            // ИЗМЕНЕНО: Передаем attacker обоим
            pushedUnit.TakeDamage(collisionDamage, attacker);
            blockingUnit.TakeDamage(collisionDamage, attacker);

            return KnockbackResult.HitUnit;
        }

        // 4. Если путь чист, просто перемещаем юнита (урона нет, опыт не нужен)
        Debug.Log($"→ {pushedUnit.Member.Name} успешно отброшен на свободную клетку {landingCell}.");
        pushedUnit.StartForceMoveTo(landingCell, duration: 0.25f);
        return KnockbackResult.Moved;
    }
}