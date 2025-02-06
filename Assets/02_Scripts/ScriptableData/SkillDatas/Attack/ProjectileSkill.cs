using UnityEngine;

/// <summary>
/// �߻�ü�� �߻��ϴ� ��ų�� �����ϱ� ���� ��ũ���ͺ� ������Ʈ
/// </summary>
[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Scriptable Objects/Player Skill/Projectile")]
public class ProjectileSkill : AttackSkill
{
    [Header("Projectile Skill")]
    public GameObject projectilePrefab = null;

    public float projectileDuration = 3f;
    public float projectileSpeed = 1f;

    public override void UseSkill(PlayerManager _player)
    {
        // �߻�ü ���� �� ���� �������� �߻�.
        base.UseSkill(_player);

        GameObject projectileObj = Instantiate
            (
            projectilePrefab, 
            _player.RangeAttackStartTr.position, 
            _player.transform.rotation
            );

        if(projectileObj != null)
        {
            var projectile = projectileObj.GetComponent<Projectile>();

            projectile.Init(_player, damage, aggro);
            projectile.ShootProjectile(projectileDuration, projectileSpeed);
        }
    }
}
