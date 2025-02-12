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
    float safeDistance = 1f;

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

    #endregion

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
        playerManager.InputManager.lastSkillUsePoint = new Vector3(bossPos.x, 0, bossPos.z);
        playerManager.SkillManager.TryUseSkill(skillType, bossPos);
        Debug.Log("������ų");
    }

    private void MoveTowardsBoss(Vector3 bossPos)
    {
        Vector3 directionToTarget = bossPos - transform.position;
        AiMove(directionToTarget.x, directionToTarget.z);
    }    
    private IEnumerator MoveBackwardsBoss()
    {
        isMoveBack = true;
        while (isMoveBack == true)
        {
            Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
            Vector3 directionToTarget = transform.position - bossPos;
            AiMove(directionToTarget.x, directionToTarget.z);
            yield return null;
        }
        yield break;
    }

    #endregion

    #region AI �ֱ����� �ൿ

    private void Update()
    {
        if (isMoveBack == false)// �������� �ƴ϶��
        {
            // �������� �ϳ��� ��ų ����
            int randomIndex = Random.Range(0, skillSlots.Length);
            // ���õ� ��ų�� ��ų�� ����ϰ�, ������ �̵�
            UseSkillWithApproach(skillSlots[randomIndex]);
        }
    }

    #endregion
}
