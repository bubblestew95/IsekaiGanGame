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

    // 플레이어 역활과 id를 매칭해서 저장하는 함수
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRoleServerRpc(ulong _clientId, string _role)
    {
        if (!playerRoles.ContainsKey(_clientId))
        {
            playerRoles.Add(_clientId, _role);
        }
    }

    // 저장된 정보 리셋시키는 함수
    [ServerRpc(RequireOwnership = false)]
    public void ResetPlayerRoleServerRpc(ulong _clientId)
    {
        if (playerRoles.ContainsKey(_clientId))
        {
            playerRoles.Remove(_clientId);
        }
    }

    // 플레이 씬 넘어가서 clientId에 맞는 플레이어 역할 가져오는 함수
    public string GetPlayerRole(ulong clientId)
    {
        return playerRoles.ContainsKey(clientId) ? playerRoles[clientId]: "None";
    }

    // 전사 클릭시
    public void ClickWarrior()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Warrior");
    }

    // 아처 클릭시
    public void ClickArcher()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Archer");
    }

    // 어쌔신 클릭시
    public void ClickAssassin()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Assassin");
    }

    // 마법사 클릭시
    public void ClickMagician()
    {
        ResetRole();
        SetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId, "P_Magician");
    }

    // 직업 선택 되돌리기
    public void ResetRole()
    {
        ResetPlayerRoleServerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
