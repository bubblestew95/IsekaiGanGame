using UnityEngine;

namespace EnumTypes
{
    public enum SkillType
    {
        None = 0,
        Skill_A,
        Skill_B,
        Skill_C,
        BasicAttack,
        Dash
    }

    public enum PlayerStateType
    {
        Idle,       // �̵� �� ��� 
        Stagger,    // �ǰ�
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

    [System.Serializable]
    public enum PlayerSkillRangeType
    {
        None,
        Direction,  // �÷��̾� ��ġ�� �������� Ư�� ������ ���ؼ� ��ų�� ����.
        AOE,        // Ư�� ��ġ���� ��ų�� ����.
    }

    [System.Serializable]
    public enum PlayerSkillActivatedType
    {
        Casting,        // ��ų ���� ������ ����� ��� �ð��� �ִ� Ÿ��
        Holding,        // ��ų ���� �� ����ؼ� ��ų�� ������ Ÿ��
        Immediately     // ��� Ÿ��
    }
}