using UnityEngine;

using EnumTypes;

public abstract class AttackSkill : PlayerSkillBase
{
    [Header("Attack Skill")]
    public int damage = 1;
    public float aggro = 1f;
    public bool isBackattackEnable = false;
    public int backAttackTimes = 3;

    public override void StartSkill(PlayerManager _player)
    {
        base.StartSkill(_player);

        _player.ChangeState(PlayerStateType.Action);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);

        _player.ChangeState(PlayerStateType.Idle);
    }

    public int DamageCalculate(PlayerManager _player)
    {
        if (isBackattackEnable && _player.IsPlayerBehindBoss())
        {
            return damage * backAttackTimes;
        }
        else
        {
            return damage;
        }
    }
}
