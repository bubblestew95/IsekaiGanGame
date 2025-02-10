using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BossAttackManager : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;

    [Header("��ä�� ���� ����")]
    [SerializeField] private GameObject fanSkillPos;
    [SerializeField] private GameObject fanAttackCollider;
    [SerializeField] private DecalProjector fanFullRangeDecal;
    [SerializeField] private DecalProjector fanChargingRangeDecal;

    [Header("�ݿ� ���� ����")]
    [SerializeField] private GameObject halfCircleSkillPos;
    [SerializeField] private GameObject halfCircleAttackCollider;
    [SerializeField] private DecalProjector halfCircleRangeDecal;

    [Header("�� ���� ����")]
    [SerializeField] private GameObject[] circleSkillPos;
    [SerializeField] private GameObject[] circleAttackColliders;
    [SerializeField] private DecalProjector[] circleFullRangeDecals;
    [SerializeField] private DecalProjector[] circleChargingRangeDecals;

    [Header("�� ������")]
    [SerializeField] private GameObject P_Stone;
    [SerializeField] private Transform rightHand;

    // ��ų ����
    private BossSkillData skill;
    private string skillName;
    private float range;
    private int damage;
    private float delay;
    private float duration;
    private float knockBackDis;

    // ��ä�� ���� ����
    private float angle;
    private float radius;
    private float thickness;
    private int segmentCount = 50;
    private bool fanFirstTime = false;
    private bool halfCircleFirstTime = false;

    // ���� �ݶ��̴� ����
    private WaitForSeconds attackColliderTime = new WaitForSeconds(0.3f);

    // ���� Ÿ��
    private Vector3 randomTargetPos;

    // �ִϸ��̼� �ӵ� ��ӽ� delay���� ���Ѿ���.
    private float animSpd = 1f;

    // �� ������ ����(stun�� ���� ��������� ��������)
    private GameObject curStone;

    // ���Ͻ� �������̿��� �ڷ�ƾ ����
    private Coroutine curCoroutine;

    // ������Ƽ
    public GameObject[] CircleSkillPos { get { return circleSkillPos; } }
    public float Delay { get { return delay; } }
    public float AnimSpd { set { animSpd = value; } }

    private void Start()
    {
        bossStateManager.bossRandomTargetCallback += SetRandomTarget;
        randomTargetPos = Vector3.zero;
    }

    private void PerformAttack(string _state)
    {
        Debug.Log(_state + "ȣ���");
        switch (_state)
        {
            case "Attack1":
                curCoroutine = StartCoroutine(Attack1());
                break;
            case "Attack2":
                curCoroutine = StartCoroutine(Attack2());
                break;
            case "Attack3":
                curCoroutine = StartCoroutine(Attack3());
                break;
            case "Attack4":
                curCoroutine = StartCoroutine(Attack4());
                break;
            case "Attack5":
                curCoroutine = StartCoroutine(Attack5());
                break;
            case "Attack6":
                curCoroutine = StartCoroutine(Attack6());
                break;
            case "Attack7":
                curCoroutine = StartCoroutine(Attack7());
                break;
            case "Attack8":
                curCoroutine = StartCoroutine(Attack8());
                break;
            case "Attack9":
                curCoroutine = StartCoroutine(Attack9());
                break;
            case "Stun":
                StartCoroutine(Stun());
                break;
            case "SpecialAttack":
                curCoroutine = StartCoroutine(SpecialAttack());
                break;
            case "Die":
                StartCoroutine(Die());
                break;
            default:
                break;
        }
    }

    private void Attack8Particle()
    {
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack8, bossStateManager.Boss.transform.position);
    }

    private void Attack4Particle()
    {
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack4, bossStateManager.Boss.transform.position);
    }

    #region [Attack]
    // �ֵθ���
    private IEnumerator Attack1()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack1").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        range = range * 2;

        // ��ų��ġ ����
        Vector3 bossPosition = bossStateManager.Boss.transform.position;
        Vector3 forwardOffset = bossStateManager.Boss.transform.forward * 2f;
        fanSkillPos.transform.position = new Vector3(bossPosition.x + forwardOffset.x, 0.3f, bossPosition.z + forwardOffset.z);

        // ��ų ������ ���� ������ �°� ����
        Vector3 targetDirection = bossStateManager.Boss.transform.forward;
        fanSkillPos.transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // ��ų ������ ����
        fanAttackCollider.GetComponent<BossAttackCollider>().Damage = damage;
        fanAttackCollider.GetComponent<BossAttackCollider>().SkillName = skillName;
        fanAttackCollider.GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        angle = 90f;
        radius = range / 2;
        thickness = 1f;
        segmentCount = 50;

        if (!fanFirstTime)
        {
            Mesh fanMesh = BuildMesh();
            fanAttackCollider.GetComponent<MeshFilter>().mesh = fanMesh;
            fanAttackCollider.GetComponent<MeshCollider>().sharedMesh = fanMesh;
        }

        // ��ų ǥ��
        fanFullRangeDecal.size = new Vector3(range, range, 1f);

        // ��ų ���°� ǥ��
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;
            fanChargingRangeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // ������ ��ų ���� �Ⱥ��̰�
        fanFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        fanChargingRangeDecal.size = new Vector3(0f, 0f, 0f);

        // attackCollider Ȱ��ȭ
        fanAttackCollider.SetActive(true);

        // ��ƼŬ ���
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack1, new Vector3(fanAttackCollider.transform.position.x + forwardOffset.x * 3f, 0.5f, fanAttackCollider.transform.position.z + forwardOffset.z * 3f));

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        fanAttackCollider.SetActive(false);
    }

    // ��Ŀ����
    private IEnumerator Attack2()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack2").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        int cnt = 0;

        // ��ų��ġ ����
        foreach (GameObject skillPos in circleSkillPos)
        {
            skillPos.transform.position = new Vector3(bossStateManager.Players[cnt].transform.position.x, 0.3f, bossStateManager.Players[cnt].transform.position.z);

            cnt++;

            if (cnt == bossStateManager.Players.Length) break;
        }

        // ��ų ������ ����
        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        foreach (GameObject attackCollider in circleAttackColliders)
        {
            attackCollider.transform.localScale = new Vector3(range, 0.5f, range);
            attackCollider.GetComponent<BossAttackCollider>().Damage = damage;
            attackCollider.GetComponent<BossAttackCollider>().SkillName = skillName;
            attackCollider.GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;
        }

        // ��ų ǥ��
        foreach (DecalProjector circleFullRangeDecal in circleFullRangeDecals)
        {
            circleFullRangeDecal.size = new Vector3(range, range, 1f);
        }

        // ��ų ���°� ǥ��
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            // ��ų ǥ��
            foreach (DecalProjector circleChargingRangeDecal in circleChargingRangeDecals)
            {
                circleChargingRangeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);
            }

            yield return null;
        }
        yield return null;

        // ������ ��ų ���� �Ⱥ��̰�
        foreach (DecalProjector circleFullRangeDecal in circleFullRangeDecals)
        {
            circleFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        }

        foreach (DecalProjector circleChargingRangeDecal in circleChargingRangeDecals)
        {
            circleChargingRangeDecal.size = new Vector3(0f, 0f, 0f);
        }

        // attackCollider Ȱ��ȭ
        foreach (GameObject circleAttackCollider in circleAttackColliders)
        {
            circleAttackCollider.SetActive(true);

            // ��ƼŬ ���
            ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack2, circleAttackCollider.transform.position);
        }

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        foreach (GameObject circleAttackCollider in circleAttackColliders)
        {
            circleAttackCollider.SetActive(false);
        }
    }

    // ���� �߱���
    private IEnumerator Attack3()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack3").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // ��ų��ġ ����
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0.3f, bossStateManager.Boss.transform.position.z);

        // ��ų ������ ����
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // ��ų ǥ��
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // ��ų ���°� ǥ��
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // ��Ÿ� ǥ�� ������
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // attackCollider Ȱ��ȭ
        circleAttackColliders[0].SetActive(true);

        // ��ƼŬ ���
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack3, circleAttackColliders[0].transform.position, Quaternion.Euler(-90f, 0f, 0f));

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        circleAttackColliders[0].SetActive(false);
    }

    // ������
    private IEnumerator Attack4()
    {
        BSD_Duration skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack4").SkillData as BSD_Duration;

        skillName = skill.SkillName;
        damage = skill.Damage;
        duration = skill.Duration;
        knockBackDis = skill.KnockbackDistance;

        // ��ų ������ ����
        GetComponent<BossAttackCollider>().Damage = damage;
        GetComponent<BossAttackCollider>().SkillName = skillName;
        GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        GetComponent<BoxCollider>().enabled = true;
        Vector3 originSize = GetComponent<BoxCollider>().size;
        GetComponent<BoxCollider>().size = new Vector3(2f, 2f, 2f);
        bossStateManager.Boss.tag = "BossAttack";

        // ���� �������� Check
        while (true)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Chase"))
            {
                break;
            }
            yield return null;
        }

        // ParticleManager.Instance.PlayParticleSetParent(ParticleManager.Instance.attack4, bossStateManager.Boss.transform.position, bossStateManager.Boss, duration + 1f);

        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Chase"))
            {
                break;
            }
            yield return null;
        }

        // ���� ������
        GetComponent<BoxCollider>().size = originSize;
        GetComponent<BoxCollider>().enabled = false;
        bossStateManager.Boss.tag = "Untagged";

        yield return null;
    }

    // Ư�� ���� ������ (�⺻ �Ÿ� 6)
    private IEnumerator Attack5()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack5").SkillData;

        skillName = skill.SkillName;
        damage = skill.Damage;
        knockBackDis = skill.KnockbackDistance;

        // ��ų ������ ����
        GetComponent<BossAttackCollider>().Damage = damage;
        GetComponent<BossAttackCollider>().SkillName = skillName;
        GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        GetComponent<BoxCollider>().enabled = true;
        Vector3 originSize = GetComponent<BoxCollider>().size;
        GetComponent<BoxCollider>().size = new Vector3(2f, 2f, 2f);
        bossStateManager.Boss.tag = "BossAttack";

        // ���� �������� Check
        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Chase2") || anim.GetCurrentAnimatorStateInfo(0).IsName("Attack5Jump"))
            {
                break;
            }

            yield return null;
        }

        // ���� ������
        GetComponent<BoxCollider>().size = originSize;
        GetComponent<BoxCollider>().enabled = false;
        bossStateManager.Boss.tag = "Untagged";

        yield return null;
    }

    // �����
    private IEnumerator SpecialAttack()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "SpecialAttack").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // ��ų��ġ ����
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0.3f, bossStateManager.Boss.transform.position.z);

        // ��ų ������ ����
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // ��ų ǥ��
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // ��ų ���°� ǥ��
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // ��Ÿ� ǥ�� ������
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // �÷��̾� ���� ���̽��� ���� �÷��̾� �±� ����
        CheckPlayerBehindRock();

        // attackCollider Ȱ��ȭ
        circleAttackColliders[0].SetActive(true);

        // ��ƼŬ ���
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.SpecialAttack, circleAttackColliders[0].transform.position);

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        circleAttackColliders[0].SetActive(false);

        // �±� ����
        ResetTag();

        yield return null;
    }

    // ��������
    private IEnumerator Attack6()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack6").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // ��ų��ġ ����
        circleSkillPos[0].transform.position = new Vector3(randomTargetPos.x, 0.3f, randomTargetPos.z);

        // ��ų ������ ����
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // ��ų ǥ��
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // ������
        GameObject stone = Instantiate(P_Stone, rightHand);
        curStone = stone;

        stone.transform.SetParent(null);
        Vector3 startPos = stone.transform.position;
        Vector3 startSize = stone.transform.localScale;
        Quaternion startRotation = stone.transform.rotation;

        // ��ų ���°� ǥ��
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            stone.transform.position = Vector3.Lerp(startPos, circleSkillPos[0].transform.position, elapseTime / delay);
            stone.transform.localScale = Vector3.Lerp(startSize, new Vector3(2f, 2f, 2f), elapseTime / delay);
            stone.transform.rotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0f, 0f, 0f), elapseTime / delay);

            yield return null;
        }
        yield return null;

        // ��Ÿ� ǥ�� ������
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // attackCollider Ȱ��ȭ
        circleAttackColliders[0].SetActive(true);

        // ��ƼŬ ���
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack6, circleAttackColliders[0].transform.position);

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        circleAttackColliders[0].SetActive(false);

        curStone = null;
    }

    // ���� ����
    private IEnumerator Attack7()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack7").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        // ��ų��ġ ����
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.AggroPlayer.transform.position.x, 0.3f, bossStateManager.AggroPlayer.transform.position.z);

        // ��ų ������ ����
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().SkillName = skillName;
        circleAttackColliders[0].GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // ��ų ǥ��
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // ��ų ���°� ǥ��
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // ��Ÿ� ǥ�� ������
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // attackCollider Ȱ��ȭ
        circleAttackColliders[0].SetActive(true);

        // ��ƼŬ ���
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack7, circleAttackColliders[0].transform.position);

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        circleAttackColliders[0].SetActive(false);


    }

    // ����
    private IEnumerator Attack8()
    {
        BSD_Duration skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack8").SkillData as BSD_Duration;

        skillName = skill.SkillName;
        damage = skill.Damage;
        duration = skill.Duration;
        knockBackDis = skill.KnockbackDistance;

        // ��ų ������ ����
        GetComponent<BossAttackCollider>().Damage = damage;
        GetComponent<BossAttackCollider>().SkillName = skillName;
        GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        GetComponent<BoxCollider>().enabled = true;
        Vector3 originSize = GetComponent<BoxCollider>().size;
        GetComponent<BoxCollider>().size = new Vector3(2f, 2f, 2f);
        bossStateManager.Boss.tag = "BossAttack";

        // ���� �������� Check
        while (true)
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Chase"))
            {
                break;
            }
            yield return null;
        }

        while (true)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Chase"))
            {
                break;
            }
            yield return null;
        }

        // ���� ������
        GetComponent<BoxCollider>().size = originSize;
        GetComponent<BoxCollider>().enabled = false;
        bossStateManager.Boss.tag = "Untagged";

        yield return null;
    }

    // �����
    private IEnumerator Attack9()
    {
        Debug.Log("Attack9 ȣ���");

        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack9").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / animSpd;
        knockBackDis = skill.KnockbackDistance;

        range = range * 2;

        // ��ų��ġ ����
        Vector3 bossPosition = bossStateManager.Boss.transform.position;
        Vector3 forwardOffset = bossStateManager.Boss.transform.forward * 2f;
        halfCircleSkillPos.transform.position = new Vector3(bossPosition.x, 0.3f, bossPosition.z);

        // ��ų ������ ���� ������ �°� ����
        Vector3 targetDirection = bossStateManager.Boss.transform.forward;
        halfCircleSkillPos.transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // ��ų ������ ����
        halfCircleAttackCollider.GetComponent<BossAttackCollider>().Damage = damage;
        halfCircleAttackCollider.GetComponent<BossAttackCollider>().SkillName = skillName;
        halfCircleAttackCollider.GetComponent<BossAttackCollider>().KnockBackDistance = knockBackDis;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        angle = 180f;
        radius = range / 2;
        thickness = 1f;
        segmentCount = 50;

        if (!halfCircleFirstTime)
        {
            Mesh fanMesh = BuildMesh();
            halfCircleAttackCollider.GetComponent<MeshFilter>().mesh = fanMesh;
            halfCircleAttackCollider.GetComponent<MeshCollider>().sharedMesh = fanMesh;
        }

        // ��ų ǥ��
        halfCircleRangeDecal.size = new Vector3(range, range, 1f);

        yield return new WaitForSeconds(0.2f);

        // ��ų ǥ�û����
        halfCircleRangeDecal.size = new Vector3(0f, 0f, 0f);

        // ��ų ���°� ǥ��
        float elapseTime = 0f;

        while (elapseTime < delay - 0.2f)
        {
            elapseTime += Time.deltaTime;
            yield return null;
        }
        yield return null;

        // attackCollider Ȱ��ȭ
        halfCircleAttackCollider.SetActive(true);

        // ��ƼŬ ���
        ParticleManager.Instance.PlayParticle(ParticleManager.Instance.attack9, new Vector3(halfCircleAttackCollider.transform.position.x + forwardOffset.x * 0.5f, 0.5f, halfCircleAttackCollider.transform.position.z + forwardOffset.z * 0.5f));

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        halfCircleAttackCollider.SetActive(false);
    }

    // ���� �ɷ�����
    private IEnumerator Stun()
    {
        // ���� �������� �ڷ�ƾ ����
        StopCoroutine(curCoroutine);

        // ���� �ʱ�ȭ
        InitAttack();

        // ���� �� ����
        Destroy(curStone);

        yield return null;
    }

    // �׾�����
    private IEnumerator Die()
    {
        // ���� �������� �ڷ�ƾ ����
        StopCoroutine(curCoroutine);

        // ���� �ʱ�ȭ
        InitAttack();

        yield return null;
    }
    #endregion

    #region [Function]
    // ��ä�� ��� ����.
    private Mesh BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ThickFanMesh";

        int vertCount = (segmentCount + 2) * 2; // ��� + �ϴ� ��
        Vector3[] vertices = new Vector3[vertCount];
        int vertIndex = 0;

        // ���(Top) �� ���
        float angleStep = angle / segmentCount * Mathf.Deg2Rad;
        vertices[vertIndex++] = Vector3.zero; // ��� �߽���
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, thickness / 2); // ��� �ܰ�
        }

        // �ϴ�(Bottom) �� ���
        vertices[vertIndex++] = new Vector3(0, 0, -thickness / 2); // �ϴ� �߽���
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, -thickness / 2); // �ϴ� �ܰ�
        }

        // �ﰢ�� �ε��� �迭 ũ�� ��� (��� + �ϴ� + ����)
        int[] triangles = new int[segmentCount * 6 + segmentCount * 6 + segmentCount * 6];
        int triIndex = 0;

        // ��� �ﰢ��
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = 0; // �߽���
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }

        // �ϴ� �ﰢ��
        int bottomStart = segmentCount + 2;
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = bottomStart; // �߽���
            triangles[triIndex++] = bottomStart + i + 1;
            triangles[triIndex++] = bottomStart + i;
        }

        // ���� �ﰢ��
        for (int i = 1; i <= segmentCount; i++)
        {
            // �ܰ� ���-�ϴ� ���� (�ո�)
            int topOuter = i;
            int bottomOuter = bottomStart + i;
            triangles[triIndex++] = topOuter;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = topOuter + 1;

            triangles[triIndex++] = topOuter + 1;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = bottomOuter + 1;
        }

        // UV �� ��� ����
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / radius + 0.5f, vertices[i].y / radius + 0.5f);
        }

        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = vertices[i].z > 0 ? Vector3.forward : Vector3.back;
        }

        // Mesh ������ ����
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }

    // ���� Ÿ�� ����
    private void SetRandomTarget(Vector3 _targetPos)
    {
        randomTargetPos = _targetPos;
    }

    // ���� �ʱ�ȭ
    private void InitAttack()
    {
        // ��Į size ����
        fanFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        fanChargingRangeDecal.size = new Vector3(0f, 0f, 0f);
        foreach (DecalProjector circleFullRangeDecal in circleFullRangeDecals)
        {
            circleFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        }
        foreach (DecalProjector circleChargingRangeDecal in circleChargingRangeDecals)
        {
            circleChargingRangeDecal.size = new Vector3(0f, 0f, 0f);
        }

        // ���� �ݶ��̴� ����
        fanAttackCollider.SetActive(false);
        foreach (GameObject circleAttackCollider in circleAttackColliders)
        {
            circleAttackCollider.SetActive(false);
        }

        // �±� ����ȭ
        bossStateManager.Boss.tag = "Untagged";
    }

    // ����� ���ڿ� �ִ��� check
    // �� �ڿ� �ִٸ� BehindRock���� tag�� �ٲ�.
    private void CheckPlayerBehindRock()
    {
        Vector3 bossPos = new Vector3(bossStateManager.Boss.transform.position.x, 0.5f, bossStateManager.Boss.transform.position.z);
        LayerMask defaultLayerMask = LayerMask.GetMask("Player");

        // �� �÷��̾����� ray������ ���ڿ� �ִ��� Ȯ��
        foreach (GameObject player in bossStateManager.Players)
        {
            Vector3 playerPos = new Vector3(player.transform.position.x, 0.5f, player.transform.position.z);
            Vector3 dir = (playerPos - bossPos).normalized;

            RaycastHit[] hits = Physics.RaycastAll(bossPos, dir, 100f, defaultLayerMask);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit ray in hits)
            {
                if (ray.collider.CompareTag("Rock"))
                {
                    player.tag = "BehindRock";
                    break;
                }
                else if (ray.collider.gameObject == player)
                {
                    break;
                }
            }
        }
    }

    // �±� ����
    private void ResetTag()
    {
        foreach (GameObject player in bossStateManager.Players)
        {
            player.tag = "Player";
        }
    }
    #endregion
}