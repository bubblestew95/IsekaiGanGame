using Unity.Netcode;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private PlayerManager playerManager = null;

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += FindPlayerObjectForClient;
    }

    private void Update()
    {
        if(playerManager != null)
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
