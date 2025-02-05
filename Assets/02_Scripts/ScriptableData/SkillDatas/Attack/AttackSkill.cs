using UnityEngine;

using EnumTypes;

public abstract class AttackSkill : PlayerSkillBase
{
    public int damage = 1;
    public float aggro = 1f;

    public override void StartSkill(PlayerManager _player)
    {
        base.StartSkill(_player);
        _player.ChangeState(PlayerStateType.Dash);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);

        _player.ChangeState(PlayerStateType.Idle);
    }
}
