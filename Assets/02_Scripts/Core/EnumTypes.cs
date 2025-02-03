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
        Idle,       // 이동 및 대기 
        Stagger,    // 피격
        Action,     // 액션을 취하는 중 (공격, 부활, 상호작용 등등)
        Dash,       // 회피
        Death       // 사망
    }

    public enum StatusEffectType
    {
        Immune,     // 무적 상태.
        Evade,       // 회피 상태. 특정 공격에는 맞도록 설계해야 할 듯.
        Stun,       // 스턴 상태. 아마도 그로기랑도 같이 쓸 수 있을 듯?
    }

    [System.Serializable]
    public enum PlayerSkillRangeType
    {
        None,
        Direction,  // 플레이어 위치를 기준으로 특정 방향을 향해서 스킬이 사용됨.
        AOE,        // 특정 위치에서 스킬이 사용됨.
    }

    [System.Serializable]
    public enum PlayerSkillActivatedType
    {
        Casting,        // 스킬 시전 이전에 잠깐의 대기 시간이 있는 타입
        Holding,        // 스킬 시전 중 계속해서 스킬이 나가는 타입
        Immediately     // 즉발 타입
    }
}