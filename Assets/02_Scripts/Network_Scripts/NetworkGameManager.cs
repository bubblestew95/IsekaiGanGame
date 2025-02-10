using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    /// <summary>
    /// Ŭ���̾�Ʈ ID�� Ű�� ������, �ش� Ŭ���̾�Ʈ�� ������ PlayerManger�� ������ ������ ��ųʸ�.
    /// </summary>
    private Dictionary<ulong, PlayerManager> multiPlayersMap = null;

    #region Public Functions

    public void OnPlayerDamaged
        (PlayerManager _playerManager, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        ulong cliendId = _playerManager.GetComponent<NetworkObject>().OwnerClientId;
        PlayerDamagedRpc(cliendId, _damage, _attackPos, _knockbackDist);
    }

    #endregion

    #region RPC

        #region Client To Server RPC

    /// <summary>
    /// �÷��̾ �������� �޾����� �������� �˸���.
    /// </summary>
    /// <param name="_cliendId"></param>
    /// <param name="_damage"></param>
    /// <param name="_attackPos"></param>
    /// <param name="_knockbackDist"></param>
    [Rpc(SendTo.Server)]
    private void PlayerDamagedRpc
        (ulong _cliendId, int _damage, Vector3 _attackPos, float _knockbackDist)
    {
        ApplyDamageToPlayerRpc(_cliendId, _damage, _attackPos, _knockbackDist);
    }

        #endregion

        #region Server To Client RPC

    /// <summary>
    /// �������� Ư�� �÷��̾�� �������� �����ϵ��� ��� Ŭ���̾�Ʈ���� ����Ѵ�.
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
