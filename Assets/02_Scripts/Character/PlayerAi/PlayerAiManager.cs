using System.Collections.Generic;
using EnumTypes;
using StructTypes;
using TMPro;
using UnityEngine;

public class PlayerAiManager : MonoBehaviour
{//�÷��̾�AI �Ѱ� ��ũ��Ʈ
    public bool isPlayerAiMode = true; //���� AI ������� �ƴ��� Ȯ���ϴ� bool
    public PlayerManager playerManager; // �÷��̾ �����ϱ����� PlayerManager Ŭ���� ����
    private List<Vector3> rocksPos = new List<Vector3>(); // ������ ��ġ�� �����ϴ� ����Ʈ
    public Vector3 mapCenterPos; // ���� �߾� ��ġ 
    public float rockSize = 2.0f; // ���� ũ�� (���÷� ����, ���� �� ũ�⿡ �°� ����)
    WaitForSeconds wait = new WaitForSeconds(0.5f);
    float safeDistance = 2f;
    // SkillSlot �迭�� ����� ��ų�� ����
    SkillSlot[] skillSlots = new SkillSlot[]
    {
        SkillSlot.Skill_A,
        SkillSlot.Skill_B,
        SkillSlot.Skill_C,
        SkillSlot.BasicAttack
    };

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }
    #region ��ġ�ľ�
    private void UpdateRockPositions()// ����ġ �ľ�
    {
        rocksPos.Clear(); // ���� ����Ʈ�� �ʱ�ȭ
        // ���� "Rock" �±׸� ���� ��� ������Ʈ�� ��ġ�� ����Ʈ�� �ޱ�
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");

        foreach (GameObject rock in rocks)
        {
            rocksPos.Add(rock.transform.position); // ���� ��ġ �߰�
        }
    }
    #endregion
    #region �̵�
    #region �⺻ �̵�
    private void AiMove(float _x, float _z) // �÷��̾� �Ŵ����� ���̽�ƽ �Է��̶� ���� �Է��� �༭ �����̴� �Լ�
    {
        
        if(playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Idle)
        {
            JoystickInputData data = new JoystickInputData();

            // �Է� ���� ��ȿ ������ ����� ���
            if (_x > 1 || _x < -1 || _z > 1 || _z < -1)
            {
                Debug.Log("Ai move input is out of range, Range is (-1 ~ 1) (X:" + _x + ", Z:" + _z + ")");

                // �Է� ���� -1 �Ǵ� 1�� ���� (������ -1, ����� 1)
                data.x = Mathf.Sign(_x); // _x�� ������ -1, ����� 1
                data.z = Mathf.Sign(_z); // _z�� ������ -1, ����� 1
            }
            else
            {
                // �Է� ���� ��ȿ ���� ���� ���� ��� �״�� ���
                data.x = _x;
                data.z = _z;
            }

            // ���̽�ƽ �����͸� �̿��� �̵� ó��
            playerManager.MovementManager.MoveByJoystick(data);
        }
    }
    #endregion
    #region �̵� ��ȭ
    private void HideBehindRock() // �� �ڷ� ���� �Լ� ( ���� AI ���� ���� ����� ȸ�� 
    {
        if (rocksPos.Count == 0) return; // ���� ������ �Լ� ����

        Vector3 playerPos = transform.position;//�÷��̾� ��ġ �ޱ� playerPos
        //�� �߾� ��ġ �ޱ� mapCenterPos        
        Vector3 closestRockPos = Vector3.zero; // ���� ����� �� ��ġ�� ���� �� 
        float closestRockDistance = Mathf.Infinity; // ���� ����� ������ �Ÿ� �ʱ�ȭ
        foreach (Vector3 rockPos in rocksPos)// ���� ����� �� ã��
        {
            float distance = Vector3.Distance(playerPos, rockPos);
            if (distance < closestRockDistance)
            {
                closestRockDistance = distance;
                closestRockPos = rockPos;
            }
        }
        // ���� ����� ���� �������� ��ǥ ��ġ ���
        Vector3 directionFromMapCenter = (closestRockPos - mapCenterPos).normalized; // �� �߽ɿ��� �� ��ġ ����
        Vector3 targetPosition = closestRockPos + directionFromMapCenter * rockSize; // ��ǥ ��ġ�� �� �ڷ�


        // ��ǥ ��ġ�� ����� ��, ��ǥ�� ���� ���� (AI �̵�)
        Vector3 directionToTarget = targetPosition - playerPos;
        float x = directionToTarget.x;
        float z = directionToTarget.z;
        AiMove(x,z);
    }
    private void RunawayFromBossAttackRange() // ���� ���� �������� ����ġ�� �Լ� ( ���� AI ���� ����  
    {
        Vector3 playerPos = transform.position;//�÷��̾� ��ġ �ޱ� playerPos
        //���� ���� ���� �ޱ� attackCenterPos
        //���� ���� ���� �߾� = ���� ���� ����
        //if ���� ���� ������ ��ä���̶�� 
        //  ���� ���� ���� �߾� = ���� ���� ���� + (������ �ٶ󺸰� �ִ� ����.�븻������() * �������� 1/2)
        //�÷��̾� ��ġ
        //�ڷ�ƾ �̵� ���� ������ ����
    }
    private void GoToBossForAttack(float _safeDistance) // �����ϱ� ���� �������� ������ ���ų� �־����� �Լ� ( ���� ���� )
    {
        Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
        Vector3 playerPos = transform.position; // �÷��̾� ��ġ �ޱ�

        // ������ �÷��̾��� ���� ���͸� ���
        Vector3 directionToTarget = bossPos - playerPos;
        float distanceToBoss = directionToTarget.magnitude; // �������� �Ÿ� ���

        // ��ǥ ��ġ���� �ʹ� ��������� �ʵ��� ���� �Ÿ���ŭ �������� �̵�

        if (distanceToBoss < _safeDistance)
        {
            // �ʹ� ����� ���, �������� ���� �Ÿ���ŭ ���������� ������ ����
            directionToTarget = directionToTarget.normalized * (distanceToBoss - _safeDistance);
        }

        // ���� ���͸� x, z ��ǥ�� ��ȯ�Ͽ� AiMove �Լ��� ����
        float x = directionToTarget.x;
        float z = directionToTarget.z;

        AiMove(x, z);
    }
    #endregion
    #endregion
    #region ����
    private void AiUseSkillWithAim(SkillSlot _type) // ��ų �����͸� �޾� ��Ÿ����� ������ �ִٸ� ���� ��ġ�� ��ų���� �ƴϸ� �������� ���� �Լ�
    {
        Vector3 bossPos = GameManager.Instance.GetBossTransform().position; // ���� ��ġ
        Vector3 playerPos = transform.position;//�÷��̾� ��ġ �ޱ� playerPos
        float bossPlayerDistance = Vector3.Distance(playerPos, bossPos); // ������ �÷��̾� ��ġ ���

        RangeSkill rangeSkill = playerManager.SkillManager.GetSkillData(_type) as RangeSkill; // ��ų �����͸� �޾ƿ�
        if(rangeSkill == null) // ���� ��ų�� �ƴϸ� (��������)
        {
            if (bossPlayerDistance < safeDistance) // ������ �÷��̾� ������ �Ÿ��� safeDistance�̸��̶��
            {
                playerManager.InputManager.lastSkillUsePoint = bossPos; // ��ų ����ϴ� ��ġ 
                playerManager.SkillManager.TryUseSkill(_type, bossPos); // ������ġ�� ��ų ���
            }
            else
            {
                Debug.Log("Ai Log Boss is out of Skill range");
                GoToBossForAttack(safeDistance);
            }
        }
        else // ���� ��ų�̸�
        {
            if (bossPlayerDistance < rangeSkill.attackRange) // ��ų �����Ÿ��� ������ �÷��̾� ���� �Ÿ����� ������
            {
                playerManager.InputManager.lastSkillUsePoint = bossPos; // ��ų ����ϴ� ��ġ 
                playerManager.SkillManager.TryUseSkill(_type, bossPos); // ������ġ�� ��ų ���
            }
            else
            {
                Debug.Log("Ai Log Boss is out of Skill range");
                GoToBossForAttack(rangeSkill.attackRange);
            }
        }
    }
    #endregion

    private void Update()
    {
        // �迭�� ��ȸ�ϸ鼭 �� ��ų�� ���� AiUseSkillWithAim�� ȣ��
        foreach (SkillSlot skill in skillSlots)
        {
            AiUseSkillWithAim(skill);
        }
    }
}
