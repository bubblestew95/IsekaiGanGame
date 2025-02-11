using System.Collections.Generic;
using Unity.Netcode;

public class RoleManager : NetworkBehaviour
{
    public static RoleManager Instance;

    private Dictionary<ulong, string> playerRoles = new Dictionary<ulong, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!playerRoles.ContainsKey(clientId))
                {
                    playerRoles[clientId] = "";
                }
            }
        }
    }

    // �÷��̾ �ٸ� ĳ���� Ŭ���Ҷ����� ȣ��Ǿ���
    // �����ִ� role = �÷��̾� ������ �̸��� ���ƾ���.
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRoleServerRpc(ulong _clientId, string _role)
    {
        if (!playerRoles.ContainsKey(_clientId))
        {
            playerRoles[_clientId] = _role;
        }
    }

    // �÷��� �� �Ѿ�� clientId�� �´� �÷��̾� ���� �������� �Լ�
    public string GetPlayerRole(ulong clientId)
    {
        return playerRoles.ContainsKey(clientId) ? playerRoles[clientId]: "None";
    }
}
