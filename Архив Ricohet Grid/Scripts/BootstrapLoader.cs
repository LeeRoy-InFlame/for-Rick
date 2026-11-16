using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    private void Start()
    {
        // ћгновенно грузим главное меню
        SceneManager.LoadScene(_mainMenuSceneName);
    }
}