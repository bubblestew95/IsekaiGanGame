using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerNetworkController
{
    private PlayerManager playerManager = null;
    private NetworkAnimator networkAnimator = null;
    private NetworkObject networkObject = null;

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
        networkAnimator = playerManager.GetComponent<NetworkAnimator>();
        networkObject = playerManager.GetComponent<NetworkObject>();
    }

    public void SetNetworkAnimatorTrigger(int _hashId)
    {
        if(networkAnimator == null)
        {
            Debug.Log("Network Animator is not valid!");
            return;
        }

        networkAnimator.SetTrigger(_hashId);
    }
}
