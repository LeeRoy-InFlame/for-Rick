using UnityEngine;

public class MouseTester : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3Int cellPos = GridManager.Instance.GetCellFromMouse();
            Debug.Log($"Клик по клетке: X={cellPos.x}, Y={cellPos.y}");

            Vector3 worldPos = GridManager.Instance.GetWorldPositionFromCell(cellPos);
            Debug.Log($"Мировая позиция: {worldPos}"); // Z будет 0
        }
    }
}