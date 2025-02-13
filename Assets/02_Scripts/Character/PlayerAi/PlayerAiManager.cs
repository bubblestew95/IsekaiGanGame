using System;
using System.Collections;
using System.Collections.Generic;
using EnumTypes;
using StructTypes;
using UnityEngine;

public class PlayerAiManager : MonoBehaviour
{
    public bool isPlayerAiMode = true; // ���� AI ������� �ƴ��� Ȯ���ϴ� bool
    public bool isMoveBack = false; // ���� ���� Ȯ���ϴ� bool
    public PlayerManager playerManager; // �÷��̾ �����ϱ����� PlayerManager Ŭ���� ����
    private List<Vector3> rocksPos = new List<Vector3>(); // ������ ��ġ�� �����ϴ� ����Ʈ
    public Vector3 mapCenterPos; // ���� �߾� ��ġ 
    public float rockSize = 2.0f; // ���� ũ�� (���÷� ����, ���� �� ũ�⿡ �°� ����)
    float safeDistance = 3f;
    private Action Action;
    // SkillSlot �迭�� ����� ��ų�� ����
    SkillSlot[] skillSlots = new SkillSlot[]
    {
        SkillSlot.Skill_A,
        SkillSlot.Skill_B,
        SkillSlot.Skill_C
    };

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        BossBT.AttackStartCallback += () => StartCoroutine(MoveBackwardsBoss()); // ���� ���� �ݹ� �ڷ�ƾ �÷��̾� ���� ����
        BossBT.AttackEndCallback += () => isMoveBack = false;
        BossBT.SpecialAttackEndCallback += () => StartCoroutine(MoveBehindRock()); // ���� ����� ���� �ڷ�ƾ �÷��̾� ���ڷ� ����
        Action += () => StartCoroutine(MoveBackwardsBoss());
    }

    #region ��ġ �ľ�

    private void UpdateRockPositions() // �� ��ġ �ľ�
    {
        rocksPos.Clear();
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");

        foreach (GameObject rock in rocks)
        {
            rocksPos.Add(rock.transform.position);
        }
    }

    // ���� ����� ���� ã�� �Լ�
    private Vector3 FindClosestRock()
    {
        Vector3 closestRock = Vector3.zero;
        float closestDistance = float.MaxValue;

        // ������ ��ġ �� ���� ����� ���� ã��
        foreach (Vector3 rockPos in rocksPos)
        {
            float distanceToRock = Vector3.Distance(rockPos, transform.position); // ���� �÷��̾��� �Ÿ�
            if (distanceToRock < closestDistance) // ���� ����� ���� ã��
            {
                closestDistance = distanceToRock;
                closestRock = rockPos;
            }
        }

        return closestRock; // ���� ����� ���� ��ġ ��ȯ
    }

    #endregion
    // �� �ڷ� ������ ��ġ�� ����ϴ� �Լ�
    private Vector3 GetPositionBehindRock(Vector3 rockPos)
    {
        // ���� �������� ��ġ�� ��� (�÷��̾�� ���� ������� ��ġ)
        Vector3 directionToRock = (transform.position - rockPos).normalized; // ���� �÷��̾��� ��� ����
        Vector3 positionBehindRock = rockPos + directionToRock * rockSize; // �� �ڷ� �̵� (���� ũ�⸸ŭ ������ ��)

        return positionBehindRock; // ���� ��ġ ��ȯ
    }
    #region �̵� ó��

    private void AiMove(float _x, float _z) // �÷��̾��� �̵� ó�� �Լ�
    {
        if (playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Idle)
        {
            JoystickInputData data = new JoystickInputData
            {
                x = Mathf.Clamp(_x, -1f, 1f),
                z = Mathf.Clamp(_z, -1f, 1f)
            };

            Vector3 direction = new Vector3(data.x, 0, data.z);
            if (direction.magnitude > 1)
            {
                direction.Normalize();
                data.x = direction.x;
                data.z = direction.z;
            }

            playerManager.MovementManager.MoveByJoystick(data);
        }
    }

    #endregion

    #region ��ų ��� �� �̵� ó��

    private void UseSkillWithApproach(SkillSlot skillType)
    {
        Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
        Vector3 playerPos = transform.position;
        float distanceToBoss = Vector3.Distance(playerPos, bossPos);

        RangeSkill rangeSkill = playerManager.SkillManager.GetSkillData(skillType) as RangeSkill;

        if (rangeSkill == null) // ���� ��ų
        {
            HandleMeleeSkill(skillType, distanceToBoss, bossPos);
        }
        else // ���� ��ų
        {
            HandleRangeSkill(skillType, distanceToBoss, rangeSkill.attackRange, bossPos);
        }
    }

    private void HandleMeleeSkill(SkillSlot skillType, float distanceToBoss, Vector3 bossPos)
    {
        if (distanceToBoss <= safeDistance)
        {
            UseMeleeSkill(skillType, bossPos);
        }
        else
        {
            MoveTowardsBoss(bossPos); // ���� ��ų�� ����ϱ� ���� �������� ����
        }
    }

    private void HandleRangeSkill(SkillSlot skillType, float distanceToBoss, float attackRange, Vector3 bossPos)
    {
        if (distanceToBoss <= attackRange)
        {
            UseRangeSkill(skillType, bossPos);
        }
        else
        {
            MoveTowardsBoss(bossPos); // ���� ��ų�� ����Ϸ��� �������� ����
        }
    }

    private void UseMeleeSkill(SkillSlot skillType, Vector3 bossPos)
    {
        playerManager.InputManager.lastSkillUsePoint = new Vector3(bossPos.x, 0, bossPos.z);
        playerManager.SkillManager.TryUseSkill(skillType, bossPos);
        Debug.Log("������ų");
    }

    private void UseRangeSkill(SkillSlot skillType, Vector3 bossPos)
    {
        playerManager.InputManager.lastSkillUsePoint = new Vector3(bossPos.x, transform.position.y, bossPos.z);
        playerManager.SkillManager.TryUseSkill(skillType, bossPos);
        Debug.Log("������ų");
    }

    private void MoveTowardsBoss(Vector3 bossPos)
    {
        Vector3 directionToTarget = bossPos - transform.position;
        AiMove(directionToTarget.x, directionToTarget.z);
    }
    private bool isCoroutineRunning = false; // �ڷ�ƾ ���� ���θ� ����

    private IEnumerator MoveBackwardsBoss()
    {
        // �̹� �ڷ�ƾ�� ���� ���̸� �ٽ� �������� �ʵ��� ����
        if (isCoroutineRunning)
        {
            yield break; // �̹� ���� ���� �ڷ�ƾ�� ������ ����
        }

        isCoroutineRunning = true; // �ڷ�ƾ ����

        while (isMoveBack)
        {
            // ������ ��ġ�� �ڽ��� ��ġ�� ���Ͽ� �̵� ������ ���
            Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
            Vector3 directionToTarget = transform.position - bossPos;

            // �̵� ó�� �Լ� ȣ��
            AiMove(directionToTarget.x, directionToTarget.z);

            yield return null;
        }

        isCoroutineRunning = false; // �ڷ�ƾ ����
        yield break;
    }
    private IEnumerator MoveBehindRock()
    {
        // �̹� �ڷ�ƾ�� ���� ���̸� ���� ��Ű�� �� �ڷ�ƾ�� �������� ����
        if (isCoroutineRunning)
        {
            yield break; // �̹� ���� ���̸� ���� �ڷ�ƾ�� �����ϰ� ���� �������� ����
        }

        isCoroutineRunning = true; // �ڷ�ƾ ����

        // �� ��ġ ����Ʈ�� ��� ������ ������Ʈ
        UpdateRockPositions();

        // ���� ������ (���� ã�� ���� ���) �ٷ� ����
        if (rocksPos.Count == 0)
        {
            isCoroutineRunning = false;
            yield break;
        }

        // ���� ����� �� ã��
        Vector3 closestRock = FindClosestRock();

        // ���� ����� ���� �������� ��ǥ ��ġ ���
        Vector3 targetPos = GetPositionBehindRock(closestRock);

        // ��ǥ ��ġ���� �̵�
        while (isMoveBack)
        {
            // ���� ��ġ���� ��ǥ ��ġ�� �̵��ϴ� ���� ���
            Vector3 directionToTarget = targetPos - transform.position;
            AiMove(directionToTarget.x, directionToTarget.z); // �̵� ó��

            // ��ǥ ��ġ�� �����ߴ��� üũ
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            {
                break; // ��ǥ ��ġ�� �����ϸ� ����
            }

            yield return null; // �� ������ ���
        }

        isCoroutineRunning = false; // �ڷ�ƾ ����
        yield break;
    }


    #endregion

    #region AI �ֱ����� �ൿ

    private void Update()
    {
        Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
        Vector3 playerPos = transform.position;
        float distanceToBoss = Vector3.Distance(playerPos, bossPos);
        if (isMoveBack == false || distanceToBoss > safeDistance)// �������� �ƴ϶��
        {
            // �������� �ϳ��� ��ų ����
            int randomIndex = UnityEngine.Random.Range(0, skillSlots.Length);
            // ���õ� ��ų�� ��ų�� ����ϰ�, ������ �̵�
            UseSkillWithApproach(skillSlots[randomIndex]);
        }
        else
        {
            StartCoroutine(MoveBackwardsBoss());
        }
    }

    #endregion
}
