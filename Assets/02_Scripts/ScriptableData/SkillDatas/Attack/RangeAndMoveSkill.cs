using System.Collections;
using UnityEngine;

/// <summary>
/// ���Ÿ� ���ݰ� ���ÿ� �̵��ϴ� ��ų.
/// </summary>
[CreateAssetMenu(fileName = "RangeAndMoveSkill", menuName = "Scriptable Objects/Player Skill/RangeAndMove")]
public class RangeAndMoveSkill : RangeSkill
{
    public float moveDistance = 1f;
    public float moveTime = 1f;

    private PlayerSkillMove skillMove = null;

    public RangeAndMoveSkill()
    {
        skillMove = new PlayerSkillMove();
    }

    public override void UseSkill(PlayerManager _player, float multiply)
    {
        base.UseSkill(_player, multiply);

        skillMove.StartPlayerMove(_player, moveDistance, moveTime);
    }
}
