using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// 0127 executeState ���� ���� �������� ���� ǥ��

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

    private bool isCoroutineRunning = false;
    private BossState previousBehavior;

    private void Update()
    {
        if (!isCoroutineRunning)
        {
            StartCoroutine(curState.ToString());
        }
    }

    #region BossBehavior
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

            if (elapseTime >= 2f)
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
        foreach (BossSkill skill in bossSkillManager.Skills)
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
        foreach (BossSkill skill in bossSkillManager.Skills)
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
        foreach (BossSkill skill in bossSkillManager.Skills)
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
        foreach (BossSkill skill in bossSkillManager.Skills)
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

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in bossSkillManager.Skills)
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

    private IEnumerator Attack6()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkill skill in bossSkillManager.Skills)
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

    private IEnumerator Attack7()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // �ִϸ��̼� ����
        SetAnimBool(curState, true);

        // ��Ÿ�� ����
        foreach (BossSkill skill in bossSkillManager.Skills)
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
    #endregion

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
}
