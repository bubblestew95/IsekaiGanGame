using EnumTypes;
using StructTypes;
using UnityEngine;

public class PlayerAiManager : MonoBehaviour
{//�÷��̾�AI �Ѱ� ��ũ��Ʈ
    public bool isPlayerAiMode = true; //���� AI ������� �ƴ��� Ȯ���ϴ� bool
    public PlayerManager playerManager; // �÷��̾ �����ϱ����� PlayerManager Ŭ���� ����


    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }   
    private void AiUseSkillWithAim(SkillSlot _type) // ��ų���� �Լ�
    {
        playerManager.InputManager.lastSkillUsePoint = Vector3.zero;
        Vector3 bossPos = playerManager.InputManager.lastSkillUsePoint;
        playerManager.SkillManager.TryUseSkill(_type, bossPos);
    }
    private void AiMove(float _x, float _z) //�����̴� �Լ�
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
    private void HideBehindRock() // �� �ڷ� ���� �Լ� ( ���� AI ���� ���� ����� ȸ�� 
    {
        //�÷��̾� ��ġ �ޱ� playerPos
        //�� �߾� ��ġ �ޱ� mapCenterPos
        //��� �� ��ġ list�� �ޱ� List<rocks>
        //��� �� ��ġ�� �÷��̾� ��ġ ����� ���� ����� �� ��ġ Ȯ��
        //�÷��̾� ��ǥ ��ġ = ����� �� ��ġ + (�� ��ġ ���� - �� �߾���ġ ���� ).�븻������() * ���� ũ��
        //navmeshagent�� ��ǥ ��ġ���� �̵� (�ڷ�ƾ �ʿ�)
    }
    private void RunawayFromBossAttackRange() // ���� ���� �������� ����ġ�� �Լ� ( ���� AI ���� ����  
    {
        //�÷��̾� ��ġ �ޱ� playerPos
        //���� ���� ���� �ޱ� attackCenterPos
        //���� ���� ���� �߾� = ���� ���� ����
        //if ���� ���� ������ ��ä���̶�� 
        //  ���� ���� ���� �߾� = ���� ���� ���� + (������ �ٶ󺸰� �ִ� ����.�븻������() * �������� 1/2)
        //�÷��̾� ��ġ - 
        //�ڷ�ƾ �̵� ���� ������ ����
    }
    private void GoToBossForAttack() // �����ϱ����� �������� ������ ���� �Լ� ( ���� ���� 
    {
        //�ڷ�ƾ �̵� ������ ����
        //{�÷��̾� ��ġ �ޱ� playerPos
        //���� ��ġ �ޱ� bossPos}
    }
}
