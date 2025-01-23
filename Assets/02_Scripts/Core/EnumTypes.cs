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
        Idle,       // 이동 및 대기 
        Stagger,    // 피격
        Action,     // 액션을 취하는 중 (공격, 부활, 상호작용 등등)
        Dash,       // 회피
        Death       // 사망
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