using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private GameObject onHitParticlePrefab = null;

    private PlayerManager owner = null;
    private int damage = 1;
    private float aggro = 1f;

    public void Init(PlayerManager _player, int _damage, float _aggro)
    {
        owner = _player;
        damage = _damage;
        aggro = _aggro;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(onHitParticlePrefab != null)
        {
            Instantiate(onHitParticlePrefab, transform.position, Quaternion.identity);
        }

        GameManager.Instance.DamageToBoss(owner, damage, aggro);

        Destroy(gameObject);
    }
}
