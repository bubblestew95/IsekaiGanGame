using Unity.Netcode;
using UnityEngine;

public class GameManagerTest : NetworkBehaviour
{
    public GameObject p_Warrior; // Client 0�� ������ ������Ʈ
    public GameObject p_Archor; // Client 1�� ������ ������Ʈ
    public GameObject p_Golem; // ��� Ŭ���̾�Ʈ�� �� �� �ִ� ���� ������Ʈ

    private void Start()
    {
        if (IsServer) // ���������� ����
        {
            SpawnPlayerControlledObjects();
            SpawnSharedObject();
        }
        else
        {
            Debug.Log("[Client] �������� ������ ������Ʈ�� ��� ��...");
        }

        if (IsOwner)
        {
            Debug.Log($"[Game Scene] �� ClientId: {NetworkManager.Singleton.LocalClientId}");
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
        if (p_Golem == null)
        {
            Debug.LogError("[Server] p_Golem �������� �������� �ʾҽ��ϴ�!");
            return;
        }

        GameObject sharedInstance = Instantiate(p_Golem);
        NetworkObject networkObject = sharedInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn();
            Debug.Log("[Server] ���� ������Ʈ C�� �����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogError("[Server] ���� ������Ʈ C�� NetworkObject�� �����ϴ�!");
        }
    }


    private GameObject GetPlayerObjectForClient(ulong clientId)
    {
        //clientId % 2 == 0 ? p_Warrior : p_Archor; // ¦�� ID �� A, Ȧ�� ID �� B

        GameObject obj = clientId % 2 == 0 ? p_Warrior : p_Archor;

        if (obj == null)
        {
            Debug.LogError($"[Server] Ŭ���̾�Ʈ {clientId}�� �Ҵ��� �������� �������� �ʽ��ϴ�!");
        }

        return obj;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Ŭ���̾�Ʈ�� �ٽ� ����Ǹ� ������ �缳��
        if (IsOwner)
        {
            RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
    [ServerRpc]
    private void RequestOwnershipServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        NetworkObject.ChangeOwnership(clientId);
        Debug.Log($"Client {clientId} ���� ������ ���� �Ϸ�!");
    }
}
