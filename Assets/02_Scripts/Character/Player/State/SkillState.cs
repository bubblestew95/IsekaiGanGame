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
    }

    public override void OnExitState()
    {
        
    }

    public override void OnUpdateState()
    {
        // ��ų ��� ������ ���� ȸ�� ��� ������. ȸ�� ��ų ��� üũ.
        InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

        if (inputBuffer.skillType == SkillSlot.Dash)
        {
            playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }


}
