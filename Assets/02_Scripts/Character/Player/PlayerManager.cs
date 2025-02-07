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

        #region InputBuffer

    private Queue<InputBufferData> skillBuffer = new Queue<InputBufferData>();
    private readonly float checkDequeueTime = 0.05f;
    private float remainDequeueTime = 0f;

        #endregion

        #region Private Variables

            #region Manager References

    private PlayerInputManager playerInputManager = null;
    private PlayerSkillManager skillMng = null;
    private StatusManager statusMng = null;
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

    private int animId_Speed = 0;
    private Vector3 lastSkillUsePoint = Vector3.zero;
    private InputBufferData nullInputBuffer = new InputBufferData();

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

    public PlayerSkillManager SkillManager
    {
        get { return skillMng; }
    }

        #endregion

    #endregion

    #region Public Functions

        #region Move, State Functions

    /// <summary>
    /// 조이스틱 입력을 받고 움직임을 처리한다.
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
        skillMng.GetSkill(_type).StartSkill(this);
    }

    /// <summary>
    /// 스킬 애니메이션이 정상적으로 끝났을 때 호출되는 함수. 
    /// </summary>
    public void EndSkill(SkillSlot _type)
    {
        Debug.LogFormat("End Skill type {0}", _type);
        skillMng.GetSkill(_type).EndSkill(this);
    }

    /// <summary>
    /// 스킬 애니메이션 중 스킬이 실제로 사용될 때 호출되는 함수.
    /// </summary>
    public void UseSkill(SkillSlot _slot)
    {
        skillMng.SkillAction(_slot);
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
    /// 플레이어가 데미지를 받음.
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
        KnockbackPlayer(_attackOriginPos, _distance);
        ChangeState(PlayerStateType.Damaged);
    }

    public void KnockbackPlayer(Vector3 _attackOriginPos, float _distance)
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

        #region Coroutines

    /// <summary>
    /// 플레이어를 넉백시키는 코루틴
    /// </summary>
    /// <param name="_attackOriginPos"></param>
    /// <param name="_distance"></param>
    /// <returns></returns>
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
        InputManager.PopSkillInputBuffer();

        // 현재 상태에 따른 행동을 업데이트한다.
        stateMachine.UpdateState();

        skillMng.DecreaseCoolTimes(Time.deltaTime);
    }

    #endregion
}
