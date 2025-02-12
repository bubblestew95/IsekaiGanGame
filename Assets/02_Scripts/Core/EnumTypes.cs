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
        Action,     // �׼��� ���ϴ� �� (����, ��Ȱ, ��ȣ�ۿ� ���)
        Dash,       // ȸ��
        Death       // ���
    }

    public enum StatusEffectType
    {
        Immune,     // ���� ����.
        Evade,       // ȸ�� ����. Ư�� ���ݿ��� �µ��� �����ؾ� �� ��.
        Stun,       // ���� ����. �Ƹ��� �׷α���� ���� �� �� ���� ��?
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