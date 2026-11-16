using System.Collections.Generic;
using UnityEngine;

public class PartyInventory : MonoBehaviour
{
    public static PartyInventory Instance { get; private set; }

    [Header("Starting Items")]
    [SerializeField] private List<EquipmentData> _startingItems = new List<EquipmentData>();

    private List<EquipmentData> _inventoryItems = new List<EquipmentData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Копируем стартовые предметы в рабочий инвентарь
            _inventoryItems = new List<EquipmentData>(_startingItems);
            Debug.Log($"Инвентарь инициализирован. Предметов: {_inventoryItems.Count}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(EquipmentData item)
    {
        if (item != null && !_inventoryItems.Contains(item))
        {
            _inventoryItems.Add(item);
            Debug.Log($"Добавлено в инвентарь: {item.itemName}");
        }
    }

    public void RemoveItem(EquipmentData item)
    {
        if (_inventoryItems.Contains(item))
        {
            _inventoryItems.Remove(item);
            Debug.Log($"Удалено из инвентаря: {item.itemName}");
        }
    }

    public List<EquipmentData> GetAllItems()
    {
        return _inventoryItems;
    }

    // Для тестов: добавить предмет по клику в инспекторе
    [ContextMenu("Add Test Item")]
    private void AddTestItem()
    {
        // Создай в Unity любой EquipmentData и перетащи его сюда для теста, 
        // или просто добавь его в _startingItems в инспекторе.
    }
}