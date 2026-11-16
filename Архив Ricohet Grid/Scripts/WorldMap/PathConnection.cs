using UnityEngine;

public class PathConnection : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private WorldMapNode _nodeA;
    [SerializeField] private WorldMapNode _nodeB;

    [Header("Visuals")]
    [SerializeField] private LineRenderer _lineRenderer;

    // --- Свойства ---
    public WorldMapNode NodeA { get { return _nodeA; } }
    public WorldMapNode NodeB { get { return _nodeB; } }

    // Отдает все точки изгиба тропинки
    public Vector3[] GetWaypoints()
    {
        if (_lineRenderer == null || _lineRenderer.positionCount == 0)
        {
            // Если линии нет, возвращаем просто прямую между узлами
            return new Vector3[] { _nodeA.transform.position, _nodeB.transform.position };
        }

        Vector3[] positions = new Vector3[_lineRenderer.positionCount];
        _lineRenderer.GetPositions(positions);
        return positions;
    }
}