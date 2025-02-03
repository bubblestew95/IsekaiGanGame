using UnityEngine;
using UnityEngine.AI;

public class BossPhaseSet : MonoBehaviour
{
    [SerializeField] private BossAttackManager bossAttackManager;
    [SerializeField] private BossSkillManager bossSkillManager;
    [SerializeField] private BossBT bossBT;
    [SerializeField] private NavMeshAgent bossNvAgent;
    [SerializeField] private Animator bossAnim;

    [Header("���� 2�� ������ų ����")]
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
        // �̵��ӵ�
        // ������ navMesh ����
        bossNvAgent.speed *= bossSpd;

        // ��Ÿ�� ����
        foreach (BossSkill randomSkill in bossSkillManager.RandomSkills)
        {
            randomSkill.CooldownModifier = bossCooldown;
        }

        // ���� �ߵ� ���� ����
        bossBT.PatternDelay *= bossPatternDelay;

        // ���� �ൿ�ӵ� ���
        bossAnim.SetFloat("AnimSpd", bossAnimSpd);

        // ���� �ൿ�ӵ� ����Ѹ�ŭ �������� ���̴°͵� ����
        bossAttackManager.AnimSpd = bossAnimSpd;

        // ���� ���Ͽ� �� ����Ʈ �߰�

        // ���� Mat ����
        bossBody.GetComponent<Renderer>().material = bossMat;
        bossHead.GetComponent<Renderer>().material = bossMat;
        bossJaw.GetComponent<Renderer>().material = bossMat;

        // �� ���� ����
        mapMaterial.color = new Color(255f / 255f, 56f / 255f, 63f / 255f);

        // �� ��ƼŬ ����
        phase1Particle.SetActive(false);
        phase2Particle.SetActive(true);
    }
}
