using UnityEngine;

namespace EnumTypes
{
    public enum InputButtonType
    {
        None = 0,
        Skill_A,
        Skill_B,
        Skill_C,
        BasicAttack
    }

    public enum PlayerStateType
    {
        Idle,       // �̵� �� ��� 
        Stagger,    // �ǰ�
        Action,     // �׼��� ���ϴ� �� (����, ��Ȱ, ��ȣ�ۿ� ���)
        Dash,       // ȸ��
        Death       // ���
    }

    [System.Serializable]
    public enum SkillRangeType
    {
        None
    }

    [System.Serializable]
    public enum SkillActivatedType
    {
        Casting,
        Holding,
        Immediately
    }
}