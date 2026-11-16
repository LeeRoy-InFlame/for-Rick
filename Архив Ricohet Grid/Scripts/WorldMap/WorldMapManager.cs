using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    [SerializeField] private PartyController _partyController;
    [SerializeField] private LayerMask _nodeLayerMask;
    [SerializeField] private List<PathConnection> _allPaths = new List<PathConnection>();

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_partyController.IsMoving)
        {
            HandleMapClick();
        }
    }

    private void HandleMapClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100f, _nodeLayerMask);

        if (hit.collider != null)
        {
            WorldMapNode targetNode = hit.collider.GetComponent<WorldMapNode>();

            if (targetNode != null && targetNode != _partyController.CurrentNode)
            {
                // 1. Сначала ищем логический путь (цепочку узлов)
                List<WorldMapNode> nodePath = FindShortestPath(_partyController.CurrentNode, targetNode);

                if (nodePath != null)
                {
                    Debug.Log($"Маршрут построен! Узлов в пути: {nodePath.Count}");

                    // 2. Преобразуем логический путь узлов в визуальные точки для движения
                    List<Vector3> fullWaypoints = BuildVisualWaypoints(nodePath);

                    // 3. Отправляем кролика в путь
                    _partyController.TravelTo(fullWaypoints, targetNode);
                }
                else
                {
                    Debug.Log("К сожалению, до этой точки нет дороги.");
                }
            }
        }
    }

    // --- АЛГОРИТМ ПОИСКА ПУТИ (BFS - Поиск в ширину) ---
    private List<WorldMapNode> FindShortestPath(WorldMapNode start, WorldMapNode target)
    {
        if (start == target) return new List<WorldMapNode> { start };

        Queue<WorldMapNode> queue = new Queue<WorldMapNode>();
        Dictionary<WorldMapNode, WorldMapNode> cameFrom = new Dictionary<WorldMapNode, WorldMapNode>();
        HashSet<WorldMapNode> visited = new HashSet<WorldMapNode>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            WorldMapNode current = queue.Dequeue();

            // Если дошли до цели, восстанавливаем путь
            if (current == target)
            {
                List<WorldMapNode> path = new List<WorldMapNode>();
                WorldMapNode step = target;

                while (step != start)
                {
                    path.Add(step);
                    step = cameFrom[step];
                }
                path.Add(start);
                path.Reverse(); // Теперь путь идет от Start к Target
                return path;
            }

            // Проверяем всех соседей текущего узла
            foreach (WorldMapNode neighbor in current.ConnectedNodes)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null; // Путь не найден
    }

    // --- СБОРКА ВИЗУАЛЬНЫХ ТОЧЕК ---
    private List<Vector3> BuildVisualWaypoints(List<WorldMapNode> nodePath)
    {
        List<Vector3> fullWaypoints = new List<Vector3>();

        for (int i = 0; i < nodePath.Count - 1; i++)
        {
            WorldMapNode current = nodePath[i];
            WorldMapNode next = nodePath[i + 1];

            // Находим тропинку между этими двумя конкретными узлами
            PathConnection connection = FindPathConnection(current, next);

            if (connection != null)
            {
                Vector3[] segmentWaypoints = connection.GetWaypoints();

                // Если мы идем "назад" по тропинке (от NodeB к NodeA), разворачиваем точки
                if (connection.NodeA == next)
                {
                    Array.Reverse(segmentWaypoints);
                }

                // Добавляем точки в общий маршрут, избегая дубликатов на стыках узлов
                foreach (Vector3 wp in segmentWaypoints)
                {
                    if (fullWaypoints.Count == 0 || Vector3.Distance(fullWaypoints[fullWaypoints.Count - 1], wp) > 0.01f)
                    {
                        fullWaypoints.Add(wp);
                    }
                }
            }
        }

        return fullWaypoints;
    }

    // Вспомогательный метод для поиска конкретной тропинки
    private PathConnection FindPathConnection(WorldMapNode node1, WorldMapNode node2)
    {
        foreach (PathConnection path in _allPaths)
        {
            if ((path.NodeA == node1 && path.NodeB == node2) ||
                (path.NodeA == node2 && path.NodeB == node1))
            {
                return path;
            }
        }
        return null;
    }
}