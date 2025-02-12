using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimationManager
{
    private PlayerManager playerManager = null;
    private Animator animator = null;
    private NetworkAnimator networkAnimator = null;

    private int animId_Damaged = 0;
    private int animId_Death = 0;
    private int animId_Speed = 0;
    private int animId_GetRevived = 0;
    private int animId_ReviveOther = 0;

    public int AnimId_Speed
    {
        get { return animId_Speed; }
    }

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
        animator =  _playerManager.GetComponent<Animator>();
        networkAnimator = _playerManager.GetComponent<NetworkAnimator>();

        animId_Damaged = Animator.StringToHash("Damaged");
        animId_Death = Animator.StringToHash("Death");
        animId_Speed = Animator.StringToHash("Speed");
        animId_GetRevived = Animator.StringToHash("GetRevived");
        animId_ReviveOther = Animator.StringToHash("ReviveOther");

    }

    public void PlayDamagedAnimation()
    {
        if (GameManager.Instance.IsLocalGame)
            animator.SetTrigger(animId_Damaged);
        else
            networkAnimator.SetTrigger(animId_Damaged);
    }

    public void PlayDeathAnimation()
    {
        if (GameManager.Instance.IsLocalGame)
            animator.SetTrigger(animId_Death);
        else
            networkAnimator.SetTrigger(animId_Death);
    }

    public void PlayGetRevivedAnimation()
    {
        if (GameManager.Instance.IsLocalGame)
            animator.SetTrigger(animId_GetRevived);
        else
            networkAnimator.SetTrigger(animId_GetRevived);
    }

    public void SetAnimatorWalkSpeed(float _speed)
    {
        animator.SetFloat(AnimId_Speed, _speed);
    }
}
