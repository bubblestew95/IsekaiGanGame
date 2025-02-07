using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PersistentNetworkManager : MonoBehaviour
{
    public static PersistentNetworkManager Instance { get; private set; }

    private Dictionary<ulong, string> clientIdToPlayerId = new Dictionary<ulong, string>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);// �� ��ȯ �Ŀ��� ����
        }
        else
        {
            Destroy(gameObject);
        }

        // Netcode���� Ŭ���̾�Ʈ ���� �̺�Ʈ ���
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            RegisterPlayer(clientId, playerId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientIdToPlayerId.ContainsKey(clientId))
        {
            clientIdToPlayerId.Remove(clientId);
            Debug.Log($"[PersistentNetworkManager] Ŭ���̾�Ʈ ���� ���� - clientId: {clientId}");
        }
    }

    public void RegisterPlayer(ulong clientId, string playerId)
    {
        if (!clientIdToPlayerId.ContainsKey(clientId))
        {
            clientIdToPlayerId[clientId] = playerId;
            Debug.Log($"[PersistentNetworkManager] ��� �Ϸ� - clientId: {clientId}, PlayerId: {playerId}");
        }
    }

    public string GetPlayerId(ulong clientId)
    {
        if (clientIdToPlayerId.TryGetValue(clientId, out string playerId))
        {
            return playerId;
        }
        return "Unknown";
    }
}
