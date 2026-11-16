using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading UI")]
    [SerializeField] private GameObject _loadingPanel;      // Панель с текстом и слайдером
    [SerializeField] private Slider _progressSlider;        // Полоска прогресса
    [SerializeField] private TextMeshProUGUI _progressText; // Текст с процентами

    [Header("Fade Effect")]
    [SerializeField] private CanvasGroup _fadeOverlay;      // Черный экран (должен иметь компонент CanvasGroup)
    [SerializeField] private float _fadeDuration = 0.8f;    // Скорость затемнения/проявления

    private bool _isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // При старте убеждаемся, что всё скрыто и прозрачно
            if (_loadingPanel != null) _loadingPanel.SetActive(false);
            if (_fadeOverlay != null) _fadeOverlay.alpha = 0f;
            if (_fadeOverlay != null) _fadeOverlay.blocksRaycasts = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (_isLoading)
        {
            Debug.LogWarning("Загрузка уже идет! Успокойся, Морти.");
            return;
        }

        _isLoading = true;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 1. ВКЛЮЧАЕМ панель загрузки ДО начала затемнения, чтобы она гарантированно существовала
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
            // Сбрасываем слайдер в 0 для красоты
            if (_progressSlider != null) _progressSlider.value = 0f;
            if (_progressText != null) _progressText.text = "Загрузка: 0%";
        }

        // 2. Плавно затемняем экран (от 0 до 1). Блокируем клики, чтобы не нажимали кнопки под ним
        if (_fadeOverlay != null)
        {
            _fadeOverlay.blocksRaycasts = true;
            yield return StartCoroutine(FadeCanvasGroup(_fadeOverlay, 0f, 1f, _fadeDuration));
        }

        // 3. Начинаем асинхронную загрузку
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Ждем полной загрузки

        // 4. Цикл обновления прогресса. Unity останавливается на 0.9, пока allowSceneActivation == false
        while (asyncLoad.progress < 0.9f)
        {
            float progress = asyncLoad.progress / 0.9f;
            UpdateLoadingUI(progress);
            yield return null;
        }

        // 5. Принудительно ставим 100% и делаем микро-паузу, чтобы игрок УВИДЕЛ заполненный слайдер
        UpdateLoadingUI(1f);
        yield return new WaitForSeconds(0.4f);

        // 6. Разрешаем активацию сцены
        asyncLoad.allowSceneActivation = true;

        // Ждем, пока сцена физически станет активной (isDone станет true)
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 7. Сцена загружена! Плавно проявляем экран (от 1 до 0)
        if (_fadeOverlay != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(_fadeOverlay, 1f, 0f, _fadeDuration));
            _fadeOverlay.blocksRaycasts = false; // Возвращаем возможность кликать
        }

        // 8. Выключаем панель загрузки и сбрасываем флаг
        if (_loadingPanel != null) _loadingPanel.SetActive(false);
        _isLoading = false;

        Debug.Log($"Сцена '{sceneName}' успешно загружена!");
    }

    // Отдельный метод для чистоты кода и гарантированного обновления обоих элементов
    private void UpdateLoadingUI(float progress)
    {
        int percentage = Mathf.RoundToInt(progress * 100f);
        if (_progressSlider != null) _progressSlider.value = progress;
        if (_progressText != null) _progressText.text = $"Загрузка: {percentage}%";
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        cg.alpha = startAlpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
    }
}