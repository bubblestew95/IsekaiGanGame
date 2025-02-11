using System.Collections;
using UnityEngine;
using Unity.Netcode;

using EnumTypes;
using StructTypes;

public class IdleState : BasePlayerState
{
    private JoystickInputData joystickInputData;
    private NetworkObject networkObj = null;
    private Coroutine updateCoroutine = null;

    public IdleState(PlayerManager _playerManager) : base(_playerManager)
    {
        stateType = PlayerStateType.Idle;

        networkObj = playerManager.GetComponent<NetworkObject>();
    }

    public override void OnEnterState()
    {
        // ���� ������ �ƴϰ�, ��Ʈ��ũ ������Ʈ�� �������� ������ ���� �ʴٸ� ����.
        if (!GameManager.Instance.IsLocalGame && !networkObj.IsOwner)
            return;

        if(GameManager.Instance.IsPCMode)
        {
            updateCoroutine = playerManager.StartCoroutine(IdlePCCoroutine());
        }
        else
        {
            updateCoroutine = playerManager.StartCoroutine(IdleMobileCoroutine());
        }

    }

    public override void OnExitState()
    {
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);

        if(updateCoroutine != null)
        {
            playerManager.StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
    }

    public override void OnUpdateState()
    {
    }

    private IEnumerator IdleMobileCoroutine()
    {
        while(true)
        {
            // ��� ������ ���� ������ �� ����.
            playerManager.InputManager.GetJoystickInputValue(out joystickInputData);
            playerManager.MovementManager.MoveByJoystick(joystickInputData);

            // ��� ������ ���� ��ų�� ��� ������. ��ų ��� üũ.
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
            }

            yield return null;
        }
    }

    private IEnumerator IdlePCCoroutine()
    {
        Camera mainCamera = Camera.main;

        while(true)
        {
            // ���� �� ���콺 �Է��� �������� �� �̵� ������ ���� ���콺�� ���� ��
            if(Input.GetMouseButtonDown(1) && 
                playerManager.InputManager.GetMouseRayHitPosition(out Vector3 mouseRayPosition))
            {
                playerManager.MovementManager.MoveToPosition(mouseRayPosition);
            }

            // ��� ������ ���� ��ų�� ��� ������. ��ų ��� üũ.
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
            }

            yield return null;
        }
    }
}
