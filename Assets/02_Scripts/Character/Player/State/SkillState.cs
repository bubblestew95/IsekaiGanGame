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
        Debug.Log("Hello?");
    }

    public override void OnExitState()
    {
        playerManager.ParticleController.DespawnParticles();
    }

    public override void OnUpdateState()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerManager.InputManager.OnSkillKeyInput(SkillSlot.Dash);
        }

        // ��ų ��� ������ ���� ȸ�� ��� ������. ȸ�� ��ų ��� üũ.
        InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

        if (inputBuffer.skillType == SkillSlot.Dash)
        {
            playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }


}
