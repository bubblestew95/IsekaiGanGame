using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

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
    private Transform[] randomPos;

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
        curState = MushState.Idle;
        previousBehavior = MushState.Idle;
        isCoroutineRunning = false;
        isDie = false;
        patternDelay = 2f;
        curCoroutine = null;
        randomPos = new Transform[4];

        anim = FindAnyObjectByType<Animator>();
        nvAgent = FindAnyObjectByType<NavMeshAgent>();
        mushStateManager = FindAnyObjectByType<MushStateManager>();
        mushSkillManager = FindAnyObjectByType<BossSkillManager>();

        GameObject pos = GameObject.Find("RandomPos");
        randomPos[0] = pos.transform.GetChild(0);
        randomPos[1] = pos.transform.GetChild(1);
        randomPos[2] = pos.transform.GetChild(2);
        randomPos[3] = pos.transform.GetChild(3);
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

    private IEnumerator Attack1()
    {
        Debug.Log(curState.ToString());

        isCoroutineRunning = true;
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in mushSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // attack1 �ִϸ��̼� ��
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool("Attack1-1", true);
                break;
            }

            yield return null;
        }

        // attack1-1 �ִϸ��̼� ��
        while (true)
        {
            if (CheckEndAnim("Attack1-1"))
            {
                SetAnimBool("Attack1", false);
                SetAnimBool("Attack1-1", false);
                break;
            }

            yield return null;
        }

        // ���¸� chase�� ����
        curState = MushState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack2()
    {
        Debug.Log(curState.ToString());

        isCoroutineRunning = true;
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in mushSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // �ִϸ��̼� ��
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
                break;
            }

            yield return null;
        }

        // ���¸� chase�� ����
        curState = MushState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack3()
    {
        Debug.Log(curState.ToString());

        isCoroutineRunning = true;
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in mushSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        float elapseTime = 0f;
        Vector3 originPos = mushStateManager.Boss.transform.position;
        Quaternion originRot = mushStateManager.Boss.transform.rotation;
        Vector3 targetPos = RandomPos().transform.position;
        Quaternion targetRot = RandomPos().transform.rotation;

        // �ִϸ��̼� attack3
        while (true)
        {
            // ���� �̵�
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= 0.5f && elapseTime <= 1.6f)
                {
                    float t = Mathf.InverseLerp(0.5f, 1.6f, elapseTime);
                    mushStateManager.Boss.transform.position = Vector3.Lerp(originPos, targetPos, t);
                    mushStateManager.Boss.transform.rotation = Quaternion.Slerp(originRot, targetRot, t);
                }
            }

            if (CheckEndAnim(curState))
            {
                SetAnimBool("Attack3-1", true);
                break;
            }

            yield return null;
        }

        elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;
            mushStateManager.Boss.transform.LookAt(mushStateManager.AggroPlayer.transform);

            if (elapseTime >= 2f)
            {
                break;
            }
            yield return null;
        }

        // �ִϸ��̼� attack3-1
        while (true)
        {
            if (CheckEndAnim("Attack3-1"))
            {
                SetAnimBool("Attack3-2", true);
                break;
            }

            yield return null;
        }

        // �ִϸ��̼� attack3-2
        while (true)
        {
            if (CheckEndAnim("Attack3-2"))
            {
                SetAnimBool("Attack3", false);
                SetAnimBool("Attack3-1", false);
                SetAnimBool("Attack3-2", false);
                break;
            }

            yield return null;
        }

        // ���¸� chase�� ����
        curState = MushState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack4()
    {
        Debug.Log(curState.ToString());

        isCoroutineRunning = true;
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in mushSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }



        float elapseTime = 0f;
        float animSpd = 1f;
        mushStateManager.SetRandomPlayer();
        mushStateManager.Boss.transform.LookAt(mushStateManager.RandomPlayer.transform);
        Vector3 originPos = mushStateManager.Boss.transform.position;
        Vector3 targetPos = mushStateManager.RandomPlayer.transform.position;

        // attack4
        while (true)
        {
            // ���� �̵�
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= (0.5f / animSpd) && elapseTime <= (1.6f / animSpd))
                {
                    float t = Mathf.InverseLerp(0.5f, 1.6f, elapseTime);
                    mushStateManager.Boss.transform.position = Vector3.Lerp(originPos, targetPos, t);
                }
            }

            if (CheckEndAnim(curState))
            {
                SetAnimBool("Attack4-1", true);
                break;
            }

            yield return null;
        }

        elapseTime = 0f;
        animSpd = 1.5f;
        mushStateManager.SetRandomPlayer();
        mushStateManager.Boss.transform.LookAt(mushStateManager.RandomPlayer.transform);
        originPos = mushStateManager.Boss.transform.position;
        targetPos = mushStateManager.RandomPlayer.transform.position;

        // �ִϸ��̼� attack4-1
        while (true)
        {
            // ���� �̵�
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-1"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= (0.5f / animSpd) && elapseTime <= (1.6f / animSpd))
                {
                    float t = Mathf.InverseLerp((0.5f / animSpd), (1.6f / animSpd), elapseTime);
                    mushStateManager.Boss.transform.position = Vector3.Lerp(originPos, targetPos, t);
                }
            }

            if (CheckEndAnim("Attack4-1"))
            {
                SetAnimBool("Attack4-2", true);
                break;
            }

            yield return null;
        }

        elapseTime = 0f;
        animSpd = 2f;
        mushStateManager.SetRandomPlayer();
        mushStateManager.Boss.transform.LookAt(mushStateManager.RandomPlayer.transform);
        originPos = mushStateManager.Boss.transform.position;
        targetPos = mushStateManager.RandomPlayer.transform.position;

        // �ִϸ��̼� attack4-2
        while (true)
        {
            // ���� �̵�
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-2"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= (0.5f / animSpd) && elapseTime <= (1.6f / animSpd))
                {
                    float t = Mathf.InverseLerp((0.5f / animSpd), (1.6f / animSpd), elapseTime);
                    mushStateManager.Boss.transform.position = Vector3.Lerp(originPos, targetPos, t);
                }
            }

            if (CheckEndAnim("Attack4-2"))
            {
                SetAnimBool("Attack4-3", true);
                break;
            }

            yield return null;
        }

        elapseTime = 0f;
        animSpd = 2.5f;
        mushStateManager.SetRandomPlayer();
        mushStateManager.Boss.transform.LookAt(mushStateManager.RandomPlayer.transform);
        originPos = mushStateManager.Boss.transform.position;
        targetPos = mushStateManager.RandomPlayer.transform.position;

        // �ִϸ��̼� attack4-3
        while (true)
        {
            // ���� �̵�
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-3"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= (0.5f / animSpd) && elapseTime <= (1.6f / animSpd))
                {
                    float t = Mathf.InverseLerp((0.5f / animSpd), (1.6f / animSpd), elapseTime);
                    mushStateManager.Boss.transform.position = Vector3.Lerp(originPos, targetPos, t);
                }
            }

            if (CheckEndAnim("Attack4-3"))
            {
                SetAnimBool("Attack4", false);
                SetAnimBool("Attack4-1", false);
                SetAnimBool("Attack4-2", false);
                SetAnimBool("Attack4-3", false);
                break;
            }

            yield return null;
        }

        // ���¸� chase�� ����
        curState = MushState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack5()
    {
        Debug.Log(curState.ToString());

        isCoroutineRunning = true;
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in mushSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        float elapseTime = 0f;
        mushStateManager.Boss.transform.LookAt(mushStateManager.AggroPlayer.transform);
        Vector3 originPos = mushStateManager.Boss.transform.position;
        Vector3 targetPos = mushStateManager.AggroPlayer.transform.position;

        // �ִϸ��̼� ��
        while (true)
        {
            // ���� �̵�
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= 0.5f && elapseTime <= 1.6f)
                {
                    float t = Mathf.InverseLerp(0.5f, 1.6f, elapseTime);
                    mushStateManager.Boss.transform.position = Vector3.Lerp(originPos, targetPos, t);
                }
            }

            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
                break;
            }

            yield return null;
        }

        // ���¸� chase�� ����
        curState = MushState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack6()
    {
        Debug.Log(curState.ToString());

        isCoroutineRunning = true;
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in mushSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // �ִϸ��̼� ��
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
                break;
            }

            yield return null;
        }

        // ���¸� chase�� ����
        curState = MushState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
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

    private void SetAnimBool(string _stateName, bool _isActive)
    {
        anim.SetBool(_stateName + "Flag", _isActive);
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

    // �ִϸ��̼� �������� check�ϴ� ���ǹ�
    private bool CheckEndAnim(string _stateName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(_stateName) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region [Function]

    // Random�� ��ġ �����ϴ� �Լ�
    private Transform RandomPos()
    {
        return randomPos[UnityEngine.Random.Range(0, 4)];
    }

    #endregion
}
