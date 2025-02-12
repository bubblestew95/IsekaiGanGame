using UnityEngine;

public class DeathState : BasePlayerState
{
    public DeathState(PlayerManager playerMng) : base(playerMng)
    {
        stateType = EnumTypes.PlayerStateType.Death;
    }

    public override void OnEnterState()
    {
        if(!GameManager.Instance.IsLocalGame)
        {
            ulong clientId = playerManager.PlayerNetworkManager.OwnerClientId;
            playerManager.PlayerNetworkManager.OnNetworkPlayerDeath?.Invoke(clientId);
        }

        playerManager.AnimationManager.PlayDeathAnimation();

    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        
    }
}
