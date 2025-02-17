using System.Collections.Generic;
using System.Data;
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

    // �÷��̾� ��Ȱ�� id�� ��Ī�ؼ� �����ϴ� �Լ�
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRoleServerRpc(ulong _clientId, string _role)
    {
        if (!playerRoles.ContainsKey(_clientId))
        {
            playerRoles.Add(_clientId, _role);
        }
    }

    // ����� ���� ���½�Ű�� �Լ�
    [ServerRpc(RequireOwnership = false)]
    public void ResetPlayerRoleServerRpc(ulong _clientId)
    {
        if (playerRoles.ContainsKey(_clientId))
        {
            playerRoles.Remove(_clientId);
        }
    }

    // �÷��� �� �Ѿ�� clientId�� �´� �÷��̾� ���� �������� �Լ�
    public string GetPlayerRole(ulong clientId)
    {
        return playerRoles.ContainsKey(clientId) ? playerRoles[clientId]: "None";
    }

    // ���� Ŭ����
    public void ClickWarrior()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Warrior");
    }

    // ��ó Ŭ����
    public void ClickArcher()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Archer");
    }

    // ��ؽ� Ŭ����
    public void ClickAssassin()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Assassin");
    }

    // ������ Ŭ����
    public void ClickMagician()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Magician");
    }

    // ���� ���� �ǵ�����
    public void ResetRole()
    {
        ResetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
