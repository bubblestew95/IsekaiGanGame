using UnityEngine;

/// <summary>
/// ���� ���� ���� ���� �ð����� ������ ���ϴ� ��ų.
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
