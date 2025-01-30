using System.Collections;
using System.Runtime.CompilerServices;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// 애니메이션 시작시 보스 위치, 각도에서 공격하는 것들이 있고,
// 플레이어의 위치에서 시작하는것들고 있고
// 각 attack collider에는 데미지에 관한 정보가 있어서 해당 데미지 만큼 플레이어한테 입혀야함.
// 각 attack collider의 size는 skill 튤팁에 있는 정보를 토대로 만들어짐.
public class BossAttackManager : MonoBehaviour
{
    [SerializeField] private BossStateManager bossStateManager;
    [SerializeField] private BossSkillManager bossSkillManager;

    [Header("부채꼴 공격 관련")]
    [SerializeField] private GameObject fanSkillPos;
    [SerializeField] private GameObject fanAttackCollider;
    [SerializeField] private DecalProjector fanFullRangeDecal;
    [SerializeField] private DecalProjector fanChargingRangeDecal;

    [Header("원 공격 관련")]
    [SerializeField] private GameObject[] circleSkillPos;
    [SerializeField] private GameObject[] circleAttackColliders;
    [SerializeField] private DecalProjector[] circleFullRangeDecals;
    [SerializeField] private DecalProjector[] circleChargingRangeDecals;

    // 스킬 관련
    private BossSkillData skill;
    private float range;
    private float damage;
    private float delay;

    // 부채꼴 생성 관련
    private float angle;
    private float radius;
    private float thickness;
    private int segmentCount = 50;
    private bool firstTime = false;

    // 공격 콜라이더 관련
    private WaitForSeconds attackColliderTime = new WaitForSeconds(0.3f);

    // 랜덤 타겟
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

    // 휘두르기
    private IEnumerator Attack1()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack1").SkillData;

        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;

        range = range * 2;

        // 스킬위치 조정
        Vector3 bossPosition = bossStateManager.Boss.transform.position;
        Vector3 forwardOffset = bossStateManager.Boss.transform.forward * 2f;
        fanSkillPos.transform.position = new Vector3(bossPosition.x + forwardOffset.x, 0.3f, bossPosition.z + forwardOffset.z);

        // 스킬 각도를 보스 각도에 맞게 설정
        Vector3 targetDirection = bossStateManager.Boss.transform.forward;
        fanSkillPos.transform.rotation = Quaternion.LookRotation(targetDirection, Vector3.up);

        // 스킬 데미지 설정
        fanAttackCollider.GetComponent<BossAttackCollider>().Damage = damage;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        angle = 90f;
        radius = range/2;
        thickness = 0.5f;
        segmentCount = 50;

        if (!firstTime)
        {
            Mesh fanMesh = BuildMesh();
            fanAttackCollider.GetComponent<MeshFilter>().mesh = fanMesh;
        }

        // 스킬 표시
        fanFullRangeDecal.size = new Vector3(range, range, 1f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;
            fanChargingRangeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // 끝나면 스킬 범위 안보이게
        fanFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        fanChargingRangeDecal.size = new Vector3(0f, 0f, 0f);

        // attackCollider 활성화
        fanAttackCollider.SetActive(true);

        yield return attackColliderTime;

        // attackCollider 비활성화
        fanAttackCollider.SetActive(false);
    }

    // 럴커패턴
    private IEnumerator Attack2()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack2").SkillData;

        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;

        int cnt = 0;

        // 스킬위치 조정
        foreach (GameObject skillPos in circleSkillPos)
        {
            skillPos.transform.position = new Vector3(bossStateManager.Players[cnt].transform.position.x, 0.3f, bossStateManager.Players[cnt].transform.position.z);
            cnt++;
        }

        // 스킬 데미지 설정
        fanAttackCollider.GetComponent<BossAttackCollider>().Damage = damage;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        foreach (GameObject attackCollider in circleAttackColliders)
        {
            attackCollider.transform.localScale = new Vector3(range, 0.5f, range);
        }

