using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

// 보스의 행동에 맞게 Animation 재생 및 Transform 이동
public class MushBT : NetworkBehaviour
{
    // 델리게이트
    public static event Action AttackStartCallback;
    public static event Action AttackEndCallback;
    public delegate void BTDelegate();
    public BTDelegate behaviorEndCallback;

    // 참조 할것들
    private Animator anim;
    private NavMeshAgent nvAgent;
    private MushStateManager mushStateManager;
    private BossSkillManager mushSkillManager;

    // 보스BT 데이터
    private MushState curState;
    private MushState previousBehavior;
    private bool isCoroutineRunning;
    private bool isDie;
    private float patternDelay;
    private Coroutine curCoroutine;

    // 프로퍼티
    public MushState CurState { get { return curState; } set { curState = value; } }

    // 초기화
    private void Awake()
    {
        anim = FindAnyObjectByType<Animator>();
        nvAgent = FindAnyObjectByType<NavMeshAgent>();
        mushStateManager = FindAnyObjectByType<MushStateManager>();
        mushSkillManager = FindAnyObjectByType<BossSkillManager>();

        curState = MushState.Idle;
        previousBehavior = MushState.Idle;
        isCoroutineRunning = false;
        isDie = false;
        patternDelay = 2f;
        curCoroutine = null;
    }

    private void Update()
    {
        if (IsServer)
        {
            // 죽었을때
            if (curState == MushState.Die && !isDie)
            {
                StopCoroutine(curCoroutine);
                StartCoroutine(curState.ToString());

                isDie = true;
            }

            // 아무것도 아닐때
            if (!isCoroutineRunning)
            {
                curCoroutine = StartCoroutine(curState.ToString());
            }
        }
    }

    #region [BossBehavior]
    private IEnumerator Idle()
    {
        yield return null;
    }

    private IEnumerator Chase()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // 따라 다니기 시작
        anim.SetBool("ChaseFlag", true);

        float elapseTime = 0f;

        // 상태가 바뀌고 or 1초마다 콜백 
        while (true)
        {
            nvAgent.SetDestination(mushStateManager.AggroPlayer.transform.position);
            elapseTime += Time.deltaTime;

            if (curState != MushState.Chase)
            {
                break;
            }

            if (elapseTime >= patternDelay)
            {
                elapseTime = 0f;
                behaviorEndCallback?.Invoke();
            }

            yield return null;
        }

        // 따라 다니기 멈춤
        anim.SetBool("ChaseFlag", false);
        nvAgent.ResetPath();

        isCoroutineRunning = false;
    }

    private IEnumerator Die()
    {
        anim.Play("Die");

        yield return null;
    }

    #endregion

    #region [Anim]
    // 애니메이션 bool값 설정
    private void SetAnimBool(MushState _state, bool _isActive)
    {
        anim.SetBool(_state.ToString() + "Flag", _isActive);
    }

    // 애니메이션 끝났는지 check하는 조건문
    private bool CheckEndAnim(MushState _state)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(_state.ToString()) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion
}
