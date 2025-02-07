using Unity.Netcode;
using UnityEngine;

public class GameManagerTest : NetworkBehaviour
{
    public GameObject prefabA; // Client 0이 조작할 오브젝트
    public GameObject prefabB; // Client 1이 조작할 오브젝트
    public GameObject sharedPrefabC; // 모든 클라이언트가 볼 수 있는 공용 오브젝트

    private void Start()
    {
        if (IsServer) // 서버에서만 실행
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

                // 각 클라이언트가 자신만 조작할 수 있도록 설정
                networkObject.SpawnAsPlayerObject(clientId);
                Debug.Log($"[Server] 클라이언트 {clientId}의 전용 오브젝트 {playerObject.name} 스폰 완료.");
            }
            else
            {
                Debug.LogError($"[Server] 클라이언트 {clientId}에 대한 오브젝트를 찾을 수 없음!");
            }
        }
    }

    private void SpawnSharedObject()
    {
        GameObject sharedInstance = Instantiate(sharedPrefabC);
        NetworkObject networkObject = sharedInstance.GetComponent<NetworkObject>();

        // 모든 클라이언트에서 볼 수 있도록 동기화
        networkObject.Spawn();
        Debug.Log("[Server] 공용 오브젝트 C가 스폰되었습니다.");
    }

    private GameObject GetPlayerObjectForClient(ulong clientId)
    {
        return clientId % 2 == 0 ? prefabA : prefabB; // 짝수 ID → A, 홀수 ID → B
    }
}
