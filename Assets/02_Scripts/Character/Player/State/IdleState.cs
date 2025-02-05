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
        // ��� ������ ���� ������ �� ����.
        playerMng.InputManager.GetJoystickInputValue(out joystickInputData);
        playerMng.MoveByJoystick(joystickInputData);

        // ��� ������ ���� ��ų�� ��� ������. ��ų ��� üũ.
        InputBufferData inputBuffer = playerMng.GetNextInput();

        if (inputBuffer.skillType != SkillSlot.None)
        {
            playerMng.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }
}
