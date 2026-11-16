using UnityEngine;

public class EquipmentVisualizer : MonoBehaviour
{
    [Header("Equipment Slots (Empty GameObjects)")]
    [SerializeField] private Transform _headSlot;
    [SerializeField] private Transform _bodySlot;
    [SerializeField] private Transform _earsSlot;
    [SerializeField] private Transform _legsSlot;
    [SerializeField] private Transform _weaponSlot;

    [Header("Settings")]
    // Имя слоя, который видит камера превью и основная камера
    [SerializeField] private string _targetLayerName = "Player";

    // Кэшируем текущие объекты в слотах
    private GameObject _currentHead;
    private GameObject _currentBody;
    private GameObject _currentEars;
    private GameObject _currentLegs;
    private GameObject _currentWeapon;

    /// <summary>
    /// Обновляет визуализацию всей экипировки на основе UnitMember
    /// </summary>
    public void ApplyEquipment(UnitMember member)
    {
        if (member == null) return;

        ApplyItemToSlot(member.GetEquippedItem(EquipmentSlot.Head), _headSlot, ref _currentHead);
        ApplyItemToSlot(member.GetEquippedItem(EquipmentSlot.Body), _bodySlot, ref _currentBody);
        ApplyItemToSlot(member.GetEquippedItem(EquipmentSlot.Ears), _earsSlot, ref _currentEars);
        ApplyItemToSlot(member.GetEquippedItem(EquipmentSlot.Legs), _legsSlot, ref _currentLegs);
        ApplyItemToSlot(member.GetEquippedItem(EquipmentSlot.Weapon), _weaponSlot, ref _currentWeapon);
    }

    /// <summary>
    /// Универсальный метод: очищает слот и инстанцирует новый предмет
    /// </summary>
    private void ApplyItemToSlot(EquipmentData item, Transform slot, ref GameObject currentObject)
    {
        if (slot == null) return;

        // ЕСЛИ ЭКИПИРОВКИ НЕТ - НИЧЕГО НЕ ДЕЛАЕМ! Оставляем базовые части тела.
        if (item == null)
        {
            // Но если был закешированный объект (старая экипировка), удаляем его
            if (currentObject != null)
            {
                Destroy(currentObject);
                currentObject = null;
            }
            return;
        }

        // Если экипировка есть, но нет префаба - ошибка
        if (item.prefab == null)
        {
            Debug.LogError($"[Visualizer] У предмета '{item.itemName}' НЕ НАЗНАЧЕН префаб!");
            return;
        }

        // 1. ПОЛНАЯ ОЧИСТКА СЛОТА (только если надеваем новую экипировку!)
        if (currentObject != null)
        {
            Destroy(currentObject);
            currentObject = null;
        }

        // Удаляем базовые части тела
        for (int i = slot.childCount - 1; i >= 0; i--)
        {
            Destroy(slot.GetChild(i).gameObject);
        }

        // 2. Инстанцируем экипировку
        currentObject = Instantiate(item.prefab, slot);

        currentObject.transform.localPosition = Vector3.zero;
        currentObject.transform.localRotation = Quaternion.identity;
        currentObject.transform.localScale = Vector3.one;

        // 3. Меняем слой
        SetLayerRecursive(currentObject, _targetLayerName);

        Debug.Log($"[Visualizer] ✅ Надето: {item.itemName} в слот {slot.name} (Layer: {_targetLayerName})");
    }

    /// <summary>
    /// Рекурсивно меняет слой у объекта и всех его детей
    /// </summary>
    private void SetLayerRecursive(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogWarning($"[Visualizer] Слой '{layerName}' не найден! Проверь настройки проекта.");
            return;
        }

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layerName);
        }
    }
}