using UnityEngine;

/// <summary>
/// 발사체를 발사하는 스킬을 정의하기 위한 스크립터블 오브젝트
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
        // 발사체 스폰 후 정면 방향으로 발사.
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
