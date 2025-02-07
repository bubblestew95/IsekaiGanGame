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
    }

    public void PlayDamagedAnimation()
    {
        if (networkAnimator != null)
            networkAnimator.SetTrigger(animId_Damaged);
        else
            animator.SetTrigger(animId_Damaged);
    }

    public void PlayDeathAnimation()
    {
        if (networkAnimator != null)
            networkAnimator.SetTrigger(animId_Death);
        else
            animator.SetTrigger(animId_Death);
    }

    public void SetAnimatorWalkSpeed(float _speed)
    {
        animator.SetFloat(AnimId_Speed, _speed);
    }
}
