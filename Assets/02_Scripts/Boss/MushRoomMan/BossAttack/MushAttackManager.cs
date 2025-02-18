using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MushAttackManager : NetworkBehaviour
{
    [Header("Attack1 - �극��")]
    [SerializeField] private GameObject bressDecal;
    [SerializeField] private GameObject bress;

    [Header("Attack2 - ������")]
    [SerializeField] private GameObject P_PoisonCloud;

    [Header("Attack3 - ��ٶ� ������")]
    [SerializeField] private GameObject attack3DecalPos;
    [SerializeField] private DecalProjector attack3Decal;
    [SerializeField] private GameObject P_PoisionRay;

    [Header("Attack4 - ������ Jump����")]
    [SerializeField] private GameObject attckJumpDecalPos;
    [SerializeField] private DecalProjector attckJumpFullDecal;
    [SerializeField] private DecalProjector attckJumpChargeDecal;
    [SerializeField] private GameObject P_AttackJump;

    [Header("Attack6 - �˻Ѹ���")]
    [SerializeField] private GameObject P_Attack6;

    // ���� �Ұ͵�
    private Animator anim;
    private BossSkillManager mushSkillManager;
    private MushStateManager mushStateManager;

    // ��ų ����
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
            case "Attack3_1":
                StartCoroutine(Attack3_1());
                break;
            case "Attack3_2":
                StartCoroutine(Attack3_2());
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

    // �극��
    private IEnumerator Attack1()
    {
        // ��Į Ű�� ��ġ����
        bressDecal.transform.position = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 1f + mushStateManager.Boss.transform.right * 1f;
        bressDecal.transform.rotation = mushStateManager.Boss.transform.localRotation;
        bressDecal.SetActive(true);

        // �ִϸ��̼��� �����ٸ�
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
        // ��ų ���� ��������
        skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack1").SkillData;

        // ��ų�� �°� ����
        skillName = skill.SkillName;
        damage = skill.Damage;
        knockBackDis = skill.KnockbackDistance;

        // ��ġ ���� �� �ڽ�����
        bress.transform.position = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 2f;
        bress.transform.rotation = mushStateManager.Boss.transform.rotation;
        bress.transform.SetParent(mushStateManager.Boss.transform);
        bress.SetActive(true);

        // �ִϸ��̼��� �����ٸ�
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

    // ������
    private IEnumerator Attack2()
    {
        GameObject attack2Object;
        Vector3 pos = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 1f;

        attack2Object = Instantiate(P_PoisonCloud, new Vector3(pos.x, 0f, pos.z), Quaternion.identity, null);

        yield return null;
    }

    // ��ٶ� ������
    private IEnumerator Attack3_1()
    {
        // �׸� ��Į ����
        attack3DecalPos.transform.position = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 2f;
        attack3DecalPos.transform.rotation = mushStateManager.Boss.transform.localRotation;
        attack3Decal.size = new Vector3(0f, 2f, 100f);
        attack3DecalPos.transform.SetParent(mushStateManager.Boss.transform);
        attack3DecalPos.SetActive(true);
        StartCoroutine(Attack3DecalCoroutine(0.5f));

        yield return null;
    }
    private IEnumerator Attack3_2()
    {
        // ��Į �Ⱥ��̰� ����
        attack3DecalPos.SetActive(false);

        // ���ڷ� ���� ���� ���� �������� ����
        if (IsServer)
        {
            Vector3 pos = mushStateManager.Boss.transform.position + mushStateManager.Boss.transform.forward * 2f;
            Quaternion rot = mushStateManager.Boss.transform.localRotation;
            Instantiate(P_PoisionRay, pos, rot, null).GetComponent<NetworkObject>().Spawn(true);
        }

        yield return null;
    }

    // ��ģ ����
    private IEnumerator Attack4()
    {
        float spd = 1f;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4"))
        {
            spd = 1f;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-1"))
        {
            spd = 1.5f;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-2"))
        {
            spd = 2f;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack4-3"))
        {
            spd = 2.5f;
        }

        // ��ų����
        skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack4").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay / spd;
        knockBackDis = skill.KnockbackDistance;

        // ��ų��ġ ����
        attckJumpDecalPos.transform.position = new Vector3(mushStateManager.RandomPlayer.transform.position.x, 0.3f, mushStateManager.RandomPlayer.transform.position.z);

        // ��ų ���̰� ��ų������Į ����
        attckJumpFullDecal.size = new Vector3(range, range, 1f);

        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            attckJumpChargeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            if (elapseTime >= delay)
            {
                break;
            }

            yield return null;
        }

        // ��Į ���� �ʱ�ȭ
        attckJumpChargeDecal.size = Vector3.zero;
        attckJumpFullDecal.size = Vector3.zero;

        // ������ ����
        Instantiate(P_AttackJump, attckJumpDecalPos.transform.position, Quaternion.identity, null);

        yield return null;
    }

    // �׳� ����
    private IEnumerator Attack5()
    {
        // ��ų����
        skill = mushSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack5").SkillData;

        skillName = skill.SkillName;
        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;
        knockBackDis = skill.KnockbackDistance;

        // ��ų��ġ ����
        attckJumpDecalPos.transform.position = new Vector3(mushStateManager.AggroPlayer.transform.position.x, 0.3f, mushStateManager.AggroPlayer.transform.position.z);

        // ��ų ���̰� ��ų������Į ����
        attckJumpFullDecal.size = new Vector3(range, range, 1f);

        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            attckJumpChargeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            if (elapseTime >= delay)
            {
                break;
            }

            yield return null;
        }

        // ��Į ���� �ʱ�ȭ
        attckJumpChargeDecal.size = Vector3.zero;
        attckJumpFullDecal.size = Vector3.zero;

        // ������ ����
        Instantiate(P_AttackJump, attckJumpDecalPos.transform.position, Quaternion.identity, null);

        yield return null;
    }

    // �˻Ѹ���
    private IEnumerator Attack6()
    {
        for (int i = 0; i < 4; i++)
        {
            if (mushStateManager.AlivePlayers[i] == null) continue;

            GameObject attack6 = Instantiate(P_Attack6, mushStateManager.AlivePlayers[i].transform);
        }

        yield return null;
    }
    #endregion

    #region [Function]

    // �ִϸ��̼� �������� Ȯ��
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

    // ���� ��Į ��� �ڷ�ƾ
    private IEnumerator Attack3DecalCoroutine(float _delayTime)
    {
        float elapseTime = 0f;
        Vector3 startSize = new Vector3(0f, 2f, 100f);
        Vector3 targetSize = new Vector3(3f, 2f, 100f);

        while (true)
        {
            elapseTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapseTime / _delayTime);
            attack3Decal.size = Vector3.Lerp(startSize, targetSize, t);

            if (elapseTime >= _delayTime)
            {
                break;
            }

            yield return null;
        }
    }

    #endregion
}
