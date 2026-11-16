using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    // Поиск пути с учетом РЕАЛЬНОГО количества шагов (а не геометрического расстояния)
    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int end, int maxSteps, bool allowTargetOccupied = false)
    {
        if (start == end) return new List<Vector3Int>();

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        queue.Enqueue(start);

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, int> stepsTaken = new Dictionary<Vector3Int, int>(); // Считаем реальные шаги!

        cameFrom[start] = start;
        stepsTaken[start] = 0;

        Vector3Int[] directions = {
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0)
        };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (current == end)
            {
                return ReconstructPath(cameFrom, start, end);
            }

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighbor = current + dir;

                // Специальное правило для атаки: цель может быть занята
                if (neighbor == end && allowTargetOccupied)
                {
                    if (stepsTaken[current] + 1 <= maxSteps)
                    {
                        cameFrom[neighbor] = current;
                        return ReconstructPath(cameFrom, start, end);
                    }
                    continue;
                }

                // Обычная проверка проходимости
                if (!GridManager.Instance.IsCellWalkable(neighbor)) continue;
                if (stepsTaken.ContainsKey(neighbor)) continue; // Уже посещали

                int currentSteps = stepsTaken[current] + 1;

                // СТРОГОЕ ОГРАНИЧЕНИЕ: если шагов больше, чем максимум, не идем туда
                if (currentSteps > maxSteps) continue;

                cameFrom[neighbor] = current;
                stepsTaken[neighbor] = currentSteps;
                queue.Enqueue(neighbor);
            }
        }

        return new List<Vector3Int>(); // Путь не найден
    }

    // Новый метод: возвращает ВСЕ клетки, до которых можно дойти за N шагов (для подсветки UI)
    public static List<Vector3Int> GetReachableCells(Vector3Int start, int maxSteps)
    {
        List<Vector3Int> reachable = new List<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        queue.Enqueue(start);

        Dictionary<Vector3Int, int> stepsTaken = new Dictionary<Vector3Int, int>();
        stepsTaken[start] = 0;

        Vector3Int[] directions = {
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0)
        };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            // Добавляем в список, если это не стартовая клетка
            if (current != start)
            {
                reachable.Add(current);
            }

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighbor = current + dir;

                if (!GridManager.Instance.IsCellWalkable(neighbor)) continue;
                if (stepsTaken.ContainsKey(neighbor)) continue;

                int currentSteps = stepsTaken[current] + 1;
                if (currentSteps > maxSteps) continue;

                stepsTaken[neighbor] = currentSteps;
                queue.Enqueue(neighbor);
            }
        }

        return reachable;
    }

    private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = end;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }
}