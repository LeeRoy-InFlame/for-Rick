using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleEndPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _victoryPanel; // Панель с кнопкой (по умолчанию выключена)
    [SerializeField] private Button _returnButton;     // Кнопка "Вернуться на карту"

    [Header("Settings")]
    [SerializeField] private string _worldMapSceneName = "WorldMap"; // Имя сцены карты мира (из Build Settings)
    [SerializeField] private float _checkDelay = 1f; // Задержка перед проверкой (чтобы анимация смерти успела проиграться)

    private bool _isBattleEnded = false;

    private void Start()
    {
        // Убеждаемся, что панель победы скрыта при старте боя
        if (_victoryPanel != null) _victoryPanel.SetActive(false);

        // Настраиваем кнопку возврата
        if (_returnButton != null)
        {
            _returnButton.onClick.AddListener(ReturnToWorldMap);
        }
    }

    private void Update()
    {
        // Если бой уже закончился, не проверяем
        if (_isBattleEnded) return;

        // Проверяем, остались ли враги на поле
        if (CheckAllEnemiesDefeated())
        {
            EndBattle();
        }
    }

    private bool CheckAllEnemiesDefeated()
    {
        // Ищем всех врагов на сцене (EnemyAI или любой юнит с IsPlayer = false)
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        // Если врагов вообще нет (например, уровень без врагов) - считаем победой
        if (enemies.Length == 0) return true;

        // Проверяем, все ли они мертвы
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                // Враг жив и активен
                return false;
            }
        }

        // Все враги мертвы или уничтожены
        return true;
    }

    private void EndBattle()
    {
        _isBattleEnded = true;
        Debug.Log(" Все враги повержены! Показываем панель победы...");

        // Небольшая задержка, чтобы игрок увидел последний взрыв/смерть
        Invoke(nameof(ShowVictoryPanel), _checkDelay);
    }

    private void ShowVictoryPanel()
    {
        if (_victoryPanel != null)
        {
            _victoryPanel.SetActive(true);
            Debug.Log("Панель победы активирована!");
        }
    }

    private void ReturnToWorldMap()
    {
        Debug.Log("Возврат на карту мира...");

        // Используем наш SceneLoader для плавного перехода
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(_worldMapSceneName);
        }
        else
        {
            Debug.LogError("SceneLoader не найден! Используем стандартную загрузку.");
            SceneManager.LoadScene(_worldMapSceneName);
        }
    }
}