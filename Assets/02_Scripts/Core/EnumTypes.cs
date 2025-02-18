using UnityEngine;

namespace EnumTypes
{
    public enum SkillSlot
    {
        None = 0,
        Skill_A,
        Skill_B,
        Skill_C,
        BasicAttack,
        Dash,
        Revive,
    }

    public enum PlayerStateType
    {
        Idle,       // �̵� �� ��� 
        Damaged,    // �ǰ�
        Action,     // �׼��� ���ϴ� �� (��Ȱ, ��ȣ�ۿ� ���)
        Dash,       // ȸ��
        Death,       // ���
        Skill, // ��ų ��� ��
    }

    /// <summary>
    /// ��ų ��� ������ ��� �����ϴ����� �������ִ� Ÿ��
    /// </summary>
    public enum SkillPointType
    {
        None = 0,   // ��� ������ �������� �ʴ� ��ų.
        Direction,  // ������ �����ϰ� ����ϴ� ��ų.
        Position,   // ��ġ�� �����ϰ� ����ϴ� ��ų.
    }

    public enum CharacterClass
    {
        Warrior, // ����
        Mage, // ����
        Archer, // �ü�
        Thief // ����
    }
}