using UnityEngine;

using EnumTypes;
public abstract class BasePlayerState
{
    protected PlayerStateType stateType;
    protected PlayerManager playerMng { get; private set; }

    public PlayerStateType StateType
    {
        get { return stateType; }
    }

    public BasePlayerState(PlayerManager playerMng)
    {
        this.playerMng = playerMng;
    }

    public abstract void OnEnterState();
    public abstract void OnUpdateState();
    public abstract void OnExitState();
}
