using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using EnumTypes;
using StructTypes;

/// <summary>
/// �÷��̾� ĳ���͸� �Ѱ������� �����ϴ� �Ŵ���.
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
    /// �Է��� �޾��� �� �Է� ���ۿ� �ش� �Է��� ��ų Ÿ���� �ִ´�.
    /// </summary>
    /// <param name="_input">�Է� ���ۿ� Enqueue�� ��ų Ÿ��</param>
    public void OnButtonInput(SkillType _input)
    {
        skillBuffer.Enqueue(_input);

        // ���� �Է� ���۰� ����ִٰ� ���Ӱ� �Էµƴٸ� Dequeue �ð��� �����ϱ� �����Ѵ�.
        if (skillBuffer.Count == 1)
            remainDequeueTime = checkDequeueTime;
    }

    /// <summary>
    /// ���̽�ƽ �Է��� �ް� �������� ó���Ѵ�.
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
    /// ���� �÷��̾��� ���� ���¸� �����Ѵ�.
    /// </summary>
    /// <param name="_type">�����ϰ��� �ϴ� ���� ����.</param>
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
    /// ��ų �ߵ��� �õ��Ѵ�.
    /// </summary>
    /// <param name="_skillIdx"></param>
    public void TryUseSkill(SkillType _type)
    {
        // ��ų ��� ������ ��ų UI �Ŵ������Լ� �޾ƿ´�.
        Vector3 point = skillUIManager.GetSkillAimPoint(_type);

        // ��ų �ߵ��� �����ߴٸ�
        if (skillMng.TryUseSkill(_type, point))
        {
            // UI�� ��Ÿ���� �����Ѵ�.
            if (battleUIManager != null)
                battleUIManager.ApplyCooltime(_type, skillMng.GetCoolTime(_type));
        }
    }

    /// <summary>
    /// ���� ��ų �Է� ���ۿ��� �ϳ��� ������.
    /// </summary>
    /// <returns>����� ��ų�� Ÿ��</returns>
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
    /// ��ų �ִϸ��̼��� ������ �� ȣ��Ǵ� �Լ�.
    /// </summary>
    public void StartSkill(SkillType _type)
    {
        GetSkill(_type).StartSkill(this);
    }

    /// <summary>
    /// ��ų �ִϸ��̼��� ���������� ������ �� ȣ��Ǵ� �Լ�. 
    /// </summary>
    public void EndSkill(SkillType _type)
    {
        GetSkill(_type).EndSkill(this);
    }

    /// <summary>
    /// ��ų �ִϸ��̼� �� ��ų ��� Ÿ�̹� �� ��ų�� ȿ���� ���� �����Ų��.
    /// </summary>
    public void UseSkill(SkillType _type)
    {
        skillMng.SkillAction(_type, 1f);
    }

    /// <summary>
    /// ������ ��ų�� ��� �������� ��ų�Ŵ������� �˾ƿ��� �Լ�.
    /// </summary>
    /// <param name="_type">������ ��ų�� Ÿ��</param>
    /// <returns>��� ���� ����</returns>
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
    /// ���� �ӽ��� �ʱ�ȭ�Ѵ�.
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
    /// ������ �ð����� ��ų �Է� ���ۿ��� �Է¸� �ϳ��� ������ ó���� �Ѵ�.
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

        // ���� ���¿� ���� �ൿ�� ������Ʈ�Ѵ�.
        stateMachine.UpdateState();

        // MoveByJoystick();

        skillMng.DecreaseCoolTimes(Time.deltaTime);
    }

    #endregion
}
