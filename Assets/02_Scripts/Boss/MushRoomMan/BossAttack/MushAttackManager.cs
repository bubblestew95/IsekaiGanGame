using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;

public class MushAttackManager : MonoBehaviour
{
    [Header("브레스 관련")]
    [SerializeField] private GameObject bressDecal;
    [SerializeField] private GameObject bress;

    // 참조 할것들
    private Animator anim;
    private BossSkillManager mushSkillManager;
    private MushStateManager mushStateManager;

    // 스킬 관련
    private BossSkillData skill;
    private string skillName;
    private float range;
    private int damage;
    private float delay;
    private float duration;
    private float knockBackDis;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        mushSkillManager = GetComponent<BossSkillManager>();
        mushStateManager = GetComponent<MushStateManager>();
    }

    private void PerformAttack(string _state)
    {
        switch (_state)
        {
            case "Attack1":
                StartCoroutine(Attack1());
                break;
            case "Attack1-1":
                StartCoroutine(Attack1_1());
                break;
            case "Attack2":
                StartCoroutine(Attack2());
                break;
            case "Attack3":
                StartCoroutine(Attack3());
                break;
            case "Attack4":
                StartCoroutine(Attack4());
                break;
            case "Attack5":
                StartCoroutine(Attack5());
                break;
            case "Attack6":
                StartCoroutine(Attack6());
                break;
            default:
                break;
        }
    }

    #region [Attack]

    private IEnumerator Attack1()
    {
        // 데칼 키고 위치변경
        bressDecal.transform.position = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 1f + mushStateManager.Boss.transform.right * 1f;
        bressDecal.transform.rotation = mushStateManager.Boss.transform.localRotation;
        bressDecal.SetActive(true);

        // 애니메이션이 끝났다면
        while (true)
        {
            if (CheckEndAnim("Attack1"))
            {
                break;
            }

            yield return null;
        }

        bressDecal.SetActive(false);
    }

    private IEnumerator Attack1_1()
    {
        // 스킬 정보 가져오고
        skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack1").SkillData;

        // 스킬에 맞게 세팅
        skillName = skill.SkillName;
        damage = skill.Damage;
        knockBackDis = skill.KnockbackDistance;

        // 위치 설정 및 자식으로
        bress.transform.position = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 2f;
        bress.transform.rotation = mushStateManager.Boss.transform.rotation;
        bress.transform.SetParent(mushStateManager.Boss.transform);
        bress.SetActive(true);

        // 애니메이션이 끝났다면
        while (true)
        {
            if (CheckEndAnim("Attack1-1"))
            {
                break;
            }

            yield return null;
        }

        bress.SetActive(false);

    }

    private IEnumerator Attack2()
    {
        yield return null;
    }

    private IEnumerator Attack3()
    {
        yield return null;
    }

    private IEnumerator Attack4()
    {
        yield return null;
    }

    private IEnumerator Attack5()
    {
        yield return null;
    }

    private IEnumerator Attack6()
    {
        yield return null;
    }
    #endregion

    #region [Function]

    // 애니메이션 끝났는지 확인
    private bool CheckEndAnim(string _animName)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(_animName) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
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
