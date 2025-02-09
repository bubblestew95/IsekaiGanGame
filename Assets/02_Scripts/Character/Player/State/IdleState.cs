using UnityEngine;

using EnumTypes;
using StructTypes;
using Unity.Netcode;

public class IdleState : BasePlayerState
{
    private JoystickInputData joystickInputData;
    private NetworkObject networkObj = null;
    public IdleState(PlayerManager playerMng) : base(playerMng)
    {
        stateType = PlayerStateType.Idle;
    }

    public override void OnEnterState()
    {
        networkObj = playerMng.GetComponent<NetworkObject>();

    }

    public override void OnExitState()
    {
        playerMng.AnimationManager.SetAnimatorWalkSpeed(0f);
    }

    public override void OnUpdateState()
    {
        if (!networkObj.IsOwner)
            return;

        // ��� ������ ���� ������ �� ����.
        playerMng.InputManager.GetJoystickInputValue(out joystickInputData);
        playerMng.MovementManager.MoveByJoystick(joystickInputData);

        // ��� ������ ���� ��ų�� ��� ������. ��ų ��� üũ.
        InputBufferData inputBuffer = playerMng.InputManager.GetNextInput();

        if (inputBuffer.skillType != SkillSlot.None)
        {
            playerMng.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }
}
