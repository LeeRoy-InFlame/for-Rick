using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform _inventoryGridParent;
    [SerializeField] private InventoryItemUI _itemPrefab;

    [Header("Links")]
    [SerializeField] private CharacterDetailPanel _characterDetailPanel; // Ссылка на среднюю панель!

    private void Start()
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        // Очищаем старую сетку
        foreach (Transform child in _inventoryGridParent)
            Destroy(child.gameObject);

        // Получаем все предметы из инвентаря
        List<EquipmentData> items = PartyInventory.Instance.GetAllItems();

        // Создаем ячейки
        foreach (EquipmentData item in items)
        {
            InventoryItemUI newItemUI = Instantiate(_itemPrefab, _inventoryGridParent);
            newItemUI.Setup(item, this); // Передаем ссылку на эту панель
        }
    }

    // Вызывается когда кликнули на предмет
    public void OnItemClicked(EquipmentData item)
    {
        if (_characterDetailPanel == null)
        {
            Debug.LogError("InventoryPanel: Не назначена ссылка на CharacterDetailPanel!");
            return;
        }

        // Экипируем на текущего выбранного юнита
        _characterDetailPanel.EquipItemOnCurrentMember(item);

        // Обновляем инвентарь (предмет исчезнет из списка)
        RefreshInventory();
    }
}