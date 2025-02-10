using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using StructTypes;
using EnumTypes;

public class PlayerInputManager
{
    private PlayerManager playerManager = null;

    /// <summary>
    /// ���� ���̽�ƽ ������Ʈ ����.
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
    /// ���� ���̽�ƽ�� �Է� ���� �޾ƿ´�.
    /// </summary>
    /// <param name="_inputData">���� ���̽�ƽ�� �Է� ���� ������ ����ü�� ����</param>
    public void GetJoystickInputValue(out JoystickInputData _inputData)
    {
        _inputData.x = joystick.Horizontal;
        _inputData.z = joystick.Vertical;
    }

    /// <summary>
    /// ������ �ð����� ��ų �Է� ���ۿ��� �Է¸� �ϳ��� ���� �����ϴ� ó���� �Ѵ�.
    /// </summary>
    public void StartInputBufferPop()
    {
        playerManager.StartCoroutine(PopInputBufferCoroutine());
    }

    /// <summary>
    /// �Է��� �޾��� �� �Է� ���ۿ� �ش� �Է��� ��ų Ÿ���� �ִ´�.
    /// </summary>
    /// <param name="_input">�Է� ���ۿ� Enqueue�� ��ų Ÿ��</param>
    public void OnButtonInput(SkillSlot _input, SkillPointData point)
    {
        InputBufferData inputBuffer = new InputBufferData();
        inputBuffer.skillType = _input;
        inputBuffer.pointData = point;

        skillBuffer.Enqueue(inputBuffer);

        // ���� �Է� ���۰� ����ִٰ� ���Ӱ� �Էµƴٸ� Dequeue �ð��� �����ϱ� �����Ѵ�.
        if (skillBuffer.Count == 1)
            remainDequeueTime = checkDequeueTime;
    }

    /// <summary>
    /// ���� ��ų �Է� ���ۿ��� �ϳ��� ������.
    /// </summary>
    /// <returns>����� ��ų�� Ÿ��</returns>
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
            // �Է� ���۰� ������� �ʰ�, Dequeue �ð��� �������� ���� �ð��� ���ҽ�Ų��.
            if (skillBuffer.Count > 0 && remainTime > 0f)
                remainTime -= Time.deltaTime;

            // Dequeue �ð��� �� �Ǿ��� �� Dequeue�� �õ��Ѵ�.
            if (remainTime <= 0f)
            {
                remainTime = checkDequeueTime;

                // Dequeue �õ�
                if (skillBuffer.Count > 0)
                    skillBuffer.Dequeue();

                yield return null;
            }
        }
    }

    #endregion
}
