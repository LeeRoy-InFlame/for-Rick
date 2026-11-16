using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDetailPanel : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private TextMeshProUGUI _classNameText;

    // Базовый портрет (можно оставить как фон, если у предмета нет paperDollSprite)
    [SerializeField] private Image _largePortraitImage;

    [Header("Paper Doll Slots (UI Images)")] // НОВОЕ: Слоты для наложения экипировки
    [SerializeField] private Image _slotBody;
    [SerializeField] private Image _slotLegs;
    [SerializeField] private Image _slotHead;
    [SerializeField] private Image _slotEars;
    [SerializeField] private Image _slotWeapon;

    [Header("Progression Texts")]
    [SerializeField] private TextMeshProUGUI _availablePointsText;
    [SerializeField] private TextMeshProUGUI _expText;

    [Header("Equipment Texts")]
    [SerializeField] private TextMeshProUGUI _helmetText;
    [SerializeField] private TextMeshProUGUI _armorText;
    [SerializeField] private TextMeshProUGUI _weaponText;

    [Header("Stats Texts")]
    [SerializeField] private TextMeshProUGUI _strengthText;
    [SerializeField] private TextMeshProUGUI _agilityText;
    [SerializeField] private TextMeshProUGUI _enduranceText;
    [SerializeField] private TextMeshProUGUI _intelligenceText;
    [SerializeField] private TextMeshProUGUI _charismaText;
    [SerializeField] private TextMeshProUGUI _luckText;

    [Header("HP/MP Texts")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _mpText;

    private UnitMember _currentMember;

    public void Setup(UnitMember member)
    {
        _currentMember = member;
        RefreshUI();

        // Показываем превью юнита
        UnitPreviewManager previewManager = FindFirstObjectByType<UnitPreviewManager>();
        if (previewManager != null)
        {
            previewManager.ShowUnit(member);
        }
    }

    public void RefreshUI()
    {
        if (_currentMember == null) return;

        // 1. Тексты
        if (_classNameText != null) _classNameText.text = $"{_currentMember.ClassName} \"{_currentMember.Name}\" (Ур. {_currentMember.Level})";

        if (_availablePointsText != null)
        {
            int points = _currentMember.AvailableStatPoints;
            _availablePointsText.text = $"Свободные очки: {points}";
            _availablePointsText.color = points > 0 ? Color.red : Color.white;
        }

        if (_expText != null)
        {
            _expText.text = $"Опыт: {_currentMember.CurrentExp} / {_currentMember.GetExpToNextLevel()}";
        }

        if (_helmetText != null) _helmetText.text = $"Голова: {_currentMember.HelmetName}";
        if (_armorText != null) _armorText.text = $"Тело: {_currentMember.ArmorName}";
        if (_weaponText != null) _weaponText.text = $"Оружие: {_currentMember.WeaponName}";

        if (_strengthText != null) _strengthText.text = $"Сила: {_currentMember.Strength}";
        if (_agilityText != null) _agilityText.text = $"Ловкость: {_currentMember.Agility}";
        if (_enduranceText != null) _enduranceText.text = $"Выносливость: {_currentMember.Endurance}";
        if (_intelligenceText != null) _intelligenceText.text = $"Интеллект: {_currentMember.Intelligence}";
        if (_charismaText != null) _charismaText.text = $"Харизма: {_currentMember.Charisma}";
        if (_luckText != null) _luckText.text = $"Удача: {_currentMember.Luck}";

        if (_hpText != null) _hpText.text = $"HP: {_currentMember.MaxHealth}";
        if (_mpText != null) _mpText.text = $"MP: {_currentMember.MaxMana}";

        // 2. ВИЗУАЛИЗАЦИЯ "БУМАЖНОЙ КУКЛЫ"
        UpdatePaperDoll();
    }

    private void UpdatePaperDoll()
    {
        // Сначала очищаем все слоты (убираем старую экипировку)
        if (_slotBody != null) _slotBody.sprite = null;
        if (_slotLegs != null) _slotLegs.sprite = null;
        if (_slotHead != null) _slotHead.sprite = null;
        if (_slotEars != null) _slotEars.sprite = null;
        if (_slotWeapon != null) _slotWeapon.sprite = null;

        // Если у нас есть базовый портрет, показываем его как фон (опционально)
        if (_largePortraitImage != null && _currentMember.LargePortraitSprite != null)
        {
            _largePortraitImage.sprite = _currentMember.LargePortraitSprite;
            _largePortraitImage.enabled = true;
        }

        // Теперь "надеваем" предметы, если они экипированы
        EquipmentData head = _currentMember.GetEquippedItem(EquipmentSlot.Head);
        if (head != null && head.paperDollSprite != null && _slotHead != null)
            _slotHead.sprite = head.paperDollSprite;

        EquipmentData body = _currentMember.GetEquippedItem(EquipmentSlot.Body);
        if (body != null && body.paperDollSprite != null && _slotBody != null)
            _slotBody.sprite = body.paperDollSprite;

        EquipmentData weapon = _currentMember.GetEquippedItem(EquipmentSlot.Weapon);
        if (weapon != null && weapon.paperDollSprite != null && _slotWeapon != null)
            _slotWeapon.sprite = weapon.paperDollSprite;

        EquipmentData ears = _currentMember.GetEquippedItem(EquipmentSlot.Ears);
        if (ears != null && ears.paperDollSprite != null && _slotEars != null)
            _slotEars.sprite = ears.paperDollSprite;

        EquipmentData legs = _currentMember.GetEquippedItem(EquipmentSlot.Legs);
        if (legs != null && legs.paperDollSprite != null && _slotLegs != null)
            _slotLegs.sprite = legs.paperDollSprite;
    }

    // НОВОЕ: Публичный геттер для получения текущего выбранного юнита
    public UnitMember GetCurrentMember()
    {
        return _currentMember;
    }

    // НОВОЕ: Метод для экипировки предмета на текущего юнита
    public void EquipItemOnCurrentMember(EquipmentData item)
    {
        Debug.Log($"[DEBUG 1] Начало экипировки: {item.itemName} на {_currentMember?.Name}");

        if (_currentMember == null || item == null)
        {
            Debug.LogWarning("Нельзя экипировать: нет выбранного юнита или предмета!");
            return;
        }

        // 1. Снимаем старый предмет
        EquipmentData oldItem = _currentMember.GetEquippedItem(item.slot);
        if (oldItem != null)
        {
            _currentMember.UnequipItem(item.slot);
            PartyInventory.Instance.AddItem(oldItem);
        }

        // 2. Надеваем новый
        _currentMember.EquipItem(item);
        PartyInventory.Instance.RemoveItem(item);

        Debug.Log($"[DEBUG 2] Данные UnitMember обновлены. Ищем UnitPreviewManager...");

        // 3. Принудительно обновляем превью
        UnitPreviewManager previewManager = FindFirstObjectByType<UnitPreviewManager>();

        if (previewManager != null)
        {
            Debug.Log($"[DEBUG 3] UnitPreviewManager НАЙДЕН! Вызываем ShowUnitPreview...");
            previewManager.ShowUnit(_currentMember);
        }
        else
        {
            Debug.LogError("[DEBUG 3] UnitPreviewManager НЕ НАЙДЕН! Проверь, есть ли объект с этим скриптом в сцене и активен ли он.");
        }

        // 4. Обновляем тексты
        Debug.Log($"[DEBUG 4] Вызываем RefreshUI...");
        RefreshUI();
    }

    public void OnPlusButtonClicked(StatType stat)
    {
        if (_currentMember != null)
        {
            if (_currentMember.SpendPoint(stat))
            {
                RefreshUI();
            }
        }
    }
}