using System.Collections.Generic;
using UnityEngine;

public class PlayerRoster : MonoBehaviour
{
    public static PlayerRoster Instance { get; private set; }

    [Header("Roster Settings")]
    [SerializeField] private List<UnitMember> _rosterMembers = new List<UnitMember>();
    [SerializeField] private int _maxDeploymentCount = 4;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // НОВОЕ: Инициализируем всех членов отряда при старте
            InitializeRoster();

            Debug.Log("PlayerRoster инициализирован и будет путешествовать между сценами.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // НОВОЕ: Метод инициализации ростера
    private void InitializeRoster()
    {
        Debug.Log($"=== ПРОВЕРКА РОСТЕРА === Найдено элементов: {_rosterMembers.Count}");

        for (int i = 0; i < _rosterMembers.Count; i++)
        {
            UnitMember member = _rosterMembers[i];

            // Рентген: показываем, есть ли у элемента данные
            if (member.Data == null)
            {
                Debug.LogError($"❌ Элемент #{i} в PlayerRoster ПУСТОЙ! Ты забыл перетащить UnitData в Инспекторе!");
            }
            else if (!member.IsInitialized)
            {
                member.Initialize(member.Data);
                Debug.Log($"✅ Элемент #{i} инициализирован: {member.Name} (HP: {member.MaxHealth})");
            }
            else
            {
                Debug.Log($"✅ Элемент #{i} уже был инициализирован: {member.Name}");
            }
        }
        Debug.Log($"=== КОНЕЦ ПРОВЕРКИ РОСТЕРА ===");
    }

    public List<UnitMember> GetRosterMembers()
    {
        return _rosterMembers;
    }

    public int GetMaxDeploymentCount()
    {
        return _maxDeploymentCount;
    }

    public void AddUnitToRoster(UnitData baseData)
    {
        if (baseData == null)
        {
            Debug.LogError("Попытка добавить в отряд пустой UnitData!");
            return;
        }

        UnitMember newMember = new UnitMember();
        newMember.Initialize(baseData);
        _rosterMembers.Add(newMember);

        Debug.Log($"В отряд добавлен: {newMember.Name} (Класс: {baseData.ClassName})");
    }

    public UnitMember GetMemberByBaseData(UnitData baseData)
    {
        foreach (UnitMember member in _rosterMembers)
        {
            if (member.Data == baseData)
            {
                return member;
            }
        }
        return null;
    }

    public void LevelUpMember(UnitMember member)
    {
        if (_rosterMembers.Contains(member))
        {
            member.LevelUp();
            Debug.Log($"{member.Name} успешно прокачан!");
        }
    }

    public void ClearRoster()
    {
        _rosterMembers.Clear();
        Debug.Log("Отряд очищен.");
    }
}