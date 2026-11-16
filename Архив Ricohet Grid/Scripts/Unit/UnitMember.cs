using UnityEngine;

public enum StatType
{
    Strength, Agility, Endurance, Intelligence, Charisma, Luck
}

[System.Serializable]
public class UnitMember
{
    [Header("Base Reference")]
    [SerializeField] private UnitData _unitData;

    [Header("Identity")]
    [SerializeField] private string _customName;
    [SerializeField] private string _className;
    [SerializeField] private Sprite _portraitSprite;
    [SerializeField] private Sprite _largePortraitSprite;

    [Header("Equipment")]
    // ТЕПЕРЬ ЭТО ССЫЛКИ НА АССЕТЫ, А НЕ ПРОСТО СТРОКИ!
    [SerializeField] private EquipmentData _equippedHead;
    [SerializeField] private EquipmentData _equippedBody;
    [SerializeField] private EquipmentData _equippedWeapon;
    [SerializeField] private EquipmentData _equippedEars;
    [SerializeField] private EquipmentData _equippedLegs;

    [Header("Base RPG Stats")] // Базовые статы (без экипировки)
    [SerializeField] private int _baseStrength;
    [SerializeField] private int _baseAgility;
    [SerializeField] private int _baseEndurance;
    [SerializeField] private int _baseIntelligence;
    [SerializeField] private int _baseCharisma;
    [SerializeField] private int _baseLuck;

    [Header("Base Combat Stats")]
    [SerializeField] private int _baseMaxHealth;
    [SerializeField] private int _baseMaxMana;
    [SerializeField] private int _maxMoveDistance;
    [SerializeField] private int _maxAttackRange;
    [SerializeField] private int _maxActions;

    [Header("Progression")]
    [SerializeField] private int _level = 1;
    [SerializeField] private int _currentExp = 0;
    [SerializeField] private int _availableStatPoints = 0;
    [SerializeField] private bool _isInitialized = false;

    [Header("Progression Settings")]
    [SerializeField] private int _baseExpRequired = 100;
    [SerializeField] private float _expGrowthRate = 1.5f;

    // --- Публичные свойства (Getters) ---
    public UnitData Data { get { return _unitData; } }
    public string Name { get { return string.IsNullOrEmpty(_customName) ? (_unitData != null ? _unitData.TemplateName : "Безымянный") : _customName; } }
    public string ClassName { get { return _className; } }
    public Sprite PortraitSprite { get { return _portraitSprite; } }
    public Sprite LargePortraitSprite { get { return _largePortraitSprite != null ? _largePortraitSprite : _portraitSprite; } }

    // Совместимость со старым UI: возвращаем имена предметов или "Пусто"
    public string HelmetName { get { return _equippedHead != null ? _equippedHead.itemName : "Пусто"; } }
    public string ArmorName { get { return _equippedBody != null ? _equippedBody.itemName : "Пусто"; } }
    public string WeaponName { get { return _equippedWeapon != null ? _equippedWeapon.itemName : "Пусто"; } }

    // ДИНАМИЧЕСКИЕ СТАТЫ: База + Бонусы экипировки
    public int Strength { get { return _baseStrength + GetEquipmentBonus(StatType.Strength); } }
    public int Agility { get { return _baseAgility + GetEquipmentBonus(StatType.Agility); } }
    public int Endurance { get { return _baseEndurance + GetEquipmentBonus(StatType.Endurance); } }
    public int Intelligence { get { return _baseIntelligence + GetEquipmentBonus(StatType.Intelligence); } }
    public int Charisma { get { return _baseCharisma + GetEquipmentBonus(StatType.Charisma); } }
    public int Luck { get { return _baseLuck + GetEquipmentBonus(StatType.Luck); } }

    public int MaxHealth { get { return _baseMaxHealth + GetEquipmentHealthBonus(); } }
    public int MaxMana { get { return _baseMaxMana + GetEquipmentManaBonus(); } }

    public int MaxMoveDistance { get { return _maxMoveDistance; } }
    public int MaxAttackRange { get { return _maxAttackRange; } }
    public int MaxActions { get { return _maxActions; } }

    public int Level { get { return _level; } }
    public int CurrentExp { get { return _currentExp; } }
    public int AvailableStatPoints { get { return _availableStatPoints; } }
    public bool IsInitialized { get { return _isInitialized; } }

    // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ РАСЧЕТА БОНУСОВ ---
    private int GetEquipmentBonus(StatType stat)
    {
        int bonus = 0;
        EquipmentData[] allGear = { _equippedHead, _equippedBody, _equippedWeapon, _equippedEars, _equippedLegs };
        foreach (var gear in allGear)
        {
            if (gear == null) continue;
            switch (stat)
            {
                case StatType.Strength: bonus += gear.strengthBonus; break;
                case StatType.Agility: bonus += gear.agilityBonus; break;
                case StatType.Endurance: bonus += gear.enduranceBonus; break;
                case StatType.Intelligence: bonus += gear.intelligenceBonus; break;
                case StatType.Charisma: bonus += gear.charismaBonus; break;
                case StatType.Luck: bonus += gear.luckBonus; break;
            }
        }
        return bonus;
    }

