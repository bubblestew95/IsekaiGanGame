using UnityEngine;

/// <summary>
/// 일정 범위 내에 일정 시간동안 공격을 가하는 스킬.
/// </summary>
[CreateAssetMenu(fileName = "AOESkill", menuName = "Scriptable Objects/Player Skill/AOE")]
public class AOESkill : AttackSkill
{
    public float maxRange = 5f;
    public float attackArea = 1f;
    public float duration = 2f;
    public float damageTickTime = 0.5f;

    public override void UseSkill(PlayerManager _player, float multiply)
    {
        base.UseSkill(_player, multiply);
    }
}
