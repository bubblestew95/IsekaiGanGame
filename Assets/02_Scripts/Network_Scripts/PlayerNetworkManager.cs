using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerNetworkManager : NetworkBehaviour
{
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
