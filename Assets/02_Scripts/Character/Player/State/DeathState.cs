using UnityEngine;

public class DeathState : BasePlayerState
{
    public DeathState(PlayerManager playerMng) : base(playerMng)
    {
        stateType = EnumTypes.PlayerStateType.Death;
    }

    public override void OnEnterState()
    {
        playerMng.AnimationManager.PlayDeathAnimation();
    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        
    }
}
