using UnityEngine;
using UnityEngine.UI;

public class SceneLoaderButton : MonoBehaviour
{
    public string SceneName; // Имя сцены для загрузки (точно как в Build Settings!)

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button != null && SceneLoader.Instance != null)
        {
            // Программно добавляем событие клика. 
            // Никаких ручных перетаскиваний в Инспекторе больше не нужно!
            _button.onClick.AddListener(() => SceneLoader.Instance.LoadScene(SceneName));
        }
        else
        {
            Debug.LogError("SceneLoaderButton: Не найдена кнопка или SceneLoader.Instance равен null! Проверь сцену Bootstrap.");
        }
    }
}