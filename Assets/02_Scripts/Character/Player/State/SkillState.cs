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
        // 스킬 사용 상태일 때는 회피 사용 가능함. 회피 스킬 사용 체크.
        InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

        if (inputBuffer.skillType == SkillSlot.Dash)
        {
            playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }


}
