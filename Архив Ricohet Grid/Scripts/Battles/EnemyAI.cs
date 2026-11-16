using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private Unit _unit;

    void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    public IEnumerator ExecuteTurn()
    {
        Debug.Log($"=== {_unit.gameObject.name} начинает тактический анализ ===");

        Unit target = SelectBestTarget();

        if (target == null)
        {
            Debug.Log($"{_unit.gameObject.name}: Целей нет. Пропуск хода.");
            yield break;
        }

        Debug.Log($"Цель выбрана: {target.name} (HP: {target.CurrentHealth}) на клетке {target.CurrentCell}");
        yield return new WaitForSeconds(0.5f);

        bool canAttackNow = CanActuallyHit(_unit.CurrentCell, target);
        bool canMove = !_unit.HasMoved && _unit.CurrentActions > 0;

        // --- СЦЕНАРИЙ 1: Мы можем атаковать прямо сейчас ---
        if (canAttackNow)
        {
            // 1а. Сначала проверяем, есть ли позиция ЛУЧШЕ для атаки (например, с укрытием)
            Vector3Int betterPosition = FindBestTacticalMove(target);

            if (betterPosition != _unit.CurrentCell && HasCover(betterPosition, target.CurrentCell))
            {
                Debug.Log($"→ {gameObject.name} занимает укрытие для атаки...");
                yield return StartCoroutine(ExecuteMoveAndAttack(betterPosition, target));
            }
            else
            {
                // 1б. Текущая позиция уже хороша. Атакуем!
                Debug.Log($"→ {gameObject.name} атакует с текущей позиции!");
                _unit.RequestAttack(target.CurrentCell);
                yield return new WaitForSeconds(1f); // Ждем анимацию

                // 1в. НОВОЕ: "Shoot and Scoot". Если после атаки осталось движение, отступаем!
                if (!_unit.HasMoved && _unit.CurrentActions > 0)
                {
                    Vector3Int retreatCell = FindBestRetreatPosition(target);
                    if (retreatCell != _unit.CurrentCell)
                    {
                        Debug.Log($"→ {gameObject.name} отступает на безопасную позицию: {retreatCell}");
                        yield return StartCoroutine(ExecuteRetreat(retreatCell));
                    }
                    else
                    {
                        Debug.Log($"→ {gameObject.name} не нашел безопасного пути для отступления.");
                    }
                }
            }
        }
        // --- СЦЕНАРИЙ 2: Мы не можем атаковать, но можем двигаться ---
        else if (canMove)
        {
            Vector3Int bestMoveCell = FindBestTacticalMove(target);

            if (bestMoveCell != _unit.CurrentCell)
            {
                Debug.Log($"→ {gameObject.name} занимает агрессивную позицию: {bestMoveCell}");
                yield return StartCoroutine(ExecuteMoveAndAttack(bestMoveCell, target));
            }
            else
            {
                Debug.Log($"→ {gameObject.name} заблокирован и не может улучшить позицию. Пропуск хода.");
            }
        }
        else
        {
            Debug.Log($"→ {gameObject.name} не имеет доступных действий.");
        }

        Debug.Log($"=== {_unit.gameObject.name} завершил ход ===\n");
    }

    // --- МОЗГ ИИ: Выбор цели ---
    private Unit SelectBestTarget()
    {
        Unit bestTarget = null;
        int bestScore = int.MaxValue;

        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit u in allUnits)
        {
            if (!u.IsPlayer || u.CurrentHealth <= 0) continue;

            int distance = GetManhattanDistance(_unit.CurrentCell, u.CurrentCell);
            int score = (distance * 10) + (u.CurrentHealth * 20);

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = u;
            }
        }

        return bestTarget;
    }

    // --- МОЗГ ИИ: Оценка позиции для АТАКИ ---
    private Vector3Int FindBestTacticalMove(Unit target)
    {
        Vector3Int currentCell = _unit.CurrentCell;
        Vector3Int bestCell = currentCell;
        int highestScore = int.MinValue;

        highestScore = EvaluateTacticalPosition(currentCell, target);

        for (int x = -_unit.MaxMoveDistance; x <= _unit.MaxMoveDistance; x++)
        {
            for (int y = -_unit.MaxMoveDistance; y <= _unit.MaxMoveDistance; y++)
            {
                Vector3Int testCell = new Vector3Int(currentCell.x + x, currentCell.y + y, 0);
                if (testCell == currentCell) continue;

                if (_unit.CanMoveTo(testCell))
                {
                    int score = EvaluateTacticalPosition(testCell, target);
                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestCell = testCell;
                    }
                }
            }
        }
        return bestCell;
    }

    private int EvaluateTacticalPosition(Vector3Int cell, Unit target)
    {
        int score = 0;
        int distance = GetManhattanDistance(cell, target.CurrentCell);
        bool canHit = CanActuallyHit(cell, target);

        if (canHit)
        {
            score += 2000;
            if (HasCover(cell, target.CurrentCell)) score += 300;
        }
        else
        {
            if (distance <= (_unit.MaxMoveDistance + _unit.MaxAttackRange))
            {
                score += 500;
            }
            else
            {
                score -= distance * 10;
            }
        }

        if (distance <= 1) score -= 200;

        return score;
    }

    // --- НОВЫЙ МОЗГ ИИ: Оценка позиции для ОТСТУПЛЕНИЯ ---
    private Vector3Int FindBestRetreatPosition(Unit target)
    {
        Vector3Int currentCell = _unit.CurrentCell;
        Vector3Int bestCell = currentCell;
        int highestScore = int.MinValue;

        // Оцениваем текущую клетку как базовый уровень
        highestScore = EvaluateRetreatScore(currentCell, target);

        for (int x = -_unit.MaxMoveDistance; x <= _unit.MaxMoveDistance; x++)
        {
            for (int y = -_unit.MaxMoveDistance; y <= _unit.MaxMoveDistance; y++)
            {
                Vector3Int testCell = new Vector3Int(currentCell.x + x, currentCell.y + y, 0);
                if (testCell == currentCell) continue;

                if (_unit.CanMoveTo(testCell))
                {
                    int score = EvaluateRetreatScore(testCell, target);
                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestCell = testCell;
                    }
                }
            }
        }
        return bestCell;
    }

    private int EvaluateRetreatScore(Vector3Int cell, Unit target)
    {
        int score = 0;
        int distance = GetManhattanDistance(cell, target.CurrentCell);

        // 1. ГЛАВНЫЙ ПРИОРИТЕТ ОТСТУПЛЕНИЯ: Разорвать прямую видимость с игроком
        if (!HasStrictLineOfSight(cell, target.CurrentCell))
        {
            score += 1000; // Огромный бонус за то, что игрок нас больше не видит
        }

        // 2. Чем дальше, тем лучше
        score += distance * 20;

        // 3. Избегать соседства с игроком любой ценой
        if (IsAdjacentToEnemy(cell))
        {
            score -= 500;
        }

        return score;
    }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

    private bool CanActuallyHit(Vector3Int fromCell, Unit target)
    {
        int distance = GetManhattanDistance(fromCell, target.CurrentCell);
        if (distance > _unit.MaxAttackRange || distance == 0) return false;

        if (_unit.AttackType is SimpleShot)
        {
            return HasStrictLineOfSight(fromCell, target.CurrentCell);
        }
        return true;
    }

    private IEnumerator ExecuteMoveAndAttack(Vector3Int moveTarget, Unit attackTarget)
    {
        List<Vector3Int> path = Pathfinding.FindPath(_unit.CurrentCell, moveTarget, _unit.MaxMoveDistance);
        if (path.Count > 0)
        {
            _unit.RequestMove(moveTarget, path);
            while (_unit.IsMoving) yield return null;
            yield return new WaitForSeconds(0.3f);

            if (CanActuallyHit(moveTarget, attackTarget))
            {
                Debug.Log($"→ {gameObject.name} атакует после тактического перемещения!");
                _unit.RequestAttack(attackTarget.CurrentCell);
                yield return new WaitForSeconds(1f);
            }
        }
    }

    // НОВЫЙ МЕТОД: Чистое перемещение без последующей атаки (для отступления)
    private IEnumerator ExecuteRetreat(Vector3Int retreatTarget)
    {
        List<Vector3Int> path = Pathfinding.FindPath(_unit.CurrentCell, retreatTarget, _unit.MaxMoveDistance);
        if (path.Count > 0)
        {
            _unit.RequestMove(retreatTarget, path);
            while (_unit.IsMoving) yield return null;
            yield return new WaitForSeconds(0.3f);
        }
    }

    private bool HasStrictLineOfSight(Vector3Int start, Vector3Int end)
    {
        int steps = Mathf.Max(Mathf.Abs(end.x - start.x), Mathf.Abs(end.y - start.y));
        if (steps == 0) return false;

        for (int i = 1; i < steps; i++)
        {
            float t = (float)i / steps;
            int checkX = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
            int checkY = Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t));
            Vector3Int checkCell = new Vector3Int(checkX, checkY, 0);

            if (GridManager.Instance.IsWall(checkCell)) return false;
            if (GridManager.Instance.GetUnitAtCell(checkCell) != null) return false;
        }
        return true;
    }

    private bool HasCover(Vector3Int myCell, Vector3Int targetCell)
    {
        return !HasStrictLineOfSight(myCell, targetCell);
    }

    private bool IsAdjacentToEnemy(Vector3Int cell)
    {
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.right, Vector3Int.left };
        foreach (Vector3Int dir in directions)
        {
            Unit neighbor = GridManager.Instance.GetUnitAtCell(cell + dir);
            if (neighbor != null && neighbor.IsPlayer)
            {
                return true;
            }
        }
        return false;
    }

    private int GetManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}