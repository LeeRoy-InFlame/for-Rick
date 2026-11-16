using System;
using UnityEngine;
using UnityEngine.UI;

public class CinematicStrikeManager : MonoBehaviour
{
    public static CinematicStrikeManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _portraitImage;

    [Header("Settings")]
    [SerializeField] private float _animationDuration = 1.2f;
    [SerializeField] private float _flashDuration = 0.1f;

    [Header("Background Flow Animation")]
    [SerializeField] private float _flowSpeed = 100f; // Скорость течения (пикселей в секунду)
    [SerializeField] private Vector2 _flowDirection = new Vector2(1f, 0f); // Направление течения (по умолчанию вправо)

    [Header("Portrait Flow Animation")]
    [SerializeField] private Animation _portraitAnimation;

    private bool _isPlaying = false;
    private RectTransform _backgroundRect;
    private Vector2 _flowOffset;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Получаем RectTransform фона для анимации
        if (_backgroundImage != null)
        {
            _backgroundRect = _backgroundImage.rectTransform;
            _flowOffset = Vector2.zero;
        }

        // Скрываем всё при старте
        if (_mainCanvasGroup != null)
        {
            _mainCanvasGroup.alpha = 0f;
            _mainCanvasGroup.interactable = false;
            _mainCanvasGroup.blocksRaycasts = false;
        }
    }

    // Главный метод. Вызывает анимацию, а по завершении выполняет action (нанесение урона)
    public void TriggerLethalStrike(Unit attacker, Unit target, Action onComplete)
    {
        if (_isPlaying) return;
        _isPlaying = true;

        // 1. Настраиваем портрет (Слева для игрока, Справа для врага)
        bool isPlayerAttacking = attacker.IsPlayer;

        // Меняем цвет фона и направление течения
        if (isPlayerAttacking)
        {
            _backgroundImage.color = new Color(0.1f, 0.4f, 0.8f, 1f);
            _flowDirection = new Vector2(1f, 0f); // Течение справа налево для игрока
        }
        else
        {
            _backgroundImage.color = new Color(0.8f, 0.1f, 0.1f, 1f);
            _flowDirection = new Vector2(-1f, 0f); // Течение слева направо для врага
        }

        // Позиционируем портрет
        RectTransform portraitRect = _portraitImage.rectTransform;
        portraitRect.anchoredPosition = isPlayerAttacking ? new Vector2(200f, -100f) : new Vector2(-200f, -100f);
        _portraitAnimation.Play();

        // Отражаем по горизонтали
        portraitRect.localScale = new Vector3(isPlayerAttacking ? 1f : -1f, 1f, 1f);

        // 2. Показываем UI
        _mainCanvasGroup.alpha = 1f;
        _mainCanvasGroup.interactable = true;
        _mainCanvasGroup.blocksRaycasts = true;

        // 3. Запускаем корутину анимации
        StartCoroutine(PlayStrikeSequence(onComplete));
    }

    private System.Collections.IEnumerator PlayStrikeSequence(Action onComplete)
    {
        float elapsedTime = 0f;
        _flowOffset = Vector2.zero;

        // Фаза 1: Появление и удержание (0 до _animationDuration - _flashDuration)
        float holdTime = _animationDuration - _flashDuration;

        while (elapsedTime < holdTime)
        {
            elapsedTime += Time.unscaledDeltaTime;

            // АНИМАЦИЯ ФОНА: Постоянное течение
            _flowOffset += _flowDirection * _flowSpeed * Time.unscaledDeltaTime;

            if (_backgroundRect != null)
            {
                // Двигаем фон как быстрое течение
                _backgroundRect.anchoredPosition = _flowOffset;
            }

            yield return null;
        }

        // Фаза 2: Вспышка и исчезновение
        elapsedTime = 0f;
        while (elapsedTime < _flashDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / _flashDuration;

            // Резко обесцвечиваем и делаем прозрачным
            _mainCanvasGroup.alpha = 1f - t;

            yield return null;
        }

        // 4. Скрываем UI полностью
        _mainCanvasGroup.alpha = 0f;
        _mainCanvasGroup.interactable = false;
        _mainCanvasGroup.blocksRaycasts = false;

        // Сбрасываем позицию фона
        if (_backgroundRect != null)
        {
            _backgroundRect.anchoredPosition = Vector2.zero;
        }

        _isPlaying = false;

        // 5. ВОЗВРАЩАЕМ УПРАВЛЕНИЕ: выполняем реальную атаку
        onComplete?.Invoke();
    }
}