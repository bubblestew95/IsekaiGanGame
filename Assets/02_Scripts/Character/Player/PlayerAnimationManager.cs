using UnityEngine;

public class PlayerAnimationManager
{
    private PlayerManager playerManager = null;
    private Animator animator = null;

    private int animId_Damaged = 0;
    private int animId_Death = 0;

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
        animator =  _playerManager.GetComponent<Animator>();

        animId_Damaged = Animator.StringToHash("Damaged");
        animId_Death = Animator.StringToHash("Death");

    }

    public void PlayDamagedAnimation()
    {
        animator.SetTrigger(animId_Damaged);
    }

    public void PlayDeathAnimation()
    {
        animator.SetTrigger(animId_Death);
    }
}
