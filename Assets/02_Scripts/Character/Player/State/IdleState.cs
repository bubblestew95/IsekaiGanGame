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
        // ���� ������ �ƴϰ�, ��Ʈ��ũ ������Ʈ�� �������� ������ ���� �ʴٸ� ����.
        if (!isLocalGame && !networkObj.IsOwner)
            return;

        // ��� ������ ���� ������ �� ����.
        playerManager.InputManager.GetJoystickInputValue(out joystickInputData);
        playerManager.MovementManager.MoveByJoystick(joystickInputData);

        // ��� ������ ���� ��ų�� ��� ������. ��ų ��� üũ.
        InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

        if (inputBuffer.skillType != SkillSlot.None)
        {
            playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
        }
    }
}
