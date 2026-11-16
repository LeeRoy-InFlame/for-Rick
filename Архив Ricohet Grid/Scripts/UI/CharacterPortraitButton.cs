using UnityEngine;
using UnityEngine.UI;

public class CharacterPortraitButton : MonoBehaviour
{
    [SerializeField] private Image _portraitImage;
    [SerializeField] private GameObject _selectionBorder;

    // Храним ссылку на конкретного члена отряда
    private UnitMember _unitMember;
    private bool _isSelected = false;

    // Публичный геттер для менеджера
    public UnitMember UnitMember { get { return _unitMember; } }

    // Принимаем UnitMember
    public void Setup(UnitMember member)
    {
        _unitMember = member;

        if (_portraitImage != null && member.Data.PortraitSprite != null)
        {
            _portraitImage.sprite = member.Data.PortraitSprite;
        }
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        if (_selectionBorder != null)
        {
            _selectionBorder.SetActive(isSelected);
        }
    }
}