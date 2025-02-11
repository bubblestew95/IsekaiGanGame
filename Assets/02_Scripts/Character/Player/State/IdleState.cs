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
        // 로컬 게임이 아니고, 네트워크 오브젝트가 소유권을 가지고 있지 않다면 리턴.
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
            // 대기 상태일 때만 움직일 수 있음.
            playerManager.InputManager.GetJoystickInputValue(out joystickInputData);
            playerManager.MovementManager.MoveByJoystick(joystickInputData);

            // 대기 상태일 때만 스킬이 사용 가능함. 스킬 사용 체크.
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
            // 오른 쪽 마우스 입력을 감지했을 때 이동 가능한 곳에 마우스가 있을 시
            if(Input.GetMouseButtonDown(1) && 
                playerManager.InputManager.GetMouseRayHitPosition(out Vector3 mouseRayPosition))
            {
                // 지정한 위치로 이동.
                playerManager.MovementManager.MoveToPosition(mouseRayPosition);
            }

            // 스킬 입력 처리
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {

                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {

                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SkillPointData data = new SkillPointData();
                    data.type = SkillPointType.Direction;
                    if(playerManager.InputManager.GetMouseRayHitPosition(out Vector3 mousePos))
                    {
                        Vector3 direction = (mousePos - playerManager.transform.position).normalized;
                        data.skillUsedRotation = Quaternion.LookRotation(direction);
                        playerManager.InputManager.OnButtonInput(SkillSlot.Dash, data);
                    }
                        
                }

                if (Input.GetMouseButtonDown(0))
                {

                }
            }

            // 대기 상태일 때만 스킬이 사용 가능함. 스킬 사용 체크.
            InputBufferData inputBuffer = playerManager.InputManager.GetNextInput();

            if (inputBuffer.skillType != SkillSlot.None)
            {
                playerManager.MovementManager.StopMove();
                playerManager.SkillManager.TryUseSkill(inputBuffer.skillType, inputBuffer.pointData);
            }

            yield return null;
        }
    }
}
