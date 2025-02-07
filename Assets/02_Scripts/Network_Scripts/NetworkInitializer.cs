using UnityEngine;
using Unity.Netcode;

public class NetworkInitializer : MonoBehaviour
{
    public GameObject networkManagerPrefab;

    private void Awake()
    {
        if (NetworkManager.Singleton == null) // NetworkManager�� ������ ����
        {
            GameObject networkManager = Instantiate(networkManagerPrefab);
            DontDestroyOnLoad(networkManager);
            Debug.Log("[Network] ���ο� NetworkManager�� �����Ǿ����ϴ�.");
        }
        else
        {
            Debug.Log("[Network] ���� NetworkManager�� �����մϴ�.");
        }
    }

    private void Start()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartHost(); // ȣ��Ʈ�� ���� (�׽�Ʈ ���)
            Debug.Log("[Network] Game ������ �ٷ� Host ����");
        }
    }
}
