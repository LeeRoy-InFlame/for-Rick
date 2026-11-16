using UnityEngine;
using UnityEngine.UI;

public class UnitContextMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button _moveButton;
    [SerializeField] private Button _attackButton;
    [SerializeField] private RectTransform _rectTransform;

    private void Awake()
    {
        // Скрываем меню при старте
        gameObject.SetActive(false);
    }

    public void SetupAndShow(Unit unit)
    {
        // Очищаем старые слушатели, чтобы не дублировать события
        _moveButton.onClick.RemoveAllListeners();
        _attackButton.onClick.RemoveAllListeners();

        // Настраиваем кнопки
        _moveButton.interactable = !unit.HasMoved && unit.CurrentActions > 0;
        _attackButton.interactable = !unit.HasShot && unit.CurrentActions > 0;

        _moveButton.onClick.AddListener(() => {
            GameManager.Instance.EnterMoveMode();
            Hide();
        });

        _attackButton.onClick.AddListener(() => {
            GameManager.Instance.EnterAttackMode();
            Hide();
        });

        // Позиционируем меню над юнитом в экранном пространстве
        Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
        // Смещаем немного вверх (например, на 80 пикселей), чтобы не перекрывать спрайт
        _rectTransform.position = screenPos + new Vector3(0, 80, 0);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}