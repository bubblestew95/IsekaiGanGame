using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private GameObject onHitParticlePrefab = null;

    private PlayerManager owner = null;
    private int damage = 1;
    private float aggro = 1f;
    private float duration = 3f;
    private float speed = 2f;

    public void Init(PlayerManager _player, int _damage, float _aggro)
    {
        owner = _player;
        damage = _damage;
        aggro = _aggro;
    }

    /// <summary>
    /// 발사체를 발사시키는 함수
    /// </summary>
    /// <param name="_duration"></param>
    /// <param name="_speed"></param>
    public void ShootProjectile(float _duration, float _speed)
    {
        duration = _duration;
        speed = _speed;
        StartCoroutine(ShootCoroutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        StopAllCoroutines();

        if(onHitParticlePrefab != null)
        {
            Instantiate(onHitParticlePrefab, transform.position, Quaternion.identity);
        }

        if(other.GetComponent<BossStateManager>() != null)
        {
            GameManager.Instance.DamageToBoss(owner, damage, aggro);
        }

        Destroy(gameObject);
    }

    private IEnumerator ShootCoroutine()
    {
        float currentTime = 0f;

        Vector3 direction = transform.forward;

        while(currentTime <= duration)
        {
            transform.position += (direction * speed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
