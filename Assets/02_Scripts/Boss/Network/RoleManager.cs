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

    // 플레이어가 다른 캐릭터 클릭할때마다 호출되야함
    // 보내주는 role = 플레이어 프리펩 이름과 같아야함.
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerRoleServerRpc(ulong _clientId, string _role)
    {
        if (!playerRoles.ContainsKey(_clientId))
        {
            playerRoles[_clientId] = _role;
        }
    }

    // 플레이 씬 넘어가서 clientId에 맞는 플레이어 역할 가져오는 함수
    public string GetPlayerRole(ulong clientId)
    {
        return playerRoles.ContainsKey(clientId) ? playerRoles[clientId]: "None";
    }
}
