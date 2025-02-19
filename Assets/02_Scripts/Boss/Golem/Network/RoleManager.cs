using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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
            UpdateCharacterSelectionClientRpc(_clientId, _role);
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

    [ClientRpc]
    private void UpdateCharacterSelectionClientRpc(ulong clientId, string selectedRole)
    {
        Debug.Log($"[Client] {clientId}�� {selectedRole} ����!");

        // UI���� �ش� ���� ��ư ��Ȱ��ȭ
        switch (selectedRole)
        {
            case "P_Warrior":
                warriorButton.interactable = false;
                break;
            case "P_Archer":
                archerButton.interactable = false;
                break;
            case "P_Assassin":
                assassinButton.interactable = false;
                break;
            case "P_Magician":
                magicianButton.interactable = false;
                break;
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
        Debug.Log("Ŭ�� ���̵� : " + NetworkManager.Singleton.LocalClientId);
        ResetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
