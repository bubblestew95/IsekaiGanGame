using System.Collections;
using UnityEngine;

/// <summary>
/// ���� ���ݰ� ���ÿ� �̵��ϴ� ��ų.
/// </summary>
[CreateAssetMenu(fileName = "MeleeAndMoveSkill", menuName = "Scriptable Objects/Player Skill/MeleeAndMove")]
public class MeleeAndMoveSkill : MeleeSkill
{
    public float moveSpeed = 1f;
    public float moveTime = 1f;
    public bool isForward = true;

    private PlayerSkillMove skillMove = null;

    public MeleeAndMoveSkill()
    {
        skillMove = new PlayerSkillMove();
    }

    public override void UseSkill(PlayerManager _player, float multiply)
    {
        base.UseSkill(_player, multiply);

        Vector3 direction = isForward ? _player.transform.forward : (_player.transform.forward * -1f);
        skillMove.StartPlayerMove(_player, moveSpeed, moveTime, direction);
    }
}
