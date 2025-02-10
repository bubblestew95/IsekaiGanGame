using Unity.Netcode;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private PlayerManager playerManager = null;

    private void Awake()
    {
        // CheckNetworkSync.loadingFinishCallback += FindPlayerObjectForClient;
    }

    private void Start()
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    private void Update()
    {
        transform.position = playerManager.transform.position;
    }


    private void FindPlayerObjectForClient()
    {
        // 내 id 가져오기
        ulong myClientId = NetworkManager.Singleton.LocalClientId;

        // 내꺼 찾기
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log("접속중인 클라 아이디 : " + client.ClientId);

            if (client.ClientId == myClientId)
            {
                playerManager = client.PlayerObject.GetComponent<PlayerManager>();
            }
        }
    }
}
