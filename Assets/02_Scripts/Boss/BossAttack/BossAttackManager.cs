using System.Collections;
using System.Runtime.CompilerServices;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// �ִϸ��̼� ���۽� ���� ��ġ, �������� �����ϴ� �͵��� �ְ�,
// �÷��̾��� ��ġ���� �����ϴ°͵�� �ְ�
// �� attack collider���� �������� ���� ������ �־ �ش� ������ ��ŭ �÷��̾����� ��������.
// �� attack collider�� size�� skill ƫ���� �ִ� ������ ���� �������.
public class BossAttackManager : MonoBehaviour
{
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;

    [Header("��ä�� ���� ����")]
    [SerializeField] private GameObject fanSkillPos;
    [SerializeField] private GameObject fanAttackCollider;
    [SerializeField] private DecalProjector fanFullRangeDecal;
    [SerializeField] private DecalProjector fanChargingRangeDecal;

    [Header("�� ���� ����")]
    [SerializeField] private GameObject[] circleSkillPos;
    [SerializeField] private GameObject[] circleAttackColliders;
    [SerializeField] private DecalProjector[] circleFullRangeDecals;
    [SerializeField] private DecalProjector[] circleChargingRangeDecals;

    // ��ų ����
    private BossSkillData skill;
    private float range;
    private float damage;
    private float delay;

    // ��ä�� ���� ����
    private float angle;
    private float radius;
    private float thickness;
    private int segmentCount = 50;
    private bool firstTime = false;

    // ���� �ݶ��̴� ����
    private WaitForSeconds attackColliderTime = new WaitForSeconds(0.3f);

    // ���� Ÿ��
    private GameObject randomTarget;

    private void Start()
    {
        bossStateManager.bossRandomTargetCallback += SetRandomTarget;
        randomTarget = bossStateManager.Players[0];
    }


    private void PerformAttack(string _state)
    {
        switch (_state)
        {
            case "Attack1":
                StartCoroutine(Attack1());
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

    // �ֵθ���
    private IEnumerator Attack1()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack1").SkillData;

        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;

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

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        angle = 90f;
        radius = range/2;
        thickness = 0.5f;
        segmentCount = 50;

        if (!firstTime)
        {
            Mesh fanMesh = BuildMesh();
            fanAttackCollider.GetComponent<MeshFilter>().mesh = fanMesh;
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

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        fanAttackCollider.SetActive(false);
    }

    // ��Ŀ����
    private IEnumerator Attack2()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack2").SkillData;

        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;

        int cnt = 0;

        // ��ų��ġ ����
        foreach (GameObject skillPos in circleSkillPos)
        {
            skillPos.transform.position = new Vector3(bossStateManager.Players[cnt].transform.position.x, 0.3f, bossStateManager.Players[cnt].transform.position.z);
            cnt++;
        }

        // ��ų ������ ����
        fanAttackCollider.GetComponent<BossAttackCollider>().Damage = damage;

        // ���� �ݶ��̴� ����(ũ��, ��ġ, ���� ��)
        foreach (GameObject attackCollider in circleAttackColliders)
        {
            attackCollider.transform.localScale = new Vector3(range, 0.5f, range);
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

        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;

        // ��ų��ġ ����
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0.3f, bossStateManager.Boss.transform.position.z);

        // ��ų ������ ����
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;

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

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        circleAttackColliders[0].SetActive(false);
    }

    private IEnumerator Attack4()
    {
        yield return null;
    }

    private IEnumerator Attack5()
    {
        yield return null;
    }

    // ��������
    private IEnumerator Attack6()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack6").SkillData;

        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;

        // ��ų��ġ ����
        circleSkillPos[0].transform.position = new Vector3(randomTarget.transform.position.x, 0.3f, randomTarget.transform.position.z);

        // ��ų ������ ����
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;

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

        yield return attackColliderTime;

        // attackCollider ��Ȱ��ȭ
        circleAttackColliders[0].SetActive(false);
    }

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
    private void SetRandomTarget(GameObject _target)
    {
        randomTarget = _target;
    }
}
