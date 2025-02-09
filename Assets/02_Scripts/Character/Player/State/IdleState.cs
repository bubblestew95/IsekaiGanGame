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

        // 대기 상태일 때만 움직일 수 있음.
        playerMng.InputManager.GetJoystickInputValue(out joystickInputData);
        playerMng.MovementManager.MoveByJoystick(joystickInputData);

        // 대기 상태일 때만 스킬이 사용 가능함. 스킬 사용 체크.
        InputBufferData inputBuffer = playerMng.InputManager.GetNextInput();

        if (inputBuffer.skillType != SkillSlot.None)
        {
            playerMng.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }
}
