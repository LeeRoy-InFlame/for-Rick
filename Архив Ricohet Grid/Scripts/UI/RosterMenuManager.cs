using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RosterMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterDetailPanel _detailPanel;
    [SerializeField] private PlayerRoster _playerRoster;

    [Header("Portrait Buttons")]
    [SerializeField] private Transform _portraitButtonsParent;
    [SerializeField] private CharacterPortraitButton _portraitButtonPrefab;

    [Header("Close Button")]
    [SerializeField] private Button _closeButton;

    // НОВОЕ: Кнопки прокачки (перетащи их из правой панели в Инспекторе)
    [Header("Stat Allocation Buttons")]
    [SerializeField] private Button _btnPlusStrength;
    [SerializeField] private Button _btnPlusAgility;
    [SerializeField] private Button _btnPlusEndurance;
    [SerializeField] private Button _btnPlusIntelligence;
    [SerializeField] private Button _btnPlusCharisma;
    [SerializeField] private Button _btnPlusLuck;

    private List<CharacterPortraitButton> _portraitButtons = new List<CharacterPortraitButton>();
    private CharacterPortraitButton _selectedButton;
    private bool _isInitialized = false;

    private void Awake()
    {
        _playerRoster = PlayerRoster.Instance;
        if (_playerRoster == null) Debug.LogError("RosterMenuManager: Не удалось найти PlayerRoster!");
    }

    private void Start()
    {
        gameObject.SetActive(false);
        if (_closeButton != null) _closeButton.onClick.AddListener(CloseMenu);

        SetupStatButtons(); // НОВОЕ: Настраиваем кнопки прокачки
    }

    // НОВОЕ: Привязываем кнопки к методу панели
    private void SetupStatButtons()
    {
        if (_btnPlusStrength != null) _btnPlusStrength.onClick.AddListener(() => _detailPanel.OnPlusButtonClicked(StatType.Strength));
        if (_btnPlusAgility != null) _btnPlusAgility.onClick.AddListener(() => _detailPanel.OnPlusButtonClicked(StatType.Agility));
        if (_btnPlusEndurance != null) _btnPlusEndurance.onClick.AddListener(() => _detailPanel.OnPlusButtonClicked(StatType.Endurance));
        if (_btnPlusIntelligence != null) _btnPlusIntelligence.onClick.AddListener(() => _detailPanel.OnPlusButtonClicked(StatType.Intelligence));
        if (_btnPlusCharisma != null) _btnPlusCharisma.onClick.AddListener(() => _detailPanel.OnPlusButtonClicked(StatType.Charisma));
        if (_btnPlusLuck != null) _btnPlusLuck.onClick.AddListener(() => _detailPanel.OnPlusButtonClicked(StatType.Luck));
    }

    public void OpenMenu()
    {
        gameObject.SetActive(true);
        if (_portraitButtonsParent != null) _portraitButtonsParent.gameObject.SetActive(true);
        if (_detailPanel != null) _detailPanel.gameObject.SetActive(true);

        if (!_isInitialized)
        {
            BuildPortraits();
            if (_portraitButtons.Count > 0) SelectPortrait(_portraitButtons[0]);
            _isInitialized = true;
        }
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    private void BuildPortraits()
    {
        foreach (Transform child in _portraitButtonsParent) Destroy(child.gameObject);
        _portraitButtons.Clear();

        if (_playerRoster == null) return;

        List<UnitMember> rosterMembers = _playerRoster.GetRosterMembers();
        foreach (UnitMember member in rosterMembers)
        {
            CharacterPortraitButton newButton = Instantiate(_portraitButtonPrefab, _portraitButtonsParent);
            newButton.Setup(member);

            Button btnComponent = newButton.GetComponent<Button>();
            CharacterPortraitButton capturedButton = newButton;
            btnComponent.onClick.AddListener(() => SelectPortrait(capturedButton));

            _portraitButtons.Add(newButton);
        }
    }

    private void SelectPortrait(CharacterPortraitButton button)
    {
        if (_selectedButton != null) _selectedButton.SetSelected(false);

        _selectedButton = button;
        _selectedButton.SetSelected(true);

        if (_detailPanel != null && button.UnitMember != null)
        {
            _detailPanel.Setup(button.UnitMember);
        }
    }
}