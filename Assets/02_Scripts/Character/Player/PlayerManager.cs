using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

/// <summary>
/// 플레이어 캐릭터를 관리하는 매니저.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// 가상 조이스틱 컴포넌트 지정.
    /// </summary>
    [SerializeField]
    private FloatingJoystick joystick = null;
    [SerializeField]
    private PlayerData playerData = null;

    private Queue<SkillType> skillBuffer = new Queue<SkillType>();
    private readonly float checkDequeueTime = 0.05f;
    private float remainDequeueTime = 0f;

    private CharacterController characterCont = null;
    private PlayerSkillManager skillMng = null;
    private StateMachine stateMachine = null;

    public StateMachine StateMachine
    {
        get { return stateMachine; }
    }

    public PlayerData PlayerData
    {
        get {  return playerData; }
    }

    #region Public Functions
    /// <summary>
    /// 스킬 발동을 시도한다.
    /// </summary>
    /// <param name="_skillIdx"></param>
    public void UseSkill(SkillType _type)
    {
        skillMng.TryUseSkill(_type);
    }

    public void OnButtonInput(SkillType _input)
    {
        skillBuffer.Enqueue(_input);
        if (skillBuffer.Count == 1)
            remainDequeueTime = checkDequeueTime;
    }

    /// <summary>
    /// 현재 스킬 입력 버퍼에서 하나를 꺼내옴.
    /// </summary>
    /// <returns>사용할 스킬의 타입</returns>
    public SkillType GetNextSkill()
    {
        if (skillBuffer.TryDequeue(out SkillType nextSkillType))
            return nextSkillType;

        return SkillType.None;
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// 조이스틱 입력을 받고 움직임을 처리한다.
    /// </summary>
    private void MoveByJoystick()
    {
        float x = joystick.Horizontal;
        float z = joystick.Vertical;
        float speed = playerData.speed;

        Vector3 moveVector = new Vector3(x, 0f, z) * speed * Time.deltaTime;

        characterCont.Move(moveVector);

        if (moveVector.sqrMagnitude == 0f)
            return;

        transform.rotation = Quaternion.LookRotation(moveVector);
    }

    private void InitStates()
    {
        stateMachine = new StateMachine();

        stateMachine.AddState(PlayerStateType.Idle, new IdleState(this));
        stateMachine.AddState(PlayerStateType.Action, new ActionState(this));
        stateMachine.AddState(PlayerStateType.Death, new DeathState(this));
        stateMachine.AddState(PlayerStateType.Stagger, new StaggerState(this));
        stateMachine.AddState(PlayerStateType.Dash, new DashState(this));
    }

    #endregion

    #region Unity Callback

    private void Awake()
    {
        characterCont = GetComponent<CharacterController>();

        skillMng = new PlayerSkillManager();
        skillMng.Init(this);

        InitStates();
    }

    private void Start()
    {
        stateMachine.ChangeState(PlayerStateType.Idle);
    }

    private void Update()
    {
        CheckSkillInputBuffer();

        stateMachine.UpdateState();

        MoveByJoystick();

        skillMng.DecreaseCoolTimes(Time.deltaTime);
        Debug.Log(skillMng.GetCoolTime(SkillType.Skill_A));

        if(Input.GetKeyDown(KeyCode.L))
        {
            OnButtonInput(SkillType.Skill_A);
        }
    }

    /// <summary>
    /// 스킬 입력 버퍼에서 입력를 빼내는 처리를 한다.
    /// </summary>
    private void CheckSkillInputBuffer()
    {
        if (skillBuffer.Count > 0 && remainDequeueTime > 0f)
            remainDequeueTime -= Time.deltaTime;

        if (remainDequeueTime <= 0f)
        {
            Debug.Log("Dequeue!");
            remainDequeueTime = checkDequeueTime;
            if (skillBuffer.Count > 0)
                skillBuffer.Dequeue();
        }

    }

    #endregion
}
