using UnityEngine;

public class UnitPreviewManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Camera _previewCamera;

    [Header("Preview Settings")]
    [SerializeField] private float _previewScaleMultiplier = 1.5f;

    private GameObject _currentUnit;

    private void Awake()
    {
        ClearPreview();
    }

    public void ShowUnit(UnitMember member)
    {
        Debug.Log($"[PREVIEW DEBUG 1] Вызван ShowUnitPreview для: {member?.Name}");

        if (member == null || member.Data == null || member.Data.UnitPrefab == null)
        {
            Debug.LogWarning("[PREVIEW DEBUG 2] Нет данных или префаба для превью!");
            ClearPreview();
            return;
        }

        ClearPreview();

        Debug.Log($"[PREVIEW DEBUG 3] Инстанцируем префаб: {member.Data.UnitPrefab.name}");
        _currentUnit = Instantiate(member.Data.UnitPrefab, _spawnPoint.position, _spawnPoint.rotation);
        _currentUnit.transform.localScale *= _previewScaleMultiplier;

        EquipmentVisualizer visualizer = _currentUnit.GetComponent<EquipmentVisualizer>();
        if (visualizer != null)
        {
            Debug.Log($"[PREVIEW DEBUG 4] Визуализатор найден на клоне! Применяем экипировку...");
            visualizer.ApplyEquipment(member);
        }
        else
        {
            Debug.LogError($"[PREVIEW DEBUG 4] ОШИБКА: На префабе {member.Data.UnitPrefab.name} НЕТ компонента EquipmentVisualizer!");
        }
    }

    private void ClearPreview()
    {
        if (_currentUnit != null)
        {
            Destroy(_currentUnit);
            _currentUnit = null;
        }
    }

    private void OnDestroy()
    {
        ClearPreview();
    }
}