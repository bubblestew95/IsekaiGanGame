using UnityEngine;

using EnumTypes;
using UnityEngine.Events;

/// <summary>
/// �÷��̾� ĳ���͸� �Ѱ������� �����ϴ� �Ŵ���.
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
    [SerializeField]
    private SkillUIManager skillUIManager = null;

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

    public PlayerNetworkManager PlayerNetworkManager
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

    public SkillUIManager SkillUIManager
    {
        get { return skillUIManager; }
    }

        #endregion

    #endregion

    #region Public Functions

        #region State Functions

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
        skillManager.GetSkillData(_type).StartSkill(this);
    }

    /// <summary>
    /// ��ų �ִϸ��̼��� ���������� ������ �� ȣ��Ǵ� �Լ�. 
    /// </summary>
    public void EndSkill(SkillSlot _type)
    {
        Debug.LogFormat("End Skill type {0}", _type);
        skillManager.GetSkillData(_type).EndSkill(this);
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
        // �⺻ ���¸� ��� ���·� �����Ѵ�.
        stateMachine.ChangeState(PlayerStateType.Idle);

        // ���� �����̰ų�, ��Ʈ��ũ ������Ʈ�� �������� ��쿡�� Ȱ��ȭ�Ѵ�.
        if (GameManager.Instance.IsLocalGame || PlayerNetworkManager.IsOwner)
        {
            // ���� UI�� Ȱ��ȭ�ϰ�, ĳ���� ��Ʈ�ѷ��� Ȱ��ȭ�Ѵ�.
            battleUIManager.transform.parent.gameObject.SetActive(true);
            characterController.enabled = true;

            // �Է� ������ ������ �����Ѵ�.
            InputManager.StartInputBufferPop();
        }
    }

    private void Update()
    {
        // ���� ���¿� ���� �ൿ�� ������Ʈ�Ѵ�.
        stateMachine.UpdateState();

        skillManager.DecreaseCoolTimes(Time.deltaTime);
    }

    #endregion
}
