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
    }
}
