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

    public void OnPlayerDamaged
        (PlayerManager _damageReceiver, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        ulong cliendId = _damageReceiver.GetComponent<NetworkObject>().OwnerClientId;
        PlayerDamagedRpc(cliendId, _damage, _attackPos, _knockbackDist);
    }
    
    public void OnBossDamaged
        (PlayerManager _damageGiver, int _damage, float _aggro)
    {
        ulong cliendId = _damageGiver.GetComponent<NetworkObject>().OwnerClientId;
        BossDamagedRpc(cliendId, _damage, _aggro);
    }

    #endregion

    #region RPC

        #region Client To Server RPC

    /// <summary>
    /// 플레이어가 데미지를 받았음을 서버에게 알린다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerDamagedRpc
        (ulong _cliendId, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        // 서버에서 다른 클라이언트들에게 특정 플레이어에게 데미지를 적용하라고 명령한다.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        ApplyDamageToPlayerRpc(_cliendId, _damage, _attackPos, _knockbackDist);
    }

    /// <summary>
    /// 보스가 데미지를 받았음을 서버에게 알린다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void BossDamagedRpc
        (ulong _cliendId, int _damage, float _aggro)
    {
        // 다른 클라이언트들에게 보스에게 데미지를 입히라고 명령한다.
        ApplyDamageToBossRpc(_cliendId, _damage, _aggro);
    }

        #endregion

        #region Server To Client RPC

    /// <summary>
    /// 서버에서 특정 플레이어에게 데미지를 적용하도록 모든 클라이언트에게 명령한다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Everyone)]
    private void ApplyDamageToPlayerRpc
        (ulong _cliendId, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        if(multiPlayersMap.TryGetValue(_cliendId, out PlayerManager playerManager))
        {
            // GameManager.Instance.ApplyDamageToPlayer(playerManager, _damage, _attackPos, _knockbackDist);
        }
    }

    /// <summary>
    /// 서버에서 보스에게 데미지를 적용하도록 모든 클라이언트에게 명령한다.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Everyone)]
    private void ApplyDamageToBossRpc
        (ulong _cliendId, int _damage, float _aggro)
    {
        if (multiPlayersMap.TryGetValue(_cliendId, out PlayerManager playerManager))
        {
            // GameManager.Instance.ApplyDamageToBoss(playerManager, _damage, _aggro);
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
