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
    /// ���� �ӽ��� ���¸� ��ȭ��Ų��.
    /// </summary>
    /// <param name="_stateType">��ȭ�ϰ��� �ϴ� ���� Ÿ��</param>
    public void ChangeState(PlayerStateType _stateType)
    {
        BasePlayerState nextState = GetState(_stateType);

        // ���� �ٲٷ��� ���°� ���� ���¿� ������ ���¶�� �ƹ� �ൿ�� �� �ϰ� ����.
        if (nextState == CurrentState)
            return;

        // ���� ���� ���¸� ������.
        if (CurrentState != null)
            currentState.OnExitState();

        // ���� ���·� ��ȯ.
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
