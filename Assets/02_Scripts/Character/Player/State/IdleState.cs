using System.Collections;
using UnityEngine;
using Unity.Netcode;

using EnumTypes;
using StructTypes;

public class IdleState : BasePlayerState
{
    private JoystickInputData joystickInputData;
    private Coroutine updateCoroutine = null;
    private Camera mainCamera = null;
    private Vector3 mouseRayPosition = Vector3.zero;

    public IdleState(PlayerManager _playerManager) : base(_playerManager)
    {
        stateType = PlayerStateType.Idle;
    }

    public override void OnEnterState()
    {
        /*
        //// ���� ������ �ƴϰ�, ��Ʈ��ũ ������Ʈ�� ���� �÷��̾ �ƴ� ��
        //if (!GameManager.Instance.IsLocalGame && !playerManager.PlayerNetworkManager.IsClientPlayer())
        //    return;
        */

        mainCamera = Camera.main;
        mouseRayPosition = Vector3.zero;
    }

    public override void OnExitState()
    {
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
    }

    public override void OnUpdateState()
    {
        // ���� ������ �ƴϰ�, ��Ʈ��ũ ������Ʈ�� ���� �÷��̾ �ƴ� ���� ��� �� �Է� ó���� ���� �ʴ´�.
        if (!GameManager.Instance.IsLocalGame && !playerManager.PlayerNetworkManager.IsClientPlayer())
            return;

#if UNITY_ANDROID

        // ��ų ��� ó��
        {
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                if (playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData))
                {
                    if (inputBuffer.skillType != SkillSlot.Dash && inputBuffer.skillType != SkillSlot.BasicAttack)
                        skillUsedTrigger = true;
                }

                return;
            }
        }

        if (!skillUsedTrigger)
        {
            // ��� ������ ���� ������ �� ����.
            playerManager.InputManager.GetJoystickInputValue(out joystickInputData);
            playerManager.MovementManager.MoveByJoystick(joystickInputData);
        }

#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        // ��ų ��� ó��
        {
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
            }
        }
        // ��ų �Է� ó��
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Skill_A);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Skill_B);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Skill_C);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.Dash);
            }

            if (Input.GetMouseButtonDown(0))
            {
                playerManager.InputManager.PC_OnSkillKeyInput(SkillSlot.BasicAttack);
            }
        }

        // �÷��̾� �̵� ó��
        {
            // ���� �� ���콺 �Է��� �������� �� �̵� ������ ���� ���콺�� ���� ��
            if (Input.GetMouseButton(1))
            {
                // ���콺�� ��ġ�� �̵� ������ ������ ���� ���� �̵�.
                if (playerManager.InputManager.GetMouseRayHitPosition(out mouseRayPosition))
                {
                    // ������ ��ġ�� �̵�.
                    playerManager.MovementManager.MoveToPosition(mouseRayPosition);
                }
                // ���콺�� ��ġ�� �̵� ������ ������ ������ �̵� ����.
                else
                {
                    playerManager.MovementManager.StopMove();
                    playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
                }
            }
            // ���콺 Ŭ���� ���߸� �̵� ����.
            if (Input.GetMouseButtonUp(1))
            {
                playerManager.MovementManager.StopMove();
                playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
            }
        }

#endif
    }
}
