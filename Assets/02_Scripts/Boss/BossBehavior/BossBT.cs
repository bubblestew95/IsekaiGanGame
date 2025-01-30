using System.Collections;
using System.Collections.Generic;
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
