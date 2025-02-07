using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

using EnumTypes;
using StructTypes;
using Unity.Netcode;
using Unity.Netcode.Components;

/// <summary>
/// �÷��̾� ĳ���͸� �Ѱ������� �����ϴ� �Ŵ���.
/// </summary>
public class PlayerManager : NetworkBehaviour
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
    private Transform rangeAttackStartTr = null;
    [SerializeField]
    private MeleeWeapon meleeWeapon = null;
    #endregion

    #region InputBuffer

    private Queue<InputBufferData> skillBuffer = new Queue<InputBufferData>();
    private readonly float checkDequeueTime = 0.05f;
    private float remainDequeueTime = 0f;

    #endregion

    #region Private Variables

    private PlayerInputManager playerInputManager = null;
    private CharacterController characterCont = null;
    private PlayerSkillManager skillMng = null;
    private StatusManager statusMng = null;
    private PlayerStateMachine stateMachine = null;
    private PlayerAttackManager attackManager = null;
    private PlayerAnimationManager animationManager = null;
    private PlayerNetworkController networkController = null;
    private Animator animator = null;

    private int animId_Speed = 0;

    private Vector3 lastSkillUsePoint = Vector3.zero;

    private InputBufferData nullInputBuffer = new InputBufferData();
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

    public Vector3 LastSkillUsePoint
    {
        get { return lastSkillUsePoint; }
    }

    public StatusManager StatusManager
    {
        get { return statusMng; }
    }

    public UIBattleUIManager BattleUIManager
    {
        get { return battleUIManager; }
    }

    public PlayerAnimationManager AnimationManager
    {
        get { return animationManager; }
    }

    public PlayerNetworkController NetworkController
    {
        get { return networkController; }
    }

    #endregion

    #region Public Functions

    #region Input, State Functions

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

        if(moveVector != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(moveVector);
    }

    /// <summary>
    /// ���� �÷��̾��� ���� ���¸� �����Ѵ�.
    /// </summary>
    /// <param name="_type">�����ϰ��� �ϴ� ���� ����.</param>
    public void ChangeState(PlayerStateType _type)
    {
        Debug.LogFormat("Player State Change To {0}.", _type);
        StateMachine.ChangeState(_type);
    }

    public void SetAnimatorWalkSpeed(float _speed)
    {
        animator.SetFloat(animId_Speed, _speed);
    }

    #endregion

    #region Skill Functions

    /// <summary>
    /// ��ų �ߵ��� �õ��Ѵ�.
    /// </summary>
    /// <param name="_skillIdx"></param>
    public void TryUseSkill(SkillSlot _type, SkillPointData _point)
    {
        // ��ų �ߵ��� �����ߴٸ�
        if (skillMng.IsSkillUsable(_type))
        {
            // ĳ���͸� ����Ʈ�� ������ ������ ������ �Ѵ�.
            if (_point.type == SkillPointType.Position || _point.type == SkillPointType.None)
            {
                transform.LookAt(_point.point);
            }
            else
            {
                transform.rotation = Quaternion.Euler(_point.point);
            }

            skillMng.UseSkill(_type);

            // UI�� ��Ÿ���� �����Ѵ�.
            if (battleUIManager != null)
                battleUIManager.ApplyCooltime(_type, skillMng.GetCoolTime(_type));
        }
    }

    public PlayerSkillBase GetSkill(SkillSlot _slot)
    {
        return skillMng.GetSkill(_slot);
    }

    /// <summary>
    /// ��ų �ִϸ��̼��� ������ �� ȣ��Ǵ� �Լ�.
    /// </summary>
    public void StartSkill(SkillSlot _type)
    {
        GetSkill(_type).StartSkill(this);
    }

    /// <summary>
    /// ��ų �ִϸ��̼��� ���������� ������ �� ȣ��Ǵ� �Լ�. 
    /// </summary>
    public void EndSkill(SkillSlot _type)
    {
        Debug.LogFormat("End Skill type {0}", _type);
        GetSkill(_type).EndSkill(this);
    }

    /// <summary>
    /// ��ų �ִϸ��̼� �� ��ų�� ������ ���� �� ȣ��Ǵ� �Լ�.
    /// </summary>
    public void UseSkill(SkillSlot _slot)
    {
        skillMng.SkillAction(_slot);
    }

    /// <summary>
    /// ������ ��ų�� ��� �������� ��ų�Ŵ������� �˾ƿ��� �Լ�.
    /// </summary>
    /// <param name="_type">������ ��ų�� Ÿ��</param>
    /// <returns>��� ���� ����</returns>
    public bool IsSkillUsable(SkillSlot _type)
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
    public void EnableMeleeAttack(int _damage, float _aggro)
    {
        meleeWeapon.Init(_damage, _aggro);
        meleeWeapon.SetTriggerEnabled(true);
    }

    public void DisableMeleeAttack()
    {
        meleeWeapon.SetTriggerEnabled(false);
    }

    public Vector3 GetMeleeWeaponPostion()
    {
        return meleeWeapon.transform.position;
    }

    /// <summary>
    /// �÷��̾ �������� ����.
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(int _damage, Vector3 _attackOriginPos, float _distance)
    {
        if(stateMachine.CurrentState.StateType == PlayerStateType.Damaged
            ||
            stateMachine.CurrentState.StateType == PlayerStateType.Death
            ||
            stateMachine.CurrentState.StateType == PlayerStateType.Dash)
        {
            Debug.Log("Player is not damageable!");
            return;
        }

        statusMng.OnDamaged(_damage);
        MovePlayer(_attackOriginPos, _distance);
        ChangeState(PlayerStateType.Damaged);
    }

    public void MovePlayer(Vector3 _attackOriginPos, float _distance)
    {
        StartCoroutine(KnockBackCoroutine(_attackOriginPos, _distance));
    }

    public void AddDamageToBoss(int _damage, float _aggro)
    {
        attackManager.AddDamageToBoss(_damage, _aggro);
    }

    public bool IsPlayerBehindBoss()
    {
        if (Vector3.Angle(transform.forward, GameManager.Instance.GetBossTransform().forward) < 80f)
        {
            return true;
        }

        return false;
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
        stateMachine.AddState(PlayerStateType.Damaged, new DamagedState(this));
        stateMachine.AddState(PlayerStateType.Dash, new DashState(this));
    }

    /// <summary>
    /// ������ �ð����� ��ų �Է� ���ۿ��� �Է¸� �ϳ��� ������ ó���� �Ѵ�.
    /// </summary>
    private void PopSkillInputBuffer()
    {
        if (skillBuffer.Count > 0 && remainDequeueTime > 0f)
            remainDequeueTime -= Time.deltaTime;

        if (remainDequeueTime <= 0f)
        {
            remainDequeueTime = checkDequeueTime;
            if (skillBuffer.Count > 0)
                skillBuffer.Dequeue();
        }

    }

    #region Coroutines

    private IEnumerator KnockBackCoroutine(Vector3 _attackOriginPos, float _distance)
    {
        float knockbackTime = 0.5f;
        float currentTime = 0f;
        float speed = _distance / knockbackTime;

        Vector3 direction =  transform.position - _attackOriginPos;
        direction.y = 0f;
        direction.Normalize();

        Debug.LogFormat("Direction : {0}, speed : {1}", direction, speed);

        while (currentTime <= knockbackTime)
        {
            characterCont.Move(direction * speed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }

    #endregion

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
        playerInputManager.Init(battleUIManager.MoveJoystick);

        attackManager = new PlayerAttackManager();
        attackManager.Init(this);

        animationManager = new PlayerAnimationManager();
        animationManager.Init(this);

        networkController = new PlayerNetworkController();
        networkController.Init(this);

        animator = GetComponent<Animator>();

        animId_Speed = Animator.StringToHash("Speed");

        InitStates();
    }

    private void Start()
    {
        stateMachine.ChangeState(PlayerStateType.Idle);

        if (GetComponent<NetworkObject>().IsOwner)
        {
            battleUIManager.transform.parent.gameObject.SetActive(true);
            characterCont.enabled = true;
        }

    }

    private void Update()
    {
        PopSkillInputBuffer();

        // ���� ���¿� ���� �ൿ�� ������Ʈ�Ѵ�.
        stateMachine.UpdateState();

        skillMng.DecreaseCoolTimes(Time.deltaTime);
    }

    #endregion
}
