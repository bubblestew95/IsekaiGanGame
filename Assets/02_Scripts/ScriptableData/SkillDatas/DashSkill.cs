using System.Collections;
using UnityEngine;

using EnumTypes;

[CreateAssetMenu(fileName = "DashSkill", menuName = "Scriptable Objects/Player Skill/Dash")]
public class DashSkill : PlayerSkillBase
{
    public float dashDistance = 5f;
    public float dashTime = 1f;

    private PlayerSkillMove skillMove = null;

    public DashSkill()
    {
        skillMove = new PlayerSkillMove();
    }

    public override void UseSkill(PlayerManager _player, float multiply)
    {
        _player.ChangeState(PlayerStateType.Dash);

        skillMove.StartPlayerMove(_player, dashDistance, dashTime);
    }

    public override void EndSkill(PlayerManager _player)
    {
        base.EndSkill(_player);

        _player.ChangeState(PlayerStateType.Idle);
    }
}
