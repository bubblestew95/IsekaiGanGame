using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossBT : NetworkBehaviour
{
    public static event Action AttackStartCallback;
    public static event Action AttackEndCallback;

    public delegate void BTDelegate();
    public BTDelegate behaviorEndCallback;
    public BTDelegate phase2BehaviorStartCallback;

    [SerializeField] public BossState curState;
    [SerializeField] private Animator anim;
    [SerializeField] private NavMeshAgent nvAgent;
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;
    [SerializeField] private BossAttackManager bossAttackManager;
    [SerializeField] private Vector3 center;
    [SerializeField] private BossPhase2Cam phase2Cam;
    [SerializeField] private BossPhaseSet phase2Set;

    private bool isCoroutineRunning = false;
    private bool isStun = false;
    private bool isDie = false;
    private bool isTriggerWall = false;
    private BossState previousBehavior;
    private float patternDelay = 2f;
    public Coroutine curCoroutine;

    public float PatternDelay { get { return patternDelay; } set { patternDelay = value; } }

    private void Start()
    {
        bossStateManager.bossWallTriggerCallback += ChangeMove;
    }

    private void Update()
    {
        if (IsServer)
        {
            // �׾�����
            if (curState == BossState.Die && !isDie)
            {
                StopCoroutine(curCoroutine);
                StartCoroutine(curState.ToString());

                isDie = true;
            }

            // ���� �ɷ�����
            if (curState == BossState.Stun && !isStun)
            {
                StopCoroutine(curCoroutine);
                StartCoroutine(curState.ToString());
                isStun = true;
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
            nvAgent.SetDestination(bossStateManager.AggroPlayer.transform.position);
            elapseTime += Time.deltaTime;

            if (curState != BossState.Chase)
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

        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
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
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack2()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
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
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack3()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
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
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack4()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Attack4 �غ��ǿ����� �ȵ���ٴϰ� ����
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        while (true)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // attack4�� ���ӽð��� ������.
        BSD_Duration attack4 = bossSkillManager.Skills
            .Where(skill => skill.SkillData.SkillName == "Attack4")
            .Select(skill => skill.SkillData as BSD_Duration)
            .FirstOrDefault(bsd => bsd != null);

        float duration = attack4.Duration;
        float elapseTime = 0f;
        float elapseTime2 = 0f;

        // �̵��ӵ� ����
        nvAgent.speed = 8f;

        // �ִϸ��̼� 4-1�� ���ӽð����� ����
        while (true)
        {
            // ��׷� �÷��̾� ����ٴ�
            if (bossStateManager.AggroPlayer == null)
            {
                Debug.Log("��׷� �÷��̾� null - Attack4");
            }
            else
            {
                nvAgent.SetDestination(bossStateManager.AggroPlayer.transform.position);
            }

            elapseTime += Time.deltaTime;
            if (elapseTime >= duration)
            {
                SetAnimBool(curState, false);
                nvAgent.ResetPath();
                nvAgent.speed = 3f;
                break;
            }
            yield return null;
        }

        // ���¸� chase�� ����
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack5()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ���� 5 ��
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5Flag", false);
                break;
            }

            yield return null;
        }

        float elapseTime = 0f;

        ulong randomNum = bossStateManager.RandomPlayer();
        GameObject target = bossStateManager.AlivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomNum);

        // ��� �ٸ� �÷��̾� �Ĵٺ��ٰ�
        while (true)
        {
            nvAgent.SetDestination(target.transform.position);
            elapseTime += Time.deltaTime;

            if (elapseTime >= 1f)
            {
                anim.SetBool("Attack5-1Flag", true);
                nvAgent.ResetPath();
                elapseTime = 0f;
                break;
            }

            yield return null;
        }

        // ���� 5-1
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-1Flag", false);
                break;
            }

            yield return null;
        }

        randomNum = bossStateManager.RandomPlayer();
        target = bossStateManager.AlivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomNum);

        // �Ǵٸ� �÷��̾� �Ĵٺ��ٰ�
        while (true)
        {
            nvAgent.SetDestination(target.transform.position);
            elapseTime += Time.deltaTime;

            if (elapseTime >= 1f)
            {
                anim.SetBool("Attack5-2Flag", true);
                nvAgent.ResetPath();
                elapseTime = 0f;
                break;
            }

            yield return null;
        }


        // ���� 5-2
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5-2") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-2Flag", false);
                break;
            }

            yield return null;
        }

        randomNum = bossStateManager.RandomPlayer();
        target = bossStateManager.AlivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomNum);

        // �Ǵٸ� �÷��̾� �Ĵٺ��ٰ� 
        while (true)
        {
            nvAgent.SetDestination(target.transform.position);
            elapseTime += Time.deltaTime;

            if (elapseTime >= 1f)
            {
                anim.SetBool("Attack5-3Flag", true);
                nvAgent.ResetPath();
                elapseTime = 0f;
                break;
            }

            yield return null;
        }

        // ���� 5-3
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5-3") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-3Flag", false);
                break;
            }

            yield return null;
        }

        // ����� �̵��ؼ� Ư������(�����)
        // ���� ��ġ���� ��� ��ġ�� Lerp�ϰ� �̵�(15~50������)�ϸ鼭, ���� �ִϸ��̼� �����ϸ� �ɵ�

        Vector3 originPos = bossStateManager.Boss.transform.position;

        while (true)
        {

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Jump"))
            {
                elapseTime += Time.deltaTime;

                if (elapseTime >= 0.5f && elapseTime <= 1.6f)
                {
                    float t = Mathf.InverseLerp(0.5f, 1.6f, elapseTime);
                    bossStateManager.Boss.transform.position = Vector3.Lerp(originPos, center, t);
                }
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Jump") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-4Flag", true);
                break;
            }

            yield return null;
        }

        // Roar(����� ����)
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Roar") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-4Flag", false);
                break;
            }

            yield return null;
        }

        // ���¸� chase�� ����
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack6()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

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
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();
    }

    private IEnumerator Attack7()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Chase -> Attack7 �Ѿ���� check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // Attack7�� �������� Check
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                Attack7BeforeClientRpc();

                // �����ִٰ� (�� ���� delay���� ������µ� ���� �ɸ��� �ð��� �M)
                yield return new WaitForSeconds(bossAttackManager.Delay - 1.1f);

                Attack7AfterClientRpc();

                // �ִϸ��̼� ��� �� skin, collider Ȱ��ȭ
                anim.SetBool("Attack7-1Flag", true);
                break;
            }
            yield return null;
        }

        // Attack 7-1�� �������� check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack7-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                SetAnimBool(curState, false);
                anim.SetBool("Attack7-1Flag", false);
                break;
            }
            yield return null;
        }


        // ���¸� chase�� ����
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();

        yield return null;
    }

    private IEnumerator Attack8()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Chase -> Attack8 �Ѿ���� check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // Attack8�� �������� Check
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                anim.SetBool("Attack8-1Flag", true);
                break;
            }
            yield return null;
        }

        // attack8�� ���ӽð��� ������.
        BSD_Duration attack8 = bossSkillManager.Skills
            .Where(skill => skill.SkillData.SkillName == "Attack8")
            .Select(skill => skill.SkillData as BSD_Duration)
            .FirstOrDefault(bsd => bsd != null);

        float duration = attack8.Duration;
        float elapseTime = 0f;

        // Attack 8-1�� �������� check
        while (true)
        {
            if (isTriggerWall)
            {
                bossStateManager.Boss.transform.LookAt(bossStateManager.AggroPlayer.transform.position);
                isTriggerWall = false;
                Debug.Log("������ȯ ȣ���");
            }

            elapseTime += Time.deltaTime;
            if (elapseTime >= duration)
            {
                SetAnimBool(curState, false);
                anim.SetBool("Attack8-1Flag", false);
                break;
            }
            yield return null;
        }

        // ���¸� chase�� ����
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
        AttackEndCallback?.Invoke();

        yield return null;
    }

    private IEnumerator Attack9()
    {
        isCoroutineRunning = true; 
        AttackStartCallback?.Invoke();
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Chase -> Attack9 �Ѿ���� check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // Attack9�� �������� Check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack9-1Flag", true);
                break;
            }
            yield return null;
        }

        // Attack 9-1�� �������� check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack9-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                SetAnimBool(curState, false);
                anim.SetBool("Attack9-1Flag", false);
                break;
            }
            yield return null;
        }

        // ���¸� chase�� ����
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false; 
        AttackEndCallback?.Invoke();

        yield return null;
    }

    private IEnumerator Stun()
    {
        isCoroutineRunning = true;

        // ���� �۾����� �ʱ�ȭ �ϴ� �ڵ�
        // 1. flag �ʱ�ȭ
        // 2. �̵��ӵ� �ʱ�ȭ
        // 3. ���� y���� �̻��Ҽ��� ������, y�� �ʱ�ȭ
        // 4. ��Ų�� collider ����ȭ
        ResetAnimBool();
        nvAgent.speed = 3f;
        bossStateManager.Boss.transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0f, bossStateManager.Boss.transform.position.z);
        nvAgent.ResetPath();

        // ����ȭ �ؾ��ϴ°�
        StunSyncClientRpc();

        // ���� �ִϸ��̼� ���� ���
        anim.Play("Stun");

        // ������
        yield return new WaitForSeconds(3f);


        StunSync2ClientRpc();

        if (previousBehavior != BossState.Attack8)
        {
            // ���¸� ���ϰɸ��� �� ���·�
            curState = previousBehavior;
        }
        else
        {
            curState = BossState.Chase;

            // ������ �������� �ݹ�
            behaviorEndCallback?.Invoke();
        }

        isCoroutineRunning = false;
        isStun = false;
    }

    private IEnumerator Phase2()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ����� �̵��ؼ� ��Ϳ�
        // ���� ��ġ���� ��� ��ġ�� Lerp�ϰ� �̵�(15~50������)�ϸ鼭, ���� �ִϸ��̼� �����ϸ� �ɵ�
        Vector3 originPos = bossStateManager.Boss.transform.position;

        float elapseTime = 0f;
        bool once = true;

        Quaternion targetRotation = Quaternion.Euler(0, -180, 0);

        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Phase2"))
            {

                if (once)
                {
                    once = false;
                    // ī�޶� �����̰�, ��� ���� �ݹ�
                    Phase2StartClientRpc();
                }

                // ���� ������ �����ϰ�
                elapseTime += Time.deltaTime;
                if (elapseTime >= 0.5f && elapseTime <= 1.6f)
                {
                    float t = Mathf.InverseLerp(0.5f, 1.6f, elapseTime);
                    bossStateManager.Boss.transform.position = Vector3.Lerp(originPos, center, t);

                    bossStateManager.Boss.transform.rotation = Quaternion.Slerp(bossStateManager.Boss.transform.rotation, targetRotation, t);
                }
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Phase2") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Phase2Flag", false);
                anim.SetBool("Phase2-1Flag", true);
                break;
            }

            yield return null;
        }

        once = true;

        // ��Ϳ�
        while (true)
        {
            // ��Ϳ� �ϴ� Ÿ�̹�
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Phase2-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.2f && once)
            {
                // ī�޶� ����
                once = false;
                Phase2RoarClientRpc();
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Phase2-1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                // ī�޶� ���󺹱�
                Phase2EndClientRpc();

                anim.SetBool("Phase2-1Flag", false);
                break;
            }

            yield return null;
        }

        // ���¸� chase�� ����
        curState = BossState.Chase;

        // ������ �������� �ݹ�
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }

    private IEnumerator Die()
    {
        anim.Play("Die");

        yield return null;
    }
    #endregion

    #region [Function]
    // �ִϸ��̼� bool�� ����
    private void SetAnimBool(BossState _state, bool _isActive)
    {
        anim.SetBool(_state.ToString() + "Flag", _isActive);
    }

    // �ִϸ��̼� �������� check�ϴ� ���ǹ�
    private bool CheckEndAnim(BossState _state)
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

    // �ִϸ��̼� ��� bool�� �ʱ�ȭ
    private void ResetAnimBool()
    {
        AnimatorControllerParameter[] parameters = anim.parameters;

        foreach (var parameter in parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(parameter.name, false);
            }
        }
    }

    private void ChangeMove()
    {
        if (anim.GetBool("Attack8Flag"))
        {
            isTriggerWall = true;
        }
    }
    #endregion

    #region [ClientRpc]

    [ClientRpc]
    private void Attack7BeforeClientRpc()
    {
        // boss�� skin�� ������� �ϱ�, collider�� ��� ��Ȱ��ȭ
        bossStateManager.BossSkin.SetActive(false);
        bossStateManager.HitCollider.enabled = false;

        // boss�� Tr�� ������ġ�� �ű��
        bossStateManager.Boss.transform.position = bossAttackManager.CircleSkillPos[0].transform.position;
    }

    [ClientRpc]
    private void Attack7AfterClientRpc()
    {
        bossStateManager.BossSkin.SetActive(true);
        bossStateManager.HitCollider.enabled = true; ;
    }

    // ���� ���� ����ȭ
    [ClientRpc]
    private void StunSyncClientRpc()
    {
        bossStateManager.BossSkin.SetActive(true);
        bossStateManager.HitCollider.enabled = true;
        bossStateManager.Boss.tag = "Untagged";
        SetAnimBool(BossState.Stun, true);
    }

    [ClientRpc]
    private void StunSync2ClientRpc()
    {
        SetAnimBool(BossState.Stun, false);
    }

    // ī�޶� �����̰�, ��� ���� �ݹ�
    [ClientRpc]
    private void Phase2StartClientRpc()
    {
        StartCoroutine(phase2Cam.MoveCam());
        phase2BehaviorStartCallback?.Invoke();
    }

    // ��Ϳ��Ҷ� ����ȭ ��ų�͵�
    [ClientRpc]
    private void Phase2RoarClientRpc()
    {
        // ī�޶� ����
        StartCoroutine(phase2Cam.ShakeCam());

        // Map Material ����(���⼭ Fire�׸�, ������ Fire�����ϴ� �۾�����), ���� Mat ����, ���� Fire����Ʈ, ������ ��ƼŬ �����ϴ°͵� ����, ���� 2������� �����ϴ°ͱ��� 
        phase2Set.BossPhase2Set();
    }

    // ������2 ������
    [ClientRpc]
    private void Phase2EndClientRpc()
    {
        // ī�޶� ���󺹱�
        StartCoroutine(phase2Cam.ReturnCam());
    }
    #endregion

}