using UnityEngine;

public class PlayerAttackManager
{
    private PlayerManager playerManager = null;

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
    }

    public void AddDamageToBoss(int _damage, float _aggro)
    {
        GameManager.Instance.DamageToBoss(playerManager, _damage, _aggro);
    }
}
