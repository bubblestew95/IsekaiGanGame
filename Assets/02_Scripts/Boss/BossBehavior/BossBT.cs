using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossBT : MonoBehaviour
{
    public delegate void BTDelegate();
    public BTDelegate behaviorEndCallback;

    [SerializeField] public BossState curState;
    [SerializeField] private Animator anim;
    [SerializeField] private NavMeshAgent nvAgent;
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;
    [SerializeField] private BossAttackManager bossAttackManager;
    [SerializeField] private Vector3 center;

    private bool isCoroutineRunning = false;
    private bool isStun = false;
    private BossState previousBehavior;
    private float patternDelay = 2f;
    private Coroutine curCoroutine;

    public float PatternDelay { get { return patternDelay; } set { patternDelay = value; } }


    private void Update()
    {
        if (curState == BossState.Stun && !isStun)
        {
            StopCoroutine(curCoroutine);
            StartCoroutine(curState.ToString());
            isStun = true;
        }

        if (!isCoroutineRunning)
        {
            curCoroutine = StartCoroutine(curState.ToString());
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
            nvAgent.SetDestination(bossStateManager.aggroPlayer.transform.position);
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
    }

    private IEnumerator Attack2()
    {
        isCoroutineRunning = true;
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
    }

    private IEnumerator Attack3()
    {
        isCoroutineRunning = true;
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
    }

    private IEnumerator Attack4()
    {
        isCoroutineRunning = true;
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

        // �̵��ӵ� ����
        nvAgent.speed = 6f;

        // �ִϸ��̼� 4-1�� ���ӽð����� ����
        while (true)
        {
            // ��׷� �÷��̾� ����ٴ�
            nvAgent.SetDestination(bossStateManager.aggroPlayer.transform.position);

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
    }

    private IEnumerator Attack5()
    {
        isCoroutineRunning = true;
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
        int randomNum = Random.Range(0, bossStateManager.Players.Length);

        // ��� �ٸ� �÷��̾� �Ĵٺ��ٰ�
        while (true)
        {
            nvAgent.SetDestination(bossStateManager.Players[randomNum].transform.position);
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

        randomNum = Random.Range(0, bossStateManager.Players.Length);

        // �Ǵٸ� �÷��̾� �Ĵٺ��ٰ�
        while (true)
        {
            nvAgent.SetDestination(bossStateManager.Players[randomNum].transform.position);
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

        randomNum = Random.Range(0, bossStateManager.Players.Length);
        // �Ǵٸ� �÷��̾� �Ĵٺ��ٰ� 
        while (true)
        {
            nvAgent.SetDestination(bossStateManager.Players[randomNum].transform.position);
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
    }

    private IEnumerator Attack6()
    {
        isCoroutineRunning = true;
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
    }

    private IEnumerator Attack7()
    {
        isCoroutineRunning = true;
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
                // boss�� skin�� ������� �ϱ�, collider�� ��� ��Ȱ��ȭ
                bossStateManager.BossSkin.SetActive(false);
                bossStateManager.Boss.GetComponent<BoxCollider>().enabled = false;

                // boss�� Tr�� ������ġ�� �ű��
                bossStateManager.Boss.transform.position = bossAttackManager.CircleSkillPos[0].transform.position;

                // �����ִٰ� (�� ���� delay���� ������µ� ���� �ɸ��� �ð��� �M)
                yield return new WaitForSeconds(bossAttackManager.Delay - 1.1f);

                // �ִϸ��̼� ��� �� skin, collider Ȱ��ȭ
                anim.SetBool("Attack7-1Flag", true);
                bossStateManager.BossSkin.SetActive(true);
                bossStateManager.Boss.GetComponent<BoxCollider>().enabled = true;
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
        bossStateManager.BossSkin.SetActive(true);
        bossStateManager.Boss.GetComponent<BoxCollider>().enabled = true;
        nvAgent.ResetPath();

        // ���� �ִϸ��̼� ���� ���
        anim.Play("Stun");
        SetAnimBool(curState, true);

        // ������
        yield return new WaitForSeconds(3f);
        SetAnimBool(curState, false);

        // ���¸� ���ϰɸ��� �� ���·�
        curState = previousBehavior;

        isCoroutineRunning = false;
        isStun = false;
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
    #endregion
}
