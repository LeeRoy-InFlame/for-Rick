using UnityEngine;

public class DebugPositions : MonoBehaviour
{
    void Update()
    {
        // Нажми клавишу "P" для вывода информации о всех юнитах
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintAllUnitPositions();
        }
    }

    private void PrintAllUnitPositions()
    {
        Debug.Log("========== ПОЗИЦИИ ВСЕХ ЮНИТОВ ==========");

        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        foreach (Unit unit in allUnits)
        {
            Debug.Log($"--- {unit.name} ---");
            Debug.Log($"  Логическая клетка (_currentCell): {unit.CurrentCell}");
            Debug.Log($"  Реальная позиция (transform.position): {unit.transform.position}");
            Debug.Log($"  Ожидается центр клетки: {GridManager.Instance.GetWorldPositionFromCell(unit.CurrentCell)}");

            // Проверяем, совпадает ли реальная позиция с центром клетки
            Vector3 expectedPos = GridManager.Instance.GetWorldPositionFromCell(unit.CurrentCell);
            float distance = Vector3.Distance(unit.transform.position, expectedPos);

            if (distance < 0.01f)
            {
                Debug.Log($"  ✓ Позиция корректна (расхождение: {distance:F3})");
            }
            else
            {
                Debug.LogError($"  ✗ ПОЗИЦИЯ НЕКОРРЕКТНА! Расхождение: {distance:F3}");
            }

            Debug.Log("");
        }

        Debug.Log("==========================================");
    }
}