using UnityEngine;

/// <summary>
/// ���� ���� ��ų�� �����ϴ� ��ũ���ͺ� ������
/// </summary>
[CreateAssetMenu(fileName = "MeleeSkill", menuName = "Scriptable Objects/Player Skill/Melee")]
public class MeleeSkill : AttackSkill
{
    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        _player.AttackManager.AddDamageToBoss(DamageCalculate(_player), aggro);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);

        _player.AttackManager.DisableMeleeAttack();
    }
}
