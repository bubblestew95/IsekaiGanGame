using UnityEngine;

public abstract class BasePlayerState
{
    protected PlayerManager playerMng { get; private set; }

    public BasePlayerState(PlayerManager playerMng)
    {
        this.playerMng = playerMng;
    }

    public abstract void OnEnterState();
    public abstract void OnUpdateState();
    public abstract void OnExitState();
}
