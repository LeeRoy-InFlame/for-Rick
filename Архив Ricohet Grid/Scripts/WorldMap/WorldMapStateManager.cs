using UnityEngine;

public class WorldMapStateManager : MonoBehaviour
{
    public static WorldMapStateManager Instance { get; private set; }

    // Данные для сохранения
    private string _currentNodeName;
    private Vector3 _lastKnownPosition;
    private bool _isExactlyOnNode;

    // --- Свойства ---
    public string CurrentNodeName { get { return _currentNodeName; } set { _currentNodeName = value; } }
    public Vector3 LastKnownPosition { get { return _lastKnownPosition; } set { _lastKnownPosition = value; } }
    public bool IsExactlyOnNode { get { return _isExactlyOnNode; } set { _isExactlyOnNode = value; } }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // ЭТО ГЛАВНОЕ: Объект не уничтожается при смене сцены
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ClearState()
    {
        _currentNodeName = "";
        _isExactlyOnNode = false;
    }
}