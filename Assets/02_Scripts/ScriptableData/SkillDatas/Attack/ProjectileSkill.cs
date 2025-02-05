using UnityEngine;

/// <summary>
/// 발사체를 발사하는 스킬을 정의하기 위한 스크립터블 오브젝트
/// </summary>
[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Scriptable Objects/Player Skill/Projectile")]
public class ProjectileSkill : AttackSkill
{
    public GameObject projectilePrefab = null;

    public float projectileSpeed = 1f;

    public override void UseSkill(PlayerManager _player)
    {
        // 발사체 스폰 후 정면 방향으로 발사하면 됨.
        base.UseSkill(_player);
    }
}
