using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using StructTypes;
using EnumTypes;

public class PlayerInputManager
{
    private PlayerManager playerManager = null;

    /// <summary>
    /// 가상 조이스틱 컴포넌트 지정.
    /// </summary>
    private FloatingJoystick joystick = null;

    private Coroutine skillUICoroutine = null;

    #region Input Buffer

    private Queue<InputBufferData> skillBuffer = new Queue<InputBufferData>();
    private readonly float checkDequeueTime = 0.05f;
    private float remainDequeueTime = 0f;
    private InputBufferData nullInputBuffer = new InputBufferData();

    #endregion

    public Vector3 lastSkillUsePoint = Vector3.zero;

    #region Public Func

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;

        joystick = _playerManager.BattleUIManager.MoveJoystick;
    }

    /// <summary>
    /// 가상 조이스틱의 입력 값을 받아온다.
    /// </summary>
    /// <param name="_inputData">가상 조이스틱의 입력 값을 저장할 구조체의 참조</param>
    public void GetJoystickInputValue(out JoystickInputData _inputData)
    {
        _inputData.x = joystick.Horizontal;
        _inputData.z = joystick.Vertical;
    }

    /// <summary>
    /// 정해진 시간마다 스킬 입력 버퍼에서 입력를 하나씩 빼서 삭제하는 처리를 하는 코루틴을 실행한다.
    /// </summary>
    public void StartInputBufferPop()
    {
        playerManager.StartCoroutine(PopInputBufferCoroutine());
    }

    /// <summary>
    /// 입력을 받았을 때 입력 버퍼에 해당 입력의 스킬 타입을 넣는다.
    /// </summary>
    /// <param name="_input">입력 버퍼에 Enqueue할 스킬 타입</param>
    public void OnButtonInput(SkillSlot _input, SkillPointData point)
    {
        InputBufferData inputBuffer = new InputBufferData();
        inputBuffer.skillType = _input;
        inputBuffer.pointData = point;

        skillBuffer.Enqueue(inputBuffer);

        // 만약 입력 버퍼가 비어있다가 새롭게 입력됐다면 Dequeue 시간을 측정하기 시작한다.
        if (skillBuffer.Count == 1)
            remainDequeueTime = checkDequeueTime;
    }

    /// <summary>
    /// 현재 스킬 입력 버퍼에서 하나를 꺼내옴.
    /// </summary>
    /// <returns>사용할 스킬의 타입</returns>
    public InputBufferData GetNextInput()
    {
        if (skillBuffer.TryDequeue(out InputBufferData nextBuffer))
        {
            lastSkillUsePoint = nextBuffer.pointData.skillUsedPosition;
            return nextBuffer;
        }

        return nullInputBuffer;
    }

    public bool GetMouseRayHitPosition(out Vector3 result)
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f))
        {
            result = hit.point;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    public void OnSkillKeyInput(SkillSlot _slot)
    {
        // 다른 스킬 UI가 활성화되어있을 경우 비활성화 처리
        {
            SkillSlot otherSkillSlot = SkillSlot.None;

            if (playerManager.SkillUIManager.IsOtherSkillUIEnabled(_slot, out otherSkillSlot))
            {
                playerManager.StopCoroutine(skillUICoroutine);
                skillUICoroutine = null;
                playerManager.SkillUIManager.SkillUIMap[otherSkillSlot].SetEnabled(false);
            }
        }

        // 대쉬 스킬일 경우 마우스 위치로 대쉬
        if (_slot == SkillSlot.Dash)
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

            return;
        }

        // 스킬 UI가 있을 경우
        if (playerManager.SkillUIManager.SkillUIMap.TryGetValue(_slot, out SkillUI_Base skillUI))
        {
            // 스킬 UI가 활성화되어있을 경우 해당 스킬 UI 위치로 스킬을 사용했다는 걸 입력하고
            // 스킬 UI 움직임을 담당하는 코루틴을 정지한다.
            if (skillUI.IsEnabled())
            {
                Debug.LogFormat("{0} Skill Input!", _slot);
                playerManager.InputManager.OnButtonInput
                    (_slot, skillUI.GetSkillAimPoint());
                playerManager.StopCoroutine(skillUICoroutine);
                skillUI.SetEnabled(false);
            }
            // 스킬 UI가 비활성화되어있을 경우 스킬 UI 활성화 및 스킬 UI 움직임 코루틴 시작.
            else
            {
                Debug.LogFormat("{0} Skill Ready!", _slot);
                skillUICoroutine = playerManager.
                    StartCoroutine(SkillAimCoroutine(skillUI));
            }
        }
        // 스킬 UI가 없을 경우 마우스 위치로 스킬 사용을 입력.
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

    /// <summary>
    /// 입력 버퍼에서 일정 시간마다 입력을 삭제하는 처리를 하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator PopInputBufferCoroutine()
    {
        float remainTime = 0f;

        while(true)
        {
            // 입력 버퍼가 비어있지 않고, Dequeue 시간이 남아있을 때만 시간을 감소시킨다.
            if (skillBuffer.Count > 0 && remainTime > 0f)
                remainTime -= Time.deltaTime;

            // Dequeue 시간이 다 되었을 때 Dequeue를 시도한다.
            if (remainTime <= 0f)
            {
                remainTime = checkDequeueTime;

                // Dequeue 시도
                if (skillBuffer.Count > 0)
                    skillBuffer.Dequeue();

                yield return null;
            }

            yield return null;
        }
    }


    private IEnumerator SkillAimCoroutine(SkillUI_Base _skillUI)
    {
        _skillUI.SetEnabled(true);

        while (true)
        {
            if (playerManager.InputManager.GetMouseRayHitPosition(out Vector3 pos))
            {
                _skillUI.AimSkill(pos);
            }

            yield return null;
        }
    }

    #endregion
}
