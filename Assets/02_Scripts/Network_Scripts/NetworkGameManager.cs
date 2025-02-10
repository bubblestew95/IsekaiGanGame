using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    /// <summary>
    /// 클라이언트 ID를 키로 가지고, 해당 클라이언트가 소유한 PlayerManger를 값으로 가지는 딕셔너리.
    /// </summary>
    private Dictionary<ulong, PlayerManager> multiPlayersMap = null;

    #region Public Functions

    public void SendPlayerHittedToServer
        (PlayerManager _playerManager, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        ulong cliendId = _playerManager.GetComponent<NetworkObject>().OwnerClientId;
        PlayerDamagedRpc(cliendId, _damage, _attackPos, _knockbackDist);
    }

    #endregion

    #region RPC

        #region Client To Server RPC

    [Rpc(SendTo.Server)]
    private void PlayerDamagedRpc
        (ulong _cliendId, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        ApplyDamageToPlayerRpc(_cliendId, _damage, _attackPos, _knockbackDist);
    }

        #endregion

        #region Server To Client RPC

    [Rpc(SendTo.Everyone)]
    private void ApplyDamageToPlayerRpc
        (ulong _cliendId, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        if(multiPlayersMap.TryGetValue(_cliendId, out PlayerManager playerManager))
        {
            GameManager.Instance.ApplyDamageToPlayer(playerManager, _damage, _attackPos, _knockbackDist);
        }
    }

        #endregion

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        multiPlayersMap = new Dictionary<ulong, PlayerManager>();
    }

    #endregion
}
