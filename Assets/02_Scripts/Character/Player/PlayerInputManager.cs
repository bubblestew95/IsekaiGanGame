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

    private Coroutine skillUICoroutine = null;
    private int groundLayerMask = 0;

    #region Input Buffer

    private Queue<InputBufferData> skillBuffer = new Queue<InputBufferData>();
    private readonly float checkDequeueTime = 0.3f;
    private float remainDequeueTime = 0f;
    private InputBufferData nullInputBuffer = new InputBufferData();

    #endregion

    public Vector3 lastSkillUsePoint = Vector3.zero;

    #region Public Func

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;

        if(_playerManager.BattleUIManager != null)
            joystick = _playerManager.BattleUIManager.MoveJoystick;
        groundLayerMask = LayerMask.GetMask("Ground");
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
    /// ������ �ð����� ��ų �Է� ���ۿ��� �Է¸� �ϳ��� ���� �����ϴ� ó���� �ϴ� �ڷ�ƾ�� �����Ѵ�.
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
            lastSkillUsePoint = nextBuffer.pointData.skillUsedPosition;
            return nextBuffer;
        }

        return nullInputBuffer;
    }

    public bool GetMouseRayHitPosition(out Vector3 result)
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f, groundLayerMask))
        {
            result = hit.point;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    /// <summary>
    /// PC ȯ�濡�� �÷��̾� ĳ������ ��ų �Է��� ó���ϴ� �Լ�.
    /// </summary>
    /// <param name="_slot"></param>
    public void PC_OnSkillKeyInput(SkillSlot _slot)
    {
        // �ٸ� ��ų UI�� Ȱ��ȭ�Ǿ����� ��� ��Ȱ��ȭ ó��
        {
            SkillSlot otherSkillSlot = SkillSlot.None;

            if (playerManager.SkillUIManager.IsOtherSkillUIEnabled(_slot, out otherSkillSlot))
            {
                playerManager.StopCoroutine(skillUICoroutine);
                skillUICoroutine = null;
                playerManager.SkillUIManager.SkillUIMap[otherSkillSlot].SetEnabled(false);
            }
        }

        // �뽬 ��ų�� ��� ���콺 ��ġ�� �ٷ� �뽬
        if (_slot == SkillSlot.Dash)
        {
            SkillPointData data = new SkillPointData();
            data.type = SkillPointType.Direction;

            if (playerManager.InputManager.GetMouseRayHitPosition(out Vector3 mousePos))
            {
                Vector3 direction = mousePos - playerManager.transform.position;
                direction.y = 0f;
                direction.Normalize();
                data.skillUsedPosition = mousePos;
                data.skillUsedRotation = Quaternion.LookRotation(direction);
                playerManager.InputManager.OnButtonInput(SkillSlot.Dash, data);
            }

            return;
        }

        // ��ų UI�� ���� ���
        if (playerManager.SkillUIManager.SkillUIMap.TryGetValue(_slot, out SkillUI_Base skillUI))
        {
            // ��ų UI�� Ȱ��ȭ�Ǿ����� ��� �ش� ��ų UI ��ġ�� ��ų�� ����ߴٴ� �� �Է��ϰ�
            // ��ų UI �������� ����ϴ� �ڷ�ƾ�� �����Ѵ�.
            if (skillUI.IsEnabled())
            {
                playerManager.InputManager.OnButtonInput
                    (_slot, skillUI.GetSkillAimPoint());
                playerManager.StopCoroutine(skillUICoroutine);
                skillUI.SetEnabled(false);
            }
            // ��ų UI�� ��Ȱ��ȭ�Ǿ����� ��� ��ų UI Ȱ��ȭ �� ��ų UI�� �����̴� �ڷ�ƾ ����.
            else
            {
                skillUICoroutine = playerManager.
                    StartCoroutine(PC_SkillAimCoroutine(skillUI));
            }
        }
        // ��ų UI�� ���� ��� ���콺 ��ġ�� ��ų ����� �Է�.
        else
        {
            SkillPointData data = new SkillPointData();
            data.type = SkillPointType.None;

            if (playerManager.InputManager.GetMouseRayHitPosition(out Vector3 mousePos))
            {
                Vector3 direction = mousePos - playerManager.transform.position;
                direction.y = 0f;
                direction.Normalize();
                data.skillUsedRotation = Quaternion.LookRotation(direction);
                playerManager.InputManager.OnButtonInput(_slot, data);
            }
        }
    }

    /// <summary>
    /// �Է� ���ۿ��� ���� �ð����� �Է��� �����ϴ� ó���� �ϴ� �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    private IEnumerator PopInputBufferCoroutine()
    {
        while(true)
        {
            // �Է� ���۰� ������� �ʰ�, Dequeue �ð��� �������� ���� �ð��� ���ҽ�Ų��.
            if (skillBuffer.Count > 0 && remainDequeueTime > 0f)
                remainDequeueTime -= Time.deltaTime;

            // Dequeue �ð��� �� �Ǿ��� �� Dequeue�� �õ��Ѵ�.
            if (remainDequeueTime <= 0f)
            {
                remainDequeueTime = checkDequeueTime;

                // Dequeue �õ�
                if (skillBuffer.Count > 0)
                    skillBuffer.Dequeue();
            }

            yield return null;
        }
    }

    private IEnumerator PC_SkillAimCoroutine(SkillUI_Base _skillUI)
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
