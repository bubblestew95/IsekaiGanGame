using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "Scriptable Objects/Player Skill/TP Attack")]
public class Teleport_AttackSkill : AttackSkill
{
    [Header("Teleport Attack Skill")]
    public float teleportDistance = 3f;

    public override void StartSkill(PlayerManager _player)
    {
        base.StartSkill(_player);

        Vector3 position = GameManager.Instance.GetBossTransform().position;

        position += GameManager.Instance.GetBossTransform().forward * -1f * teleportDistance;

        _player.transform.position = position;
        _player.transform.LookAt(GameManager.Instance.GetBossTransform().position);
    }

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        int dmg = DamageCalculate(_player);
        _player.EnableMeleeAttack(dmg, aggro);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);
        _player.DisableMeleeAttack();
    }
}
