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

        #region Private Variables

            #region Manager References

    private PlayerInputManager playerInputManager = null;
    private PlayerSkillManager skillManager = null;
    private StatusManager statusManager = null;
    private PlayerStateMachine stateMachine = null;
    private PlayerAttackManager attackManager = null;
    private PlayerAnimationManager animationManager = null;
    private PlayerNetworkController networkController = null;

            #endregion

            #region Components

    private CharacterController characterCont = null;
    private Animator animator = null;

            #endregion

            #region Variables

    private Vector3 lastSkillUsePoint = Vector3.zero;

            #endregion

        #endregion

    #endregion

    #region Properties

    public PlayerData PlayerData
    {
        get { return playerData; }
    }

    public Transform RangeAttackStartTr
    {
        get { return rangeAttackStartTr; }
    }

    public Vector3 LastSkillUsePoint
    {
        get { return lastSkillUsePoint; }
    }

    public MeleeWeapon PlayerMeleeWeapon
    {
        get { return meleeWeapon; }
    }

        #region Manager References

    public PlayerStateMachine StateMachine
    {
        get { return stateMachine; }
    }

    public PlayerInputManager InputManager
    {
        get { return playerInputManager; }
    }

    public StatusManager StatusManager
    {
        get { return statusManager; }
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

    public PlayerSkillManager SkillManager
    {
        get { return skillManager; }
    }

    public PlayerAttackManager AttackManager
    {
        get { return attackManager; }
    }

        #endregion

    #endregion

    #region Public Functions

        #region Move, State Functions

    /// <summary>
    /// ���̽�ƽ �Է��� �ް� �������� ó���Ѵ�.
    /// </summary>
    public void MoveByJoystick(JoystickInputData _inputData)
    {
        float speed = playerData.walkSpeed;

        Vector3 moveVector = new Vector3(_inputData.x, 0f, _inputData.z) * speed * Time.deltaTime;

        characterCont.Move(moveVector);

        float currentSpeed = moveVector.sqrMagnitude;

        AnimationManager.SetAnimatorWalkSpeed(currentSpeed);

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

        #endregion

        #region Skill Functions

    /// <summary>
    /// ��ų �ִϸ��̼��� ������ �� ȣ��Ǵ� �Լ�.
    /// </summary>
    public void StartSkill(SkillSlot _type)
    {
        skillManager.GetSkill(_type).StartSkill(this);
    }

    /// <summary>
    /// ��ų �ִϸ��̼��� ���������� ������ �� ȣ��Ǵ� �Լ�. 
    /// </summary>
    public void EndSkill(SkillSlot _type)
    {
        Debug.LogFormat("End Skill type {0}", _type);
        skillManager.GetSkill(_type).EndSkill(this);
    }

    /// <summary>
    /// ��ų �ִϸ��̼� �� ��ų�� ������ ���� �� ȣ��Ǵ� �Լ�.
    /// </summary>
    public void UseSkill(SkillSlot _slot)
    {
        skillManager.SkillAction(_slot);
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
    /// �÷��̾��� �Ŵ��� Ŭ�������� �����ϰ� �ʱ�ȭ�Ѵ�.
    /// </summary>
    private void InitManagers()
    {
        skillManager = new PlayerSkillManager();
        skillManager.Init(this);

        statusManager = new StatusManager();
        statusManager.Init(this);

        playerInputManager = new PlayerInputManager();
        playerInputManager.Init(battleUIManager.MoveJoystick);

        attackManager = new PlayerAttackManager();
        attackManager.Init(this);

        animationManager = new PlayerAnimationManager();
        animationManager.Init(this);

        networkController = new PlayerNetworkController();
        networkController.Init(this);
    }

    #endregion

    #region Unity Callback

    private void Awake()
    {
        characterCont = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        InitStates();

        InitManagers();
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
        InputManager.PopSkillInputBuffer();

        // ���� ���¿� ���� �ൿ�� ������Ʈ�Ѵ�.
        stateMachine.UpdateState();

        skillManager.DecreaseCoolTimes(Time.deltaTime);
    }

    #endregion
}
