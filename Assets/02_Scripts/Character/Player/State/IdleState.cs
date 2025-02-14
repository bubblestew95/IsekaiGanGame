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
        // ���� ������ �ƴϰ�, ��Ʈ��ũ ������Ʈ�� ���� �÷��̾ �ƴ� ��
        if (!GameManager.Instance.IsLocalGame && !playerManager.PlayerNetworkManager.IsLocalPlayer)
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
                    mouseRayPosition = Vector3.zero;
                    if (playerManager.InputManager.GetMouseRayHitPosition(out mouseRayPosition))
                    {
                        // ������ ��ġ�� �̵�.
                        playerManager.MovementManager.MoveToPosition(mouseRayPosition);
                    }
                    else
                    {
                        playerManager.MovementManager.StopMove();
                    }
                }
                if (Input.GetMouseButtonUp(1))
                {
                    playerManager.MovementManager.StopMove();
                }
            }
        }
    }
}
