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
        Normal,     // �����̻� ����
        Immune,     // ���� ����.
        Evade,       // ȸ�� ����. Ư�� ���ݿ��� �µ��� �����ؾ� �� ��.
        Stun,       // ���� ����. �Ƹ��� �׷α���� ���� �� �� ���� ��?
        Vuln_Stack  // �޴� ���ط� ���� ����.
    }

    [System.Serializable]
    public enum PlayerSkillRangeType
    {
        None
    }

    [System.Serializable]
    public enum PlayerSkillActivatedType
    {
        Casting,        // ��ų ���� ������ ����� ��� �ð��� �ִ� Ÿ��
        Holding,        // ��ų ���� �� ����ؼ� ��ų�� ������ Ÿ��
        Immediately     // ��� Ÿ��
    }
}