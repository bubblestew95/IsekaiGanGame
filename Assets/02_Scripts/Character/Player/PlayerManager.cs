using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using EnumTypes;
using StructTypes;

/// <summary>
/// 플레이어 캐릭터를 총괄적으로 관리하는 매니저.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    #region Variables

    [HideInInspector]
    public UnityEvent OnPlayerDead = new UnityEvent();

    #region Inspector Variables

    [SerializeField]
    private PlayerData playerData = null;
    [SerializeField]
    private UIBattleUIManager battleUIManager = null;
    [SerializeField]
    private SkillUIManager skillUIManager = null;

    [SerializeField]
    private Transform rangeAttackStartTr = null;
    #endregion

    #region InputBuffer

    private Queue<SkillType> skillBuffer = new Queue<SkillType>();
    private readonly float checkDequeueTime = 0.05f;
    private float remainDequeueTime = 0f;

    #endregion

    #region Private Variables

    private FloatingJoystick joystick = null;
    private PlayerInputManager playerInputManager = null;
    private CharacterController characterCont = null;
    private PlayerSkillManager skillMng = null;
    private StatusManager statusMng = null;
    private PlayerStateMachine stateMachine = null;
    private PlayerAttackManager attackManager = null;
    private Animator animator = null;
    private int animId_Speed = 0;

    #endregion

    #endregion

    #region Properties
    public PlayerStateMachine StateMachine
    {
        get { return stateMachine; }
    }

    public PlayerData PlayerData
    {
        get {  return playerData; }
    }

    public PlayerInputManager InputManager
    {
        get { return playerInputManager; }
    }

    public Transform RangeAttackStartTr
    {
        get { return rangeAttackStartTr; }
    }
    #endregion

    #region Public Functions

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
    public void MoveByJoystick(JoystickInputData _inputData)
    {
        float speed = playerData.walkSpeed;

        Vector3 moveVector = new Vector3(_inputData.x, 0f, _inputData.z) * speed * Time.deltaTime;

        characterCont.Move(moveVector);

        float currentSpeed = moveVector.sqrMagnitude;

        SetAnimatorWalkSpeed(currentSpeed);

        if (currentSpeed == 0f)
            return;

        transform.rotation = Quaternion.LookRotation(moveVector);
    }

    /// <summary>
    /// 현재 플레이어의 동작 상태를 변경한다.
    /// </summary>
    /// <param name="_type">변경하고자 하는 동작 상태.</param>
    public void ChangeState(PlayerStateType _type)
    {
        StateMachine.ChangeState(_type);
    }

    public void SetAnimatorWalkSpeed(float _speed)
    {
        animator.SetFloat(animId_Speed, _speed);
    }

    #region Skill Functions

    /// <summary>
    /// 스킬 발동을 시도한다.
    /// </summary>
    /// <param name="_skillIdx"></param>
    public void TryUseSkill(SkillType _type)
    {
        // 스킬 사용 지점을 스킬 UI 매니저에게서 받아온다.
        Vector3 point = skillUIManager.GetSkillAimPoint(_type);

        // 스킬 발동에 성공했다면
        if (skillMng.TryUseSkill(_type, point))
        {
            // UI에 쿨타임을 적용한다.
            if (battleUIManager != null)
                battleUIManager.ApplyCooltime(_type, skillMng.GetCoolTime(_type));
        }
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
    
    public PlayerSkillBase GetSkill(SkillType _type)
    {
        return skillMng.GetSkill(_type);
    }

    /// <summary>
    /// 스킬 애니메이션이 시작할 때 호출되는 함수.
    /// </summary>
    public void StartSkill(SkillType _type)
    {
        GetSkill(_type).StartSkill(this);
    }

    /// <summary>
    /// 스킬 애니메이션이 정상적으로 끝났을 때 호출되는 함수. 
    /// </summary>
    public void EndSkill(SkillType _type)
    {
        GetSkill(_type).EndSkill(this);
    }

    /// <summary>
    /// 스킬 애니메이션 중 스킬 사용 타이밍 때 스킬의 효과를 씬에 적용시킨다.
    /// </summary>
    public void UseSkill(SkillType _type)
    {
        skillMng.SkillAction(_type, 1f);
    }

    /// <summary>
    /// 지정한 스킬이 사용 가능한지 스킬매니저에서 알아오는 함수.
    /// </summary>
    /// <param name="_type">지정할 스킬의 타입</param>
    /// <returns>사용 가능 여부</returns>
    public bool IsSkillUsable(SkillType _type)
    {
        if(skillMng == null)
        {
            Debug.LogError("Skill Manager is not valid!");
            return false;
        }

        return skillMng.IsSkillUsable(_type);
    }

    #endregion

    #region Attack Functions

    public void RayAttack(float _damage, float _maxDistance)
    {
        attackManager.RayAttack(_damage, _maxDistance);
    }

    #endregion

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

        skillMng = new PlayerSkillManager();
        skillMng.Init(this);

        statusMng = new StatusManager();
        statusMng.Init(this);

        playerInputManager = new PlayerInputManager();
        playerInputManager.Init(FindAnyObjectByType<FloatingJoystick>());

        attackManager = new PlayerAttackManager();
        attackManager.Init(this);

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
