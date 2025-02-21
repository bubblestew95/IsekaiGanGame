using UnityEngine;

/// <summary>
/// �߻�ü�� �߻��ϴ� ��ų�� �����ϱ� ���� ��ũ���ͺ� ������Ʈ
/// </summary>
[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Scriptable Objects/Player Skill/Projectile")]
public class ProjectileSkill : AttackSkill
{
    [Header("Projectile Skill")]
    public float projectileDuration = 3f;
    public float projectileSpeed = 1f;

    public override void UseSkill(PlayerManager _player)
    {
        // �߻�ü ���� �� ���� �������� �߻�.
        base.UseSkill(_player);

        PlayerProjectileManager projectileManager = _player.GetComponent<PlayerProjectileManager>();

        if(projectileManager == null)
        {
            Debug.LogWarning("Projectile Manager is Null!");
            return;
        }

        projectileManager.ShootProjectile(damage, aggro, projectileDuration, projectileSpeed
            , isBackattackEnable, backAttackTimes);
    }
}
