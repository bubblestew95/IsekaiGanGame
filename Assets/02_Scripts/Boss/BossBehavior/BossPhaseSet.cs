using UnityEngine;
using UnityEngine.AI;

public class BossPhaseSet : MonoBehaviour
{
    [SerializeField] private BossAttackManager bossAttackManager;
    [SerializeField] private BossSkillManager bossSkillManager;
    [SerializeField] private BossBT bossBT;
    [SerializeField] private NavMeshAgent bossNvAgent;
    [SerializeField] private Animator bossAnim;

    [Header("보스 2패 수정시킬 값들")]
    public float bossSpd = 1.5f;
    public float bossCooldown = 0.8f;
    public float bossPatternDelay = 0.5f;
    public float bossAnimSpd = 1.5f;
    public Material bossMat;
    public GameObject bossBody;
    public GameObject bossHead;
    public GameObject bossJaw;
    public Material mapMaterial;
    public GameObject phase1Particle;
    public GameObject phase2Particle;

    private void Start()
    {
        BossPhase2Set();
    }

    private void BossPhase2Set()
    {
        // 이동속도
        // 보스의 navMesh 변경
        bossNvAgent.speed *= bossSpd;

        // 쿨타임 감소
        foreach (BossSkill randomSkill in bossSkillManager.RandomSkills)
        {
            randomSkill.CooldownModifier = bossCooldown;
        }

        // 패턴 발동 간격 감소
        bossBT.PatternDelay *= bossPatternDelay;

        // 패턴 행동속도 배속
        bossAnim.SetFloat("AnimSpd", bossAnimSpd);

        // 패턴 행동속도 배속한만큼 보스공격 보이는것도 수정
        bossAttackManager.AnimSpd = bossAnimSpd;

        // 보스 패턴에 불 이펙트 추가

        // 보스 Mat 변경
        bossBody.GetComponent<Renderer>().material = bossMat;
        bossHead.GetComponent<Renderer>().material = bossMat;
        bossJaw.GetComponent<Renderer>().material = bossMat;

        // 맵 색깔 변경
        mapMaterial.color = new Color(255f / 255f, 56f / 255f, 63f / 255f);

        // 맵 파티클 변경
        phase1Particle.SetActive(false);
        phase2Particle.SetActive(true);
    }
}
