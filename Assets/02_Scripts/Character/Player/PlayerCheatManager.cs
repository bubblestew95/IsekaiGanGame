using UnityEngine;

using EnumTypes;

public class PlayerCheatManager : MonoBehaviour
{
    private PlayerManager playerManager = null;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            playerManager.StatusManager.OnDamaged(10);
            playerManager.BattleUIManager.UpdatePlayerHp();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Death)
            {
                playerManager.StatusManager.SetMaxHp(playerManager.StatusManager.MaxHp / 2);
                playerManager.StatusManager.SetCurrentHp(playerManager.StatusManager.MaxHp);

                playerManager.BattleUIManager.UpdatePlayerHp();
                playerManager.AnimationManager.PlayGetRevivedAnimation();
                playerManager.GetComponent<CharacterController>().enabled = true;
                playerManager.ChangeState(PlayerStateType.Idle);
            }
        }
    }
}
