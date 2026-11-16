using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Game/Equipment")]
public class EquipmentData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    public GameObject prefab; // Префаб для визуализации

    [Header("Equipment Slot")]
    public EquipmentSlot slot; // Head, Body, Weapon, Ears, Legs

    [Header("Stat Bonuses")]
    public int strengthBonus;
    public int agilityBonus;
    public int enduranceBonus;
    public int intelligenceBonus;
    public int charismaBonus;
    public int luckBonus;
    public int maxHealthBonus;
    public int maxManaBonus;

    [Header("UI Paper Doll")]
    [Tooltip("Спрайт, который будет накладываться на персонажа в меню")]
    public Sprite paperDollSprite;
}

public enum EquipmentSlot
{
    Head, Body, Weapon, Ears, Legs
}