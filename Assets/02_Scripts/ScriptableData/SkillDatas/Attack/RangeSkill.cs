using UnityEngine;

/// <summary>
/// ���̷� ���Ÿ� ������ ó���ϴ� ��ų�� ó���ϱ� ���� ��ũ���ͺ� ������Ʈ.
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
