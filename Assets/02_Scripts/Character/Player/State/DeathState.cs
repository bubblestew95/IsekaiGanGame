using UnityEngine;

public class DeathState : BasePlayerState
{
    public DeathState(PlayerManager playerMng) : base(playerMng)
    {
        stateType = EnumTypes.PlayerStateType.Death;
    }

    public override void OnEnterState()
    {
        // �÷��̾ �׾��� ��, ���� ������ �ƴ� ��쿡�� ��Ʈ��ũ���� ������ �˸�.
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
