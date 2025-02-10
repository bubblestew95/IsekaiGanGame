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
        // �� id ��������
        ulong myClientId = NetworkManager.Singleton.LocalClientId;

        // ���� ã��
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log("�������� Ŭ�� ���̵� : " + client.ClientId);

            if (client.ClientId == myClientId)
            {
                playerManager = client.PlayerObject.GetComponent<PlayerManager>();
            }
        }
    }
}
