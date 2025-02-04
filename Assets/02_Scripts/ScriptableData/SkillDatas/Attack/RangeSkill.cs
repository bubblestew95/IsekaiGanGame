using UnityEngine;

/// <summary>
/// 레이로 원거리 공격을 처리하는 스킬을 처리하기 위한 스크립터블 오브젝트.
/// </summary>
[CreateAssetMenu(fileName = "RangeSkill", menuName = "Scriptable Objects/Player Skill/Range")]
public class RangeSkill : AttackSkill
{
    public float attackRange = 5f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);
        _player.RayAttack(damage, attackRange);
    }
}
