using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Blueprint")]
    [SerializeField] private string _templateName;     // Название класса для редактора (например, "Стрелок")
    [SerializeField] private GameObject _unitPrefab;   // Единственное, что действительно важно для спавна
    [SerializeField] private AttackBase _defaultAttackType; // Базовая атака

    // --- Поля ниже нужны ТОЛЬКО для того, чтобы UnitMember мог скопировать их при создании ---
    [Header("Base Defaults (Copied to Member)")]
    [SerializeField] private string _className;
    [SerializeField] private Sprite _portraitSprite;
    [SerializeField] private Sprite _largePortraitSprite;
    [SerializeField] private string _helmetName;
    [SerializeField] private string _armorName;
    [SerializeField] private string _weaponName;
    [SerializeField] private int _strength;
    [SerializeField] private int _agility;
    [SerializeField] private int _endurance;
    [SerializeField] private int _intelligence;
    [SerializeField] private int _charisma;
    [SerializeField] private int _luck;
    [SerializeField] private int _maxHealth = 1;
    [SerializeField] private int _maxMana = 0;
    [SerializeField] private int _maxMoveDistance = 3;
    [SerializeField] private int _maxAttackRange = 5;
    [SerializeField] private int _maxActions = 2;

    // --- Публичные свойства (Getters) ---
    public string TemplateName { get { return _templateName; } }
    public GameObject UnitPrefab { get { return _unitPrefab; } }
    public AttackBase DefaultAttackType { get { return _defaultAttackType; } }

    // Геттеры для базовых значений (используются только при инициализации UnitMember)
    public string ClassName { get { return _className; } }
    public Sprite PortraitSprite { get { return _portraitSprite; } }
    public Sprite LargePortraitSprite { get { return _largePortraitSprite != null ? _largePortraitSprite : _portraitSprite; } }
    public string HelmetName { get { return _helmetName; } }
    public string ArmorName { get { return _armorName; } }
    public string WeaponName { get { return _weaponName; } }
    public int Strength { get { return _strength; } }
    public int Agility { get { return _agility; } }
    public int Endurance { get { return _endurance; } }
    public int Intelligence { get { return _intelligence; } }
    public int Charisma { get { return _charisma; } }
    public int Luck { get { return _luck; } }
    public int MaxHealth { get { return _maxHealth; } }
    public int MaxMana { get { return _maxMana; } }
    public int MaxMoveDistance { get { return _maxMoveDistance; } }
    public int MaxAttackRange { get { return _maxAttackRange; } }
    public int MaxActions { get { return _maxActions; } }
}