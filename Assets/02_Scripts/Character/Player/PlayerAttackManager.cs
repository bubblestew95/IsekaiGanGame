using UnityEngine;

public class PlayerAttackManager
{
    private PlayerManager playerManager = null;

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
    }

    public void RayAttack(int _damage, float _maxDistance)
    {
        Transform startTr = playerManager.RangeAttackStartTr;
        Ray ray = new Ray(startTr.position, playerManager.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _maxDistance))
        {
            GameManager.Instance.DamageToBoss(playerManager, _damage);
        }
    }

    public void MeleeAttack(float damage)
    {

    }
}
