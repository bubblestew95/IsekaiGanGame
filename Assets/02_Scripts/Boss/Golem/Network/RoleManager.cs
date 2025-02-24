using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;

public class RoleManager : NetworkBehaviour
{
    public static RoleManager Instance;

    private string selectedCharacter = "-1"; // �⺻�� (-1: �������� ����)

    public string SelectedCharacter => selectedCharacter; // selectedCharacter�� Getter

    private string currentLobbyId => RoomManager.Instance?.currentLobby?.Id;

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

    // ĳ���� ���� �� ������ ������Ʈ
    public async void SelectCharacter(int characterIndex)
    {
        if (string.IsNullOrEmpty(currentLobbyId))
        {
            Debug.LogError("[RoleManager] �κ� �������� �ʽ��ϴ�!");
            return;
        }

        string playerId = AuthenticationService.Instance.PlayerId;

        if (characterIndex == -1)
        {
            Debug.Log($"[RoleManager] �÷��̾� {playerId} ĳ���� ���� ���� (-1)");
        }
        else
        {
            Debug.Log($"[RoleManager] �÷��̾� {playerId} -> ĳ���� {characterIndex} ����");
        }

        try
        {
            var options = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "CharacterSelection", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, characterIndex.ToString()) }
                }
            };

            await LobbyService.Instance.UpdatePlayerAsync(currentLobbyId, playerId, options);
            Debug.Log($"[RoleManager] �÷��̾� {playerId} ĳ���� ���� ������Ʈ �Ϸ�: {characterIndex}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"[RoleManager] ĳ���� ���� ������Ʈ ����: {ex.Message}");
        }
    }


    // �÷��̾� ��Ȱ�� id�� ��Ī�ؼ� �����ϴ� �Լ�
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRoleServerRpc(ulong _clientId, string _role)
    {
        Debug.Log($"[Server] SetPlayerRoleServerRpc ȣ��� - ClientID: {_clientId}, Role: {_role}");
        if (!playerRoles.ContainsKey(_clientId))
        {
            playerRoles.Add(_clientId, _role);
            Debug.Log($"[Server] {playerRoles[_clientId]} ���ҷ� ���� �Ϸ�.");
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

    public void ClickWarrior()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Warrior");

        Lobby_CharacterSelector.Instance.SelectCharacter(0);
    }

    public void ClickArcher()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Archer");

        Lobby_CharacterSelector.Instance.SelectCharacter(1);
    }

    public void ClickAssassin()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Assassin");

        Lobby_CharacterSelector.Instance.SelectCharacter(2);
    }

    public void ClickMagician()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Magician");

        Lobby_CharacterSelector.Instance.SelectCharacter(3);
    }


    // ���� ���� �ǵ�����
    public void ResetRole()
    {
        Debug.Log("Ŭ�� ���̵� : " + NetworkManager.Singleton.LocalClientId);
        ResetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
