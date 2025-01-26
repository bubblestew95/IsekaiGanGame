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

        // 따라 다니기 시작
        anim.SetBool("ChaseFlag", true);

        float elapseTime = 0f;

        // 상태가 바뀌고 or 1초마다 콜백 
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

        // 따라 다니기 멈춤
        anim.SetBool("ChaseFlag", false);
        nvAgent.ResetPath();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack1()
    {
        isCoroutineRunning = true;

        // 애니메이션 시작
        anim.SetBool("Attack1Flag", true);

        // 쿨타임 실행, 보스 공격력 설정
        SetBoss();

        // 애니메이션 끝
        while (true)
        {
            Debug.Log("Attack1 실행중");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack1") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack1Flag", false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack2()
    {
        isCoroutineRunning = true;

        // 애니메이션 시작
        anim.SetBool("Attack2Flag", true);

        // 쿨타임 실행, 보스 공격력 설정
        SetBoss();

        // 애니메이션 끝
        while (true)
        {
            Debug.Log("Attack2 실행중");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack2") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack2Flag", false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack3()
    {
        isCoroutineRunning = true;

        // 애니메이션 시작
        anim.SetBool("Attack3Flag", true);

        // 쿨타임 실행, 보스 공격력 설정
        SetBoss();

        // 애니메이션 끝
        while (true)
        {
            Debug.Log("Attack3 실행중");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack3") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack3Flag", false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack4()
    {
        isCoroutineRunning = true;

        // 애니메이션 시작
        anim.SetBool("Attack4Flag", true);

        // 쿨타임 실행, 보스 공격력 설정
        SetBoss();

        // 애니메이션 끝
        while (true)
        {
            Debug.Log("Attack4 실행중");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack4Flag", false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack5()
    {
        isCoroutineRunning = true;

        // 애니메이션 시작
        anim.SetBool("Attack5Flag", true);

        // 쿨타임 실행, 보스 공격력 설정
        SetBoss();

        // 애니메이션 끝
        while (true)
        {
            Debug.Log("Attack5 실행중");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5Flag", false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack6()
    {
        isCoroutineRunning = true;

        // 애니메이션 시작
        anim.SetBool("Attack6Flag", true);

        // 쿨타임 실행, 보스 공격력 설정
        SetBoss();

        // 애니메이션 끝
        while (true)
        {
            Debug.Log("Attack6 실행중");

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack6") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack6Flag", false);
                break;
            }

            yield return null;
        }

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }
    #endregion

    // 보스 설정
    private void SetBoss()
    {
        // 쿨타임 실행, 보스 공격력 설정
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
