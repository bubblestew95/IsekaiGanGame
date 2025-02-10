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

    #region Input Buffer

    private Queue<InputBufferData> skillBuffer = new Queue<InputBufferData>();
    private readonly float checkDequeueTime = 0.05f;
    private float remainDequeueTime = 0f;
    private InputBufferData nullInputBuffer = new InputBufferData();

    #endregion

    private Vector3 lastSkillUsePoint = Vector3.zero;

    public Vector3 LastSkillUsePoint
    {
        get { return lastSkillUsePoint; }
    }

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
    /// 정해진 시간마다 스킬 입력 버퍼에서 입력를 하나씩 빼서 삭제하는 처리를 한다.
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
            lastSkillUsePoint = nextBuffer.pointData.point;
            return nextBuffer;
        }

        return nullInputBuffer;
    }

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
        }
    }

    #endregion
}