        // 스킬 표시
        foreach (DecalProjector circleFullRangeDecal in circleFullRangeDecals)
        {
            circleFullRangeDecal.size = new Vector3(range, range, 1f);
        }

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            // 스킬 표시
            foreach (DecalProjector circleChargingRangeDecal in circleChargingRangeDecals)
            {
                circleChargingRangeDecal.size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);
            }

            yield return null;
        }
        yield return null;

        // 끝나면 스킬 범위 안보이게
        foreach (DecalProjector circleFullRangeDecal in circleFullRangeDecals)
        {
            circleFullRangeDecal.size = new Vector3(0f, 0f, 0f);
        }

        foreach (DecalProjector circleChargingRangeDecal in circleChargingRangeDecals)
        {
            circleChargingRangeDecal.size = new Vector3(0f, 0f, 0f);
        }

        // attackCollider 활성화
        foreach (GameObject circleAttackCollider in circleAttackColliders)
        {
            circleAttackCollider.SetActive(true);
        }

        yield return attackColliderTime;

        // attackCollider 비활성화
        foreach (GameObject circleAttackCollider in circleAttackColliders)
        {
            circleAttackCollider.SetActive(false);
        }
    }

    // 땅에 발구름
    private IEnumerator Attack3()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack3").SkillData;

        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;

        // 스킬위치 조정
        circleSkillPos[0].transform.position = new Vector3(bossStateManager.Boss.transform.position.x, 0.3f, bossStateManager.Boss.transform.position.z);

        // 스킬 데미지 설정
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // 스킬 표시
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // 사거리 표시 없에기
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // attackCollider 활성화
        circleAttackColliders[0].SetActive(true);

        yield return attackColliderTime;

        // attackCollider 비활성화
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

    // 돌던지기
    private IEnumerator Attack6()
    {
        skill = bossSkillManager.Skills.Find(skill => skill.SkillData.SkillName == "Attack6").SkillData;

        range = skill.AttackRange;
        damage = skill.Damage;
        delay = skill.AttackColliderDelay;

        // 스킬위치 조정
        circleSkillPos[0].transform.position = new Vector3(randomTarget.transform.position.x, 0.3f, randomTarget.transform.position.z);

        // 스킬 데미지 설정
        circleAttackColliders[0].GetComponent<BossAttackCollider>().Damage = damage;

        // 공격 콜라이더 설정(크기, 위치, 각도 등)
        circleAttackColliders[0].transform.localScale = new Vector3(range, 0.5f, range);

        // 스킬 표시
        circleFullRangeDecals[0].size = new Vector3(range, range, 1f);

        // 스킬 차는거 표시
        float elapseTime = 0f;

        while (elapseTime < delay)
        {
            elapseTime += Time.deltaTime;

            circleChargingRangeDecals[0].size = new Vector3(range * (elapseTime / delay), range * (elapseTime / delay), 1f);

            yield return null;
        }
        yield return null;

        // 사거리 표시 없에기
        circleFullRangeDecals[0].size = new Vector3(0f, 0f, 0f);
        circleChargingRangeDecals[0].size = new Vector3(0f, 0f, 0f);

        // attackCollider 활성화
        circleAttackColliders[0].SetActive(true);

        yield return attackColliderTime;

        // attackCollider 비활성화
        circleAttackColliders[0].SetActive(false);
    }

    // 부채꼴 모양 만듦.
    private Mesh BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ThickFanMesh";

        int vertCount = (segmentCount + 2) * 2; // 상단 + 하단 점
        Vector3[] vertices = new Vector3[vertCount];
        int vertIndex = 0;

        // 상단(Top) 점 계산
        float angleStep = angle / segmentCount * Mathf.Deg2Rad;
        vertices[vertIndex++] = Vector3.zero; // 상단 중심점
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, thickness / 2); // 상단 외곽
        }

        // 하단(Bottom) 점 계산
        vertices[vertIndex++] = new Vector3(0, 0, -thickness / 2); // 하단 중심점
        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = i * angleStep;
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            vertices[vertIndex++] = new Vector3(x, y, -thickness / 2); // 하단 외곽
        }

        // 삼각형 인덱스 배열 크기 계산 (상단 + 하단 + 측면)
        int[] triangles = new int[segmentCount * 6 + segmentCount * 6 + segmentCount * 6];
        int triIndex = 0;

        // 상단 삼각형
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = 0; // 중심점
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }

        // 하단 삼각형
        int bottomStart = segmentCount + 2;
        for (int i = 1; i <= segmentCount; i++)
        {
            triangles[triIndex++] = bottomStart; // 중심점
            triangles[triIndex++] = bottomStart + i + 1;
            triangles[triIndex++] = bottomStart + i;
        }

        // 측면 삼각형
        for (int i = 1; i <= segmentCount; i++)
        {
            // 외곽 상단-하단 연결 (앞면)
            int topOuter = i;
            int bottomOuter = bottomStart + i;
            triangles[triIndex++] = topOuter;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = topOuter + 1;

            triangles[triIndex++] = topOuter + 1;
            triangles[triIndex++] = bottomOuter;
            triangles[triIndex++] = bottomOuter + 1;
        }

        // UV 및 노멀 설정
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

        // Mesh 데이터 설정
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }

    // 랜덤 타겟 설정
    private void SetRandomTarget(GameObject _target)
    {
        randomTarget = _target;
    }
}
