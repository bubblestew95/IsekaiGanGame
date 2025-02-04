using UnityEngine;

/// <summary>
/// �߻�ü�� �߻��ϴ� ��ų�� �����ϱ� ���� ��ũ���ͺ� ������Ʈ
/// </summary>
[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Scriptable Objects/Player Skill/Projectile")]
public class ProjectileSkill : AttackSkill
{
    public GameObject projectilePrefab = null;

    public float projectileSpeed = 1f;

    public override void UseSkill(PlayerManager _player)
    {
        // �߻�ü ���� �� ���� �������� �߻��ϸ� ��.
        base.UseSkill(_player);
    }
}
