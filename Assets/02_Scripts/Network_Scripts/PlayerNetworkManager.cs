using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

public class PlayerNetworkManager : NetworkBehaviour
{
    public UnityAction<ulong> OnNetworkPlayerDeath;
    public UnityAction<ulong> OnNetworkPlayerRevive;

    private PlayerManager playerManager = null;
    private NetworkAnimator networkAnimator = null;

    public void SetNetworkAnimatorTrigger(int _hashId)
    {
        if(networkAnimator == null)
        {
            Debug.Log("Network Animator is not valid!");
            return;
        }

        networkAnimator.SetTrigger(_hashId);
    }

    public void NetworkRevivePlayer()
    {
        ServerRevivePlayerRpc(OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void ServerRevivePlayerRpc(ulong _clientId)
    {
        ApplyRevivePlayerRpc(_clientId);
    }

    [Rpc(SendTo.Everyone)]
    private void ApplyRevivePlayerRpc(ulong _clientId)
    {
        var obj = NetworkManager.ConnectedClients[_clientId].PlayerObject;

        if (obj != null)
        {
            obj.GetComponent<PlayerManager>().ApplyRevive();
        }
    }

    public override void OnNetworkSpawn()
    {
        FindAnyObjectByType<NetworkGameManager>().CheckPlayerSpawnServerRpc();
    }

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        networkAnimator = playerManager.GetComponent<NetworkAnimator>();
    }
}
