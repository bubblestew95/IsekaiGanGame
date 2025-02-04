using UnityEngine;

using EnumTypes;

public abstract class AttackSkill : PlayerSkillBase
{
    float damage = 1f;
    float aggro = 1f;

    public override void UseSkill(PlayerManager _player, float multiply)
    {
        _player.ChangeState(PlayerStateType.Action);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);

        _player.ChangeState(PlayerStateType.Idle);
    }
}
