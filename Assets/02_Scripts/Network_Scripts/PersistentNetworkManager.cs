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
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� NetworkManager ����
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            Destroy(gameObject);
        }
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
        Debug.Log($"[PersistentNetworkManager] Ŭ���̾�Ʈ ����� - clientId: {clientId}");

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"[PersistentNetworkManager] ���� ������. Host ClientId: {NetworkManager.Singleton.LocalClientId}");
            clientId = 0; // Host�� �׻� ClientId 0���� ����
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        if (!clientIdToPlayerId.ContainsKey(clientId))
        {
            clientIdToPlayerId[clientId] = playerId;
            Debug.Log($"[PersistentNetworkManager] ��� �Ϸ� - clientId: {clientId}, PlayerId: {playerId}");
        }

        // Ŭ���̾�Ʈ���� playerId ����ȭ
        SendPlayerIdToClientRpc(clientId, playerId);
    }



    [ClientRpc]
    private void SendPlayerIdToClientRpc(ulong clientId, string playerId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            clientIdToPlayerId[clientId] = playerId;
            Debug.Log($"[PersistentNetworkManager] Ŭ���̾�Ʈ���� PlayerId ������Ʈ �Ϸ� - clientId: {clientId}, PlayerId: {playerId}");
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

        Debug.LogError($"[PersistentNetworkManager] GetPlayerId() ����: clientId {clientId}�� �ش��ϴ� PlayerId ����.");
        return "Unknown";
    }

}
