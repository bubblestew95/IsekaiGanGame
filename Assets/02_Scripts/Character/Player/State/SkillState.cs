using UnityEngine;

using EnumTypes;
using StructTypes;

public class SkillState : BasePlayerState
{
    public SkillState(PlayerManager _playerManager) : base(_playerManager)
    {
        stateType = PlayerStateType.Skill;
    }

    public override void OnEnterState()
    {
        playerManager.MovementManager.StopMove();
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
    }

    public override void OnExitState()
    {
        playerManager.ParticleController.DespawnParticles();

        if(playerManager.AttackManager.IsMeleeWeaponSet())
        {
            playerManager.AttackManager.DisableMeleeAttack();
        }
    }

    public override void OnUpdateState()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Dash);
        }

        // ��ų ��� ������ ���� ȸ�� ��� ������. ȸ�� ��ų ��� üũ.
        InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

        if (inputBuffer.skillType == SkillSlot.Dash)
        {
            playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }


}
