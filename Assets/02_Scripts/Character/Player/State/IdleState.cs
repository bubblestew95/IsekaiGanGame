using EnumTypes;
using UnityEngine;

public class IdleState : BasePlayerState
{
    public IdleState(PlayerManager playerMng) : base(playerMng)
    {
    }

    public override void OnEnterState()
    {
        
    }

    public override void OnExitState()
    {
        playerMng.SetAnimatorWalkSpeed(0f);
    }

    public override void OnUpdateState()
    {
        // ��� ������ ���� ������ �� ����.
        playerMng.MoveByJoystick();

        // ��� ������ ���� ��ų�� ��� ������. ��ų ��� üũ.
        SkillType skillType = playerMng.GetNextSkill();

        if(skillType != SkillType.None)
        {
            playerMng.UseSkill(skillType);
        }
    }
}
