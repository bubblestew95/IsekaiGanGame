using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

// ������ �ൿ�� �°� Animation ��� �� Transform �̵�
public class MushBT : NetworkBehaviour
{
    // ��������Ʈ
    public static event Action AttackStartCallback;
    public static event Action AttackEndCallback;
    public delegate void BTDelegate();
    public BTDelegate behaviorEndCallback;

    // ���� �Ұ͵�
    private Animator anim;
    private NavMeshAgent nvAgent;
    private MushStateManager mushStateManager;
    private BossSkillManager mushSkillManager;

    // ����BT ������
    private MushState curState;
    private MushState previousBehavior;
    private bool isCoroutineRunning;
    private bool isDie;
    private float patternDelay;
    private Coroutine curCoroutine;

    // ������Ƽ
    public MushState CurState { get { return curState; } set { curState = value; } }

    // �ʱ�ȭ
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
            // �׾�����
            if (curState == MushState.Die && !isDie)
            {
                StopCoroutine(curCoroutine);
                StartCoroutine(curState.ToString());

                isDie = true;
            }

            // �ƹ��͵� �ƴҶ�
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

        // ���� �ٴϱ� ����
        anim.SetBool("ChaseFlag", true);

        float elapseTime = 0f;

        // ���°� �ٲ�� or 1�ʸ��� �ݹ� 
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

        // ���� �ٴϱ� ����
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
    // �ִϸ��̼� bool�� ����
    private void SetAnimBool(MushState _state, bool _isActive)
    {
        anim.SetBool(_state.ToString() + "Flag", _isActive);
    }

    // �ִϸ��̼� �������� check�ϴ� ���ǹ�
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
