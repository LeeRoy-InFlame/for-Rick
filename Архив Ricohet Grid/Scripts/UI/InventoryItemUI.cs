using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Button _itemButton;

    private EquipmentData _itemData;
    private InventoryPanel _inventoryPanel;

    private void Awake()
    {
        if (_itemButton != null)
        {
            _itemButton.onClick.AddListener(OnItemClicked);
        }
    }

    // ИЗМЕНЕНО: Теперь принимаем ссылку на панель
    public void Setup(EquipmentData data, InventoryPanel inventoryPanel)
    {
        _itemData = data;
        _inventoryPanel = inventoryPanel;

        if (_iconImage != null)
        {
            _iconImage.sprite = data.icon;
            _iconImage.enabled = data.icon != null;
        }

        if (_nameText != null)
        {
            _nameText.text = data.itemName;
        }
    }

    private void OnItemClicked()
    {
        if (_itemData != null && _inventoryPanel != null)
        {
            _inventoryPanel.OnItemClicked(_itemData);
        }
    }
}