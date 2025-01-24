using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using EnumTypes;

/// <summary>
/// 플레이어 캐릭터를 총괄적으로 관리하는 매니저.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent OnPlayerDead = new UnityEvent();

    [SerializeField]
    private PlayerData playerData = null;
    [SerializeField]
    private UIBattleUIManager battleUIManager = null;

    private FloatingJoystick joystick = null;

    private Queue<SkillType> skillBuffer = new Queue<SkillType>();
    private readonly float checkDequeueTime = 0.05f;
    private float remainDequeueTime = 0f;

    private CharacterController characterCont = null;
    private PlayerSkillManager skillMng = null;
    private StatusManager statusMng = null;
    private PlayerStateMachine stateMachine = null;
    private Animator animator = null;

    private int animId_Speed = 0;

    public PlayerStateMachine StateMachine
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
        // 스킬 발동에 성공했다면
        if(skillMng.TryUseSkill(_type))
        {
            // UI에 쿨타임을 적용한다.
            if (battleUIManager != null)
                battleUIManager.ApplyCooltime(_type, skillMng.GetCoolTime(_type));
        }
    }

    /// <summary>
    /// 입력을 받았을 때 입력 버퍼에 해당 입력의 스킬 타입을 넣는다.
    /// </summary>
    /// <param name="_input">입력 버퍼에 Enqueue할 스킬 타입</param>
    public void OnButtonInput(SkillType _input)
    {
        skillBuffer.Enqueue(_input);

        // 만약 입력 버퍼가 비어있다가 새롭게 입력됐다면 Dequeue 시간을 측정하기 시작한다.
        if (skillBuffer.Count == 1)
            remainDequeueTime = checkDequeueTime;
    }

    /// <summary>
    /// 조이스틱 입력을 받고 움직임을 처리한다.
    /// </summary>
    public void MoveByJoystick()
    {
        if (joystick == null)
        {
            return;
        }

        float x = joystick.Horizontal;
        float z = joystick.Vertical;
        float speed = playerData.walkSpeed;

        Vector3 moveVector = new Vector3(x, 0f, z) * speed * Time.deltaTime;

        characterCont.Move(moveVector);

        float currentSpeed = moveVector.sqrMagnitude;

        SetAnimatorWalkSpeed(currentSpeed);

        if (currentSpeed == 0f)
            return;

        transform.rotation = Quaternion.LookRotation(moveVector);
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

    public void EndSkill()
    {
        ChangeState(PlayerStateType.Idle);
    }

    public void ChangeState(PlayerStateType _type)
    {
        StateMachine.ChangeState(_type);
    }

    public void SetAnimatorWalkSpeed(float _speed)
    {
        animator.SetFloat(animId_Speed, _speed);
    }

    #endregion

    #region Private Functions

    /// <summary>
    /// 상태 머신을 초기화한다.
    /// </summary>
    private void InitStates()
    {
        stateMachine = new PlayerStateMachine();

        stateMachine.AddState(PlayerStateType.Idle, new IdleState(this));
        stateMachine.AddState(PlayerStateType.Action, new ActionState(this));
        stateMachine.AddState(PlayerStateType.Death, new DeathState(this));
        stateMachine.AddState(PlayerStateType.Stagger, new StaggerState(this));
        stateMachine.AddState(PlayerStateType.Dash, new DashState(this));
    }

    /// <summary>
    /// 정해진 시간마다 스킬 입력 버퍼에서 입력를 하나씩 빼내는 처리를 한다.
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

    #region Unity Callback

    private void Awake()
    {
        characterCont = GetComponent<CharacterController>();

        joystick = FindAnyObjectByType<FloatingJoystick>();

        skillMng = new PlayerSkillManager();
        skillMng.Init(this);

        statusMng = new StatusManager();
        statusMng.Init(this);

        animator = GetComponent<Animator>();

        animId_Speed = Animator.StringToHash("Speed");

        InitStates();
    }

    private void Start()
    {
        stateMachine.ChangeState(PlayerStateType.Idle);
    }

    private void Update()
    {
        CheckSkillInputBuffer();

        // 현재 상태에 따른 행동을 업데이트한다.
        stateMachine.UpdateState();

        // MoveByJoystick();

        skillMng.DecreaseCoolTimes(Time.deltaTime);
    }

    #endregion
}
