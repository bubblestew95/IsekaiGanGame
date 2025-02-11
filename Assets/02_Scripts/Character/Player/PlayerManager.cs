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
public class PlayerManager : MonoBehaviour
{
    #region Variables

        #region Inspector Variables

    [Header("References")]
    [SerializeField]
    private PlayerData playerData = null;
    [SerializeField]
    private UIBattleUIManager battleUIManager = null;

    [Header("Attack Settings")]
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
    private PlayerMovementManager movementManager = null;

            #endregion

            #region Components

    private CharacterController characterController = null;
    private PlayerNetworkManager playerNetworkManager = null;

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

    public PlayerNetworkManager NetworkController
    {
        get { return playerNetworkManager; }
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
        skillManager.GetSkillData(_type).StartSkill(this);
    }

    /// <summary>
    /// 스킬 애니메이션이 정상적으로 끝났을 때 호출되는 함수. 
    /// </summary>
    public void EndSkill(SkillSlot _type)
    {
        Debug.LogFormat("End Skill type {0}", _type);
        skillManager.GetSkillData(_type).EndSkill(this);
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
        playerInputManager.Init(this);

        attackManager = new PlayerAttackManager();
        attackManager.Init(this);

        animationManager = new PlayerAnimationManager();
        animationManager.Init(this);

        movementManager = new PlayerMovementManager();
        movementManager.Init(this);
    }

    #endregion

    #region Unity Callback

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        playerNetworkManager = GetComponent<PlayerNetworkManager>();

        InitStates();

        InitManagers();
    }

    private void Start()
    {
        // 기본 상태를 대기 상태로 설정한다.
        stateMachine.ChangeState(PlayerStateType.Idle);

        // 로컬 게임이거나, 네트워크 오브젝트의 소유자일 경우에만 활성화한다.
        if (GameManager.Instance.IsLocalGame || NetworkController.IsOwner)
        {
            // 전투 UI를 활성화하고, 캐릭터 컨트롤러를 활성화한다.
            battleUIManager.transform.parent.gameObject.SetActive(true);
            characterController.enabled = true;

            // 입력 버퍼의 갱신을 시작한다.
            InputManager.StartInputBufferPop();
        }
    }

    private void Update()
    {
        // 현재 상태에 따른 행동을 업데이트한다.
        stateMachine.UpdateState();

        skillManager.DecreaseCoolTimes(Time.deltaTime);
    }

    #endregion
}
