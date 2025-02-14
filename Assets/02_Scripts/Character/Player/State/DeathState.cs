using UnityEngine;

public class DeathState : BasePlayerState
{
    public DeathState(PlayerManager playerMng) : base(playerMng)
    {
        stateType = EnumTypes.PlayerStateType.Death;
    }

    public override void OnEnterState()
    {
        // 플레이어가 죽었을 때, 로컬 게임이 아닌 경우에만 네트워크에게 죽음을 알림.
        if (!GameManager.Instance.IsLocalGame)
        {
            ulong clientId = playerManager.PlayerNetworkManager.OwnerClientId;
            playerManager.PlayerNetworkManager.OnNetworkPlayerDeath?.Invoke(clientId);
        }

        playerManager.GetComponent<CharacterController>().enabled = false;

        playerManager.SkillUIManager.SetAllSkillUIEnabled(false);
        playerManager.MovementManager.StopMove();
        playerManager.AnimationManager.PlayDeathAnimation();
    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        
    }
}
