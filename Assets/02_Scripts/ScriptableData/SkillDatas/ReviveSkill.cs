using UnityEngine;

using EnumTypes;
[CreateAssetMenu(fileName = "ReviveSkill", menuName = "Scriptable Objects/Player Skill/Revive")]
public class ReviveSkill : PlayerSkillBase
{
    public float reviveRange = 2f;

    public override void UseSkill(PlayerManager _player)
    {
        base.UseSkill(_player);

        ReviveNearPlayers(_player);
    }

    private void ReviveNearPlayers(PlayerManager _player)
    {
        Collider[] hits = Physics.OverlapSphere(_player.transform.position, reviveRange, LayerMask.GetMask("Player"));

        foreach (Collider hitCollider in hits)
        {
            Debug.LogFormat("{0} near by iii");

            //PlayerManager playerManager = hitCollider.GetComponent<PlayerManager>();
            //if (playerManager != null && playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Death)
            //{
            //    playerManager.StatusManager.SetMaxHp(playerManager.StatusManager.MaxHp / 2);
            //    playerManager.StatusManager.SetCurrentHp(playerManager.StatusManager.MaxHp);

            //    playerManager.BattleUIManager.UpdatePlayerHp();
            //    playerManager.AnimationManager.PlayGetRevivedAnimation();
            //    playerManager.GetComponent<CharacterController>().enabled = true;
            //}
        }
    }
}
