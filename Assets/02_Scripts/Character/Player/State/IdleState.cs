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

        //if(GameManager.Instance.IsPCMode)
        //{
        //    updateCoroutine = playerManager.StartCoroutine(IdlePCCoroutine());
        //}
        //else
        //{
        //    updateCoroutine = playerManager.StartCoroutine(IdleMobileCoroutine());
        //}
        */

        mainCamera = Camera.main;
        mouseRayPosition = Vector3.zero;
    }

    public override void OnExitState()
    {
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);

        //if(updateCoroutine != null)
        //{
        //    playerManager.StopCoroutine(updateCoroutine);
        //    updateCoroutine = null;
        //}
    }

    public override void OnUpdateState()
    {
        // ���� ������ �ƴϰ�, ��Ʈ��ũ ������Ʈ�� ���� �÷��̾ �ƴ� ���� ��� �� �Է� ó���� ���� �ʴ´�.
        if (!GameManager.Instance.IsLocalGame && !playerManager.PlayerNetworkManager.IsClientPlayer())
            return;

#if UNITY_ANDROID

        // ��� ������ ���� ������ �� ����.
        playerManager.InputManager.GetJoystickInputValue(out joystickInputData);
        playerManager.MovementManager.MoveByJoystick(joystickInputData);

#elif UNITY_STANDALONE_WIN || UNITY_EDITOR

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

        // ��ų ��� ó��
        {
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
            }
        }
    }

    /*
    /// <summary>
    /// ��� ������ �� ����� �Է� ó���� �ϴ� �ڷ�ƾ.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// ��� ������ �� PC �Է¿� ���� ó���� �ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    private IEnumerator IdlePCCoroutine()
    {
        Camera mainCamera = Camera.main;
        Vector3 mouseRayPosition = Vector3.zero;
        while (true)
        {
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

            // ��ų ��� ó��
            {
                InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

                if (inputBuffer.skillType != SkillSlot.None)
                {
                    playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
                }
            }

            yield return null;

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
        }
    }
    */
}
