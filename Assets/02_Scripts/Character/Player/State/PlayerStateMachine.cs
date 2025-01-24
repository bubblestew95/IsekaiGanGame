using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class PlayerStateMachine
{
    private BasePlayerState currentState = null;

    private Dictionary<PlayerStateType, BasePlayerState> stateMap = new Dictionary<PlayerStateType, BasePlayerState>();

    public BasePlayerState CurrentState
    {
        get { return currentState; }
    }

    public BasePlayerState GetState(PlayerStateType _stateType)
    {
        if(stateMap.TryGetValue(_stateType, out BasePlayerState playerState))
        {
            return playerState;
        }

        return null;
    }

    public void AddState(PlayerStateType _stateType, BasePlayerState _state)
    {
        stateMap.TryAdd(_stateType, _state);
    }

    public void DeleteState(PlayerStateType _stateType)
    {
        stateMap.Remove(_stateType);
    }

    /// <summary>
    /// 상태 머신의 상태를 변화시킨다.
    /// </summary>
    /// <param name="_stateType">변화하고자 하는 상태 타입</param>
    public void ChangeState(PlayerStateType _stateType)
    {
        // 먼저 현재 상태를 종료함.
        if (CurrentState != null)
            currentState.OnExitState();

        // 다음 상태로 전환.
        BasePlayerState nextState = GetState(_stateType);
        if (nextState != null)
        {
            currentState = nextState;
            currentState.OnEnterState();
        }
    }

    public void UpdateState()
    {
        if (currentState != null)
            currentState.OnUpdateState();
    }
}
