using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Не забудь это добавить!

public class WorldMapNode : MonoBehaviour
{
    [Header("Node Settings")]
    [SerializeField] private string _nodeName;
    [SerializeField] private bool _isBattleNode;
    [SerializeField] private string _battleSceneName;

    [Header("Connections")]
    [SerializeField] private List<WorldMapNode> _connectedNodes = new List<WorldMapNode>();

    public string NodeName { get { return _nodeName; } }
    public bool IsBattleNode { get { return _isBattleNode; } }
    public List<WorldMapNode> ConnectedNodes { get { return _connectedNodes; } }

    public void OnNodeReached()
    {
        Debug.Log($"Отряд прибыл в: {_nodeName}");

        if (_isBattleNode)
        {
            Debug.Log("Начинается сражение! Сохраняем позицию и загружаем сцену...");

            // ИСПРАВЛЕНИЕ: Современный аналог FindObjectOfType
            PartyController party = FindFirstObjectByType<PartyController>();

            if (party != null)
            {
                party.SaveWorldMapState();
            }

            // SceneManager.LoadScene(_battleSceneName);
        }
        else
        {
            Debug.Log("Мирное место. Открываем магазин/таверну.");
        }
    }
}