using System.Collections;
using UnityEngine;

/// <summary>
/// 물리 공격과 동시에 이동하는 스킬.
/// </summary>
[CreateAssetMenu(fileName = "MeleeAndMoveSkill", menuName = "Scriptable Objects/Player Skill/MeleeAndMove")]
public class MeleeAndMoveSkill : MeleeSkill
{
    public float moveDistance = 1f;
    public float moveTime = 1f;

    private PlayerSkillMove skillMove = null;

    public MeleeAndMoveSkill()
    {
        skillMove = new PlayerSkillMove();
    }

    public override void UseSkill(PlayerManager _player, float multiply)
    {
        base.UseSkill(_player, multiply);

        skillMove.StartPlayerMove(_player, moveDistance, moveTime);
    }
}
