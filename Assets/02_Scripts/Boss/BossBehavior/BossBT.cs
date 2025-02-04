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

        // 따라 다니기 시작
        anim.SetBool("ChaseFlag", true);

        float elapseTime = 0f;

        // 상태가 바뀌고 or 1초마다 콜백 
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

        // 따라 다니기 멈춤
        anim.SetBool("ChaseFlag", false);
        nvAgent.ResetPath();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack1()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행, 보스 공격력 설정
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // 애니메이션 끝
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
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
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행, 보스 공격력 설정
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // 애니메이션 끝
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
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
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행, 보스 공격력 설정
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // 애니메이션 끝
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
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
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행, 보스 공격력 설정
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Attack4 준비모션에서는 안따라다니게 설정
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

        // attack4의 지속시간을 가져옴.
        BSD_Duration attack4 = bossSkillManager.Skills
            .Where(skill => skill.SkillData.SkillName == "Attack4")
            .Select(skill => skill.SkillData as BSD_Duration)
            .FirstOrDefault(bsd => bsd != null);

        float duration = attack4.Duration;
        float elapseTime = 0f;

        // 이동속도 설정
        nvAgent.speed = 6f;

        // 애니메이션 4-1을 지속시간동안 실행
        while (true)
        {
            // 어그로 플레이어 따라다님
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

        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;
    }

    private IEnumerator Attack5()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 공격 5 끝
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

        // 잠시 다른 플레이어 쳐다보다가
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

        // 공격 5-1
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

        // 또다른 플레이어 쳐다보다가
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


        // 공격 5-2
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
        // 또다른 플레이어 쳐다보다가 
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

        // 공격 5-3
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5-3") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-3Flag", false);
                break;
            }

            yield return null;
        }

        // 가운데로 이동해서 특수패턴(전멸기)
        // 현재 위치에서 가운데 위치로 Lerp하게 이동(15~50프레임)하면서, 점프 애니메이션 실행하면 될듯

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

        // Roar(전멸기 패턴)
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Roar") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                anim.SetBool("Attack5-4Flag", false);
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
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 애니메이션 끝
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                SetAnimBool(curState, false);
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

    private IEnumerator Attack7()
    {
        isCoroutineRunning = true;
        previousBehavior = curState;

        // 애니메이션 시작
        SetAnimBool(curState, true);

        // 쿨타임 실행
        foreach (BossSkill skill in bossSkillManager.RandomSkills)
        {
            if (skill.SkillData.SkillName == curState.ToString())
            {
                skill.UseSkill();
            }
        }

        // Chase -> Attack7 넘어갔는지 check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName(curState.ToString()))
            {
                break;
            }
            yield return null;
        }

        // Attack7이 끝났는지 Check
        while (true)
        {
            if (CheckEndAnim(curState))
            {
                // boss의 skin을 사라지게 하기, collider도 잠시 비활성화
                bossStateManager.BossSkin.SetActive(false);
                bossStateManager.Boss.GetComponent<BoxCollider>().enabled = false;

                // boss의 Tr을 공격위치로 옮기기
                bossStateManager.Boss.transform.position = bossAttackManager.CircleSkillPos[0].transform.position;

                // 몇초있다가 (총 공격 delay에서 내려찍는데 까지 걸리는 시간을 뻄)
                yield return new WaitForSeconds(bossAttackManager.Delay - 1.1f);

                // 애니메이션 재생 및 skin, collider 활성화
                anim.SetBool("Attack7-1Flag", true);
                bossStateManager.BossSkin.SetActive(true);
                bossStateManager.Boss.GetComponent<BoxCollider>().enabled = true;
                break;
            }
            yield return null;
        }

        // Attack 7-1이 끝났는지 check
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


        // 상태를 chase로 변경
        curState = BossState.Chase;

        // 패턴이 끝났음을 콜백
        behaviorEndCallback?.Invoke();

        isCoroutineRunning = false;

        yield return null;
    }

    private IEnumerator Stun()
    {
        isCoroutineRunning = true;

        // 기존 작업들을 초기화 하는 코드
        // 1. flag 초기화
        // 2. 이동속도 초기화
        // 3. 보스 y값이 이상할수도 있으니, y값 초기화
        // 4. 스킨과 collider 정상화
        ResetAnimBool();
        nvAgent.speed = 3f;
        bossStateManager.Boss.transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0f, bossStateManager.Boss.transform.position.z);
        bossStateManager.BossSkin.SetActive(true);
        bossStateManager.Boss.GetComponent<BoxCollider>().enabled = true;
        nvAgent.ResetPath();

        // 스턴 애니메이션 강제 재생
        anim.Play("Stun");
        SetAnimBool(curState, true);

        // 몇초후
        yield return new WaitForSeconds(3f);
        SetAnimBool(curState, false);

        // 상태를 스턴걸리기 전 상태로
        curState = previousBehavior;

        isCoroutineRunning = false;
        isStun = false;
    }
    #endregion

    #region [Function]
    // 애니메이션 bool값 설정
    private void SetAnimBool(BossState _state, bool _isActive)
    {
        anim.SetBool(_state.ToString() + "Flag", _isActive);
    }

    // 애니메이션 끝났는지 check하는 조건문
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

    // 애니메이션 모든 bool값 초기화
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
