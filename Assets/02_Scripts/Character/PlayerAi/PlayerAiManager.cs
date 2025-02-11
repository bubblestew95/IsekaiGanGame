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
        JoystickInputData data = new JoystickInputData();
        if (_x>1 || _x<-1 || _z > 1 || _z < -1)
        {
            Debug.Log("Ai move input is out of range, Range is (-1 ~ 1) (X:" + _x + ",Z:" + _z + ")");
        }
        data.x = _x;
        data.z = _z;
        playerManager.MovementManager.MoveByJoystick(data);
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
    private void GoToBossForAttack() // �����ϱ� ���� �������� ������ ���� �Լ� ( ���� ���� 
    {
        Vector3 bossPos = playerManager.InputManager.lastSkillUsePoint; // ���� ��ġ
        Vector3 playerPos = transform.position;//�÷��̾� ��ġ �ޱ� playerPos
        // ��ǥ ��ġ�� ����� ��, ��ǥ�� ���� ���� (AI �̵�)
        Vector3 directionToTarget = bossPos - playerPos;
        float x = directionToTarget.x;
        float z = directionToTarget.z;
        AiMove(x, z);
    }
    #endregion
    #endregion
    #region ����
    private void AiUseSkillWithAim(SkillSlot _type) // ��ų �����͸� �޾� ��Ÿ����� ������ �ִٸ� ���� ��ġ�� ��ų���� �ƴϸ� �������� ���� �Լ�
    {
        Vector3 bossPos = playerManager.InputManager.lastSkillUsePoint; // ���� ��ġ
        Vector3 playerPos = transform.position;//�÷��̾� ��ġ �ޱ� playerPos
        float bossPlayerDistance = Vector3.Distance(playerPos, bossPos); // ������ �÷��̾� ��ġ ���

        RangeSkill rangeSkill = playerManager.SkillManager.GetSkillData(_type) as RangeSkill; // ��ų �����͸� �޾ƿ�
        if(rangeSkill == null) // ���� ��ų�� �ƴϸ� (��������)
        {
            if (bossPlayerDistance < 1f) // ������ �÷��̾� ������ �Ÿ��� 1�̸��̶��
            {
                playerManager.SkillManager.TryUseSkill(_type, bossPos); // ������ġ�� ��ų ���
            }
            else
            {
                Debug.Log("Ai Log Boss is out of Skill range");
                GoToBossForAttack();
            }
        }
        else // ���� ��ų�̸�
        {
            if (bossPlayerDistance < rangeSkill.attackRange) // ��ų �����Ÿ��� ������ �÷��̾� ���� �Ÿ����� ������
            {
                playerManager.SkillManager.TryUseSkill(_type, bossPos); // ������ġ�� ��ų ���
            }
            else
            {
                Debug.Log("Ai Log Boss is out of Skill range");
                GoToBossForAttack();
            }
        }
    }
    #endregion

    private void Update()
    {
        AiUseSkillWithAim(SkillSlot.Skill_A);
        AiUseSkillWithAim(SkillSlot.Skill_B);
        AiUseSkillWithAim(SkillSlot.Skill_C);
        AiUseSkillWithAim(SkillSlot.BasicAttack);
    }
}
