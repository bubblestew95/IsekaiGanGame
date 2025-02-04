using UnityEngine;

using EnumTypes;
using StructTypes;

public class IdleState : BasePlayerState
{
    private JoystickInputData joystickInputData;

    public IdleState(PlayerManager playerMng) : base(playerMng)
    {
    }

    public override void OnEnterState()
    {
        
    }

    public override void OnExitState()
    {
        playerMng.SetAnimatorWalkSpeed(0f);
    }

    public override void OnUpdateState()
    {
        // 대기 상태일 때만 움직일 수 있음.
        playerMng.InputManager.GetJoystickInputValue(out joystickInputData);
        playerMng.MoveByJoystick(joystickInputData);

        // 대기 상태일 때만 스킬이 사용 가능함. 스킬 사용 체크.
        InputBufferData inputBuffer = playerMng.GetNextInput();

        if (inputBuffer.skillType != SkillSlot.None)
        {
            playerMng.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }
}