    private int GetEquipmentHealthBonus()
    {
        int bonus = 0;
        EquipmentData[] allGear = { _equippedHead, _equippedBody, _equippedWeapon, _equippedEars, _equippedLegs };
        foreach (var gear in allGear) { if (gear != null) bonus += gear.maxHealthBonus; }
        return bonus;
    }

    private int GetEquipmentManaBonus()
    {
        int bonus = 0;
        EquipmentData[] allGear = { _equippedHead, _equippedBody, _equippedWeapon, _equippedEars, _equippedLegs };
        foreach (var gear in allGear) { if (gear != null) bonus += gear.maxManaBonus; }
        return bonus;
    }

    // --- ОСНОВНАЯ ЛОГИКА ---

    public void Initialize(UnitData baseData)
    {
        if (_isInitialized) return;

        _unitData = baseData;
        _className = baseData.ClassName;
        _portraitSprite = baseData.PortraitSprite;
        _largePortraitSprite = baseData.LargePortraitSprite;

        // Инициализируем именно БАЗОВЫЕ статы
        _baseStrength = baseData.Strength;
        _baseAgility = baseData.Agility;
        _baseEndurance = baseData.Endurance;
        _baseIntelligence = baseData.Intelligence;
        _baseCharisma = baseData.Charisma;
        _baseLuck = baseData.Luck;

        _baseMaxHealth = baseData.MaxHealth;
        _baseMaxMana = baseData.MaxMana;
        _maxMoveDistance = baseData.MaxMoveDistance;
        _maxAttackRange = baseData.MaxAttackRange;
        _maxActions = baseData.MaxActions;

        _isInitialized = true;
    }

    public void LevelUp()
    {
        _level++;
        _availableStatPoints += 1;
        Debug.Log($"{Name} получил {_level} уровень! Доступно очков характеристик: {_availableStatPoints}");
    }

    public void AddExp(int expAmount)
    {
        _currentExp += expAmount;
        int expToNextLevel = GetExpToNextLevel();

        while (_currentExp >= expToNextLevel)
        {
            _currentExp -= expToNextLevel;
            LevelUp();
            expToNextLevel = GetExpToNextLevel();
        }
    }

    public bool SpendPoint(StatType stat)
    {
        if (_availableStatPoints <= 0)
        {
            Debug.LogWarning("Нет доступных очков характеристик!");
            return false;
        }

        _availableStatPoints--;

        // Увеличиваем именно БАЗОВЫЕ статы, бонусы от шмоток прибавятся сверху автоматически
        switch (stat)
        {
            case StatType.Strength:
                _baseStrength++;
                _baseMaxHealth += 2;
                break;
            case StatType.Agility: _baseAgility++; break;
            case StatType.Endurance:
                _baseEndurance++;
                _baseMaxHealth += 5;
                break;
            case StatType.Intelligence: _baseIntelligence++; break;
            case StatType.Charisma: _baseCharisma++; break;
            case StatType.Luck: _baseLuck++; break;
        }

        Debug.Log($"{Name} вложил очко в {stat}. Осталось очков: {_availableStatPoints}");
        return true;
    }

    // --- МЕТОДЫ ЭКИПИРОВКИ ---

    public void EquipItem(EquipmentData item)
    {
        if (item == null) return;

        // Сначала снимаем старый предмет из этого слота, если он есть
        UnequipItem(item.slot);

        // Надеваем новый
        switch (item.slot)
        {
            case EquipmentSlot.Head: _equippedHead = item; break;
            case EquipmentSlot.Body: _equippedBody = item; break;
            case EquipmentSlot.Weapon: _equippedWeapon = item; break;
            case EquipmentSlot.Ears: _equippedEars = item; break;
            case EquipmentSlot.Legs: _equippedLegs = item; break;
        }

        Debug.Log($"{Name} экипировал: {item.itemName}");
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Head: _equippedHead = null; break;
            case EquipmentSlot.Body: _equippedBody = null; break;
            case EquipmentSlot.Weapon: _equippedWeapon = null; break;
            case EquipmentSlot.Ears: _equippedEars = null; break;
            case EquipmentSlot.Legs: _equippedLegs = null; break;
        }
    }

    public EquipmentData GetEquippedItem(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Head: return _equippedHead;
            case EquipmentSlot.Body: return _equippedBody;
            case EquipmentSlot.Weapon: return _equippedWeapon;
            case EquipmentSlot.Ears: return _equippedEars;
            case EquipmentSlot.Legs: return _equippedLegs;
            default: return null;
        }
    }

    public int GetExpToNextLevel()
    {
        return Mathf.RoundToInt(_baseExpRequired * Mathf.Pow(_expGrowthRate, _level - 1));
    }
}