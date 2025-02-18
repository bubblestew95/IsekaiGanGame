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
            Debug.Log("Hello!");
            ulong clientId = playerManager.PlayerNetworkManager.OwnerClientId;
            playerManager.PlayerNetworkManager.OnNetworkPlayerDeath?.Invoke(clientId);
        }

        // �÷��̾ �׾��� ��, ��ų UI�� ��Ȱ��ȭ�ϰ� �̵��� ���߸� ��� �ִϸ��̼��� ���.
        {
            if (playerManager.SkillUIManager != null)
                playerManager.SkillUIManager.SetAllSkillUIEnabled(false);
            if (playerManager.MovementManager != null)
                playerManager.MovementManager.StopMove();
            if (playerManager.AnimationManager != null)
                playerManager.AnimationManager.PlayDeathAnimation();
        }

        playerManager.GetComponent<CharacterController>().enabled = false;

    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        
    }
}
