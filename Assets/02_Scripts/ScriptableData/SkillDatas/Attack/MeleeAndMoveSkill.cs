using System.Collections;
using UnityEngine;

/// <summary>
/// ���� ���ݰ� ���ÿ� �̵��ϴ� ��ų.
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
