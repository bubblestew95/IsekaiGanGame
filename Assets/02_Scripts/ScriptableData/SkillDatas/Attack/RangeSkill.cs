using UnityEngine;

/// <summary>
/// ���̷� ���Ÿ� ������ ó���ϴ� ��ų�� ó���ϱ� ���� ��ũ���ͺ� ������Ʈ.
/// </summary>
[CreateAssetMenu(fileName = "RangeSkill", menuName = "Scriptable Objects/Player Skill/Range")]
public class RangeSkill : AttackSkill
{
    [Header("Range Skill")]
    public float attackRange = 5f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        Transform startTr = _player.RangeAttackStartTr;
        Ray ray = new Ray(startTr.position, _player.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange, bossLayerMask))
        {
            GameManager.Instance.DamageToBoss(_player, DamageCalculate(_player), aggro);
        }
    }
}
