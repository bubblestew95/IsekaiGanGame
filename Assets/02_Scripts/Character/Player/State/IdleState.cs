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
    private Coroutine skillUICoroutine = null;

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

    private void UseSkill(SkillSlot _slot)
    {
        if (playerManager.SkillUIManager.SkillUIMap.TryGetValue(_slot, out SkillUI_Base skillUI))
        {
            if (skillUI as SkillUI_AOE)
            {
                if (skillUI.IsEnabled())
                {
                    playerManager.InputManager.OnButtonInput
                        (SkillSlot.Skill_A, skillUI.GetSkillAimPoint());
                    playerManager.StopCoroutine(skillUICoroutine);
                    skillUI.SetEnabled(false);
                }
                else
                {
                    skillUICoroutine = playerManager.
                        StartCoroutine(SkillAOEPositionCoroutine(skillUI));
                }
            }
            else if (skillUI as SkillUI_Direction)
            {
                if (skillUI.IsEnabled())
                {
                    playerManager.InputManager.OnButtonInput
                        (SkillSlot.Skill_A, skillUI.GetSkillAimPoint());
                    playerManager.StopCoroutine(skillUICoroutine);
                    skillUI.SetEnabled(false);
                }
                else
                {
                    skillUICoroutine = playerManager.
                        StartCoroutine(SkillAOEPositionCoroutine(skillUI));
                }
            }
        }
        else
        {
            SkillPointData data = new SkillPointData();
            data.type = SkillPointType.None;

            if (playerManager.InputManager.GetMouseRayHitPosition(out Vector3 mousePos))
            {
                Vector3 direction = (mousePos - playerManager.transform.position).normalized;
                data.skillUsedRotation = Quaternion.LookRotation(direction);
                playerManager.InputManager.OnButtonInput(_slot, data);
            }
        }
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
                // ������ ��ġ�� �̵�.
                playerManager.MovementManager.MoveToPosition(mouseRayPosition);
            }

            // ��ų �Է� ó��
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    UseSkill(SkillSlot.Skill_A);
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    UseSkill(SkillSlot.Skill_B);
                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    UseSkill(SkillSlot.Skill_C);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SkillPointData data = new SkillPointData();
                    data.type = SkillPointType.Direction;

                    if (playerManager.InputManager.GetMouseRayHitPosition(out Vector3 mousePos))
                    {
                        Vector3 direction = (mousePos - playerManager.transform.position).normalized;
                        data.skillUsedPosition = mousePos;
                        data.skillUsedRotation = Quaternion.LookRotation(direction);
                        playerManager.InputManager.OnButtonInput(SkillSlot.Dash, data);
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    UseSkill(SkillSlot.BasicAttack);
                }
            }

            // ��� ������ ���� ��ų�� ��� ������. ��ų ��� üũ.
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
                playerManager.MovementManager.StopMove();
            }

            yield return null;
        }
    }

    private IEnumerator SkillAOEPositionCoroutine(SkillUI_Base _skillUI)
    {
        _skillUI.SetEnabled(true);

        while (true)
        {
            if(playerManager.InputManager.GetMouseRayHitPosition(out Vector3 pos))
            {
                _skillUI.AimSkill(pos);
            }

            yield return null;
        }
    }
}
