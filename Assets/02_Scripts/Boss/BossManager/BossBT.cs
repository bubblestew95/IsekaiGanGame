using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossBT : MonoBehaviour
{
    public delegate void BehaviorEndDelegate();
    public BehaviorEndDelegate behaviorEndCallback;

    [SerializeField] public BossState curState;
    
    [SerializeField] private Animator anim;
    [SerializeField] private List<BossSkillCooldown> skills;
    [SerializeField] private NavMeshAgent nvAgent;
    [SerializeField] private BossStateManager bossState;

    private bool isCoroutineRunning = false;

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

        // ���� �ٴϱ� ����
        anim.SetBool("ChaseFlag", true);

        float elapseTime = 0f;

        // ���°� �ٲ�� or 1�ʸ��� �ݹ� 
        while (true)
        {
            nvAgent.SetDestination(bossState.aggroPlayer.transform.position);
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

        // �ִϸ��̼� ����
        anim.SetBool("Attack1Flag", true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        SetBoss();

        // �ִϸ��̼� ��
        while (true)
        {
            Debug.Log("Attack1 ������");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack1Flag", false);
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

        // �ִϸ��̼� ����
        anim.SetBool("Attack2Flag", true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        SetBoss();

        // �ִϸ��̼� ��
        while (true)
        {
            Debug.Log("Attack2 ������");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack2Flag", false);
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

        // �ִϸ��̼� ����
        anim.SetBool("Attack3Flag", true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        SetBoss();

        // �ִϸ��̼� ��
        while (true)
        {
            Debug.Log("Attack3 ������");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack3Flag", false);
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

        // �ִϸ��̼� ����
        anim.SetBool("Attack4Flag", true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        SetBoss();

        // �ִϸ��̼� ��
        while (true)
        {
            Debug.Log("Attack4 ������");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack4Flag", false);
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

        // �ִϸ��̼� ����
        anim.SetBool("Attack5Flag", true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        SetBoss();

        // �ִϸ��̼� ��
        while (true)
        {
            Debug.Log("Attack5 ������");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5Flag", false);
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

        // �ִϸ��̼� ����
        anim.SetBool("Attack6Flag", true);

        // ��Ÿ�� ����, ���� ���ݷ� ����
        SetBoss();

        // �ִϸ��̼� ��
        while (true)
        {
            Debug.Log("Attack6 ������");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack6") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack6Flag", false);
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
    #endregion

    // ���� ����
    private void SetBoss()
    {
        // ��Ÿ�� ����, ���� ���ݷ� ����
        foreach (BossSkillCooldown skill in skills)
        {
            if (skill.bossSkillData.SkillName == curState.ToString())
            {
                skill.StartCooldown();
                bossState.damage = skill.bossSkillData.Damage;
            }
        }
    }

}
