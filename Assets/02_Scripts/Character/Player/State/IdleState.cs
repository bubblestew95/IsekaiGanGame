using UnityEngine;

using EnumTypes;
using StructTypes;
using Unity.Netcode;

public class IdleState : BasePlayerState
{
    private JoystickInputData joystickInputData;
    private NetworkObject networkObj = null;
    private bool isLocalGame = false;

    public IdleState(PlayerManager _playerManager) : base(_playerManager)
    {
        stateType = PlayerStateType.Idle;
    }

    public override void OnEnterState()
    {
        networkObj = playerManager.GetComponent<NetworkObject>();
        // isLocalGame = GameManager.Instance.IsLocalGame;
    }

    public override void OnExitState()
    {
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
    }

    public override void OnUpdateState()
    {
        // 로컬 게임이 아니고, 네트워크 오브젝트가 소유권을 가지고 있지 않다면 리턴.
        if (!isLocalGame && !networkObj.IsOwner)
            return;

        // 대기 상태일 때만 움직일 수 있음.
        playerManager.InputManager.GetJoystickInputValue(out joystickInputData);
        playerManager.MovementManager.MoveByJoystick(joystickInputData);

        // 대기 상태일 때만 스킬이 사용 가능함. 스킬 사용 체크.
        InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

        if (inputBuffer.skillType != SkillSlot.None)
        {
            playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }
}
