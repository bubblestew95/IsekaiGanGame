using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

using EnumTypes;
using StructTypes;
using Unity.Netcode;
using Unity.Netcode.Components;

/// <summary>
/// 플레이어 캐릭터를 총괄적으로 관리하는 매니저.
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
    private PlayerMovementManager movementManager = null;

            #endregion

            #region Components

    private CharacterController characterCont = null;
    private Animator animator = null;

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

    public PlayerMovementManager MovementManager
    {
        get { return movementManager; }
    }

        #endregion

    #endregion

    #region Public Functions

        #region State Functions

    /// <summary>
    /// 현재 플레이어의 동작 상태를 변경한다.
    /// </summary>
    /// <param name="_type">변경하고자 하는 동작 상태.</param>
    public void ChangeState(PlayerStateType _type)
    {
        Debug.LogFormat("Player State Change To {0}.", _type);
        StateMachine.ChangeState(_type);
    }

        #endregion

        #region Skill Functions

    /// <summary>
    /// 스킬 애니메이션이 시작할 때 호출되는 함수.
    /// </summary>
    public void StartSkill(SkillSlot _type)
    {
        skillManager.GetSkill(_type).StartSkill(this);
    }

    /// <summary>
    /// 스킬 애니메이션이 정상적으로 끝났을 때 호출되는 함수. 
    /// </summary>
    public void EndSkill(SkillSlot _type)
    {
        Debug.LogFormat("End Skill type {0}", _type);
        skillManager.GetSkill(_type).EndSkill(this);
    }

    /// <summary>
    /// 스킬 애니메이션 중 스킬이 실제로 사용될 때 호출되는 함수.
    /// </summary>
    public void UseSkill(SkillSlot _slot)
    {
        skillManager.SkillAction(_slot);
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
        stateMachine.AddState(PlayerStateType.Damaged, new DamagedState(this));
        stateMachine.AddState(PlayerStateType.Dash, new DashState(this));
    }

    /// <summary>
    /// 플레이어의 매니저 클래스들을 생성하고 초기화한다.
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

        movementManager = new PlayerMovementManager();
        movementManager.Init(this);
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

        // 현재 상태에 따른 행동을 업데이트한다.
        stateMachine.UpdateState();

        skillManager.DecreaseCoolTimes(Time.deltaTime);
    }

    #endregion
}
