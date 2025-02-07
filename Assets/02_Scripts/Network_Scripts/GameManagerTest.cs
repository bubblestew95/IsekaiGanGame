using Unity.Netcode;
using UnityEngine;

public class GameManagerTest : NetworkBehaviour
{
    public GameObject prefabA; // Client 0�� ������ ������Ʈ
    public GameObject prefabB; // Client 1�� ������ ������Ʈ
    public GameObject sharedPrefabC; // ��� Ŭ���̾�Ʈ�� �� �� �ִ� ���� ������Ʈ

    private void Start()
    {
        if (IsServer) // ���������� ����
        {
            SpawnPlayerControlledObjects();
            SpawnSharedObject();
        }
    }

    private void SpawnPlayerControlledObjects()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;
            GameObject playerObject = GetPlayerObjectForClient(clientId);

            if (playerObject != null)
            {
                GameObject instance = Instantiate(playerObject);
                NetworkObject networkObject = instance.GetComponent<NetworkObject>();

                // �� Ŭ���̾�Ʈ�� �ڽŸ� ������ �� �ֵ��� ����
                networkObject.SpawnAsPlayerObject(clientId);
                Debug.Log($"[Server] Ŭ���̾�Ʈ {clientId}�� ���� ������Ʈ {playerObject.name} ���� �Ϸ�.");
            }
            else
            {
                Debug.LogError($"[Server] Ŭ���̾�Ʈ {clientId}�� ���� ������Ʈ�� ã�� �� ����!");
            }
        }
    }

    private void SpawnSharedObject()
    {
        GameObject sharedInstance = Instantiate(sharedPrefabC);
        NetworkObject networkObject = sharedInstance.GetComponent<NetworkObject>();

        // ��� Ŭ���̾�Ʈ���� �� �� �ֵ��� ����ȭ
        networkObject.Spawn();
        Debug.Log("[Server] ���� ������Ʈ C�� �����Ǿ����ϴ�.");
    }

    private GameObject GetPlayerObjectForClient(ulong clientId)
    {
        return clientId % 2 == 0 ? prefabA : prefabB; // ¦�� ID �� A, Ȧ�� ID �� B
    }
}
