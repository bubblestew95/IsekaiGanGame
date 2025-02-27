using System.Collections;
using UnityEngine;

/// <summary>
/// ������Ʈ Ǯ���� �̿��ϴ� �߻�ü Ŭ����.
/// </summary>
public class Projectile : PoolableObject<Projectile>
{
    /// <summary>
    /// �߻�ü�� ���� ������Ʈ.
    /// �߻�ü�� �ٽ� ������Ʈ Ǯ�� ���ư��� �ٽ� ���� �� ������ �̻��ϰ� ǥ���Ǵ� ���� ���� ���� ����.
    /// TrailRenderer ������Ʈ�� ���ٸ� �׳� null�� �θ� ��.
    /// </summary>
    [SerializeField]
    private TrailRenderer trailRenderer = null;

    private PlayerManager owner = null;
    private int damage = 1;
    private float aggro = 1f;
    private bool backAttackEnabled = false;
    private int backAttackTime = 3;

    /// <summary>
    /// �߻�ü�� ���� �ʱ�ȭ �Լ�.
    /// </summary>
    /// <param name="_player"></param>
    /// <param name="_damage"></param>
    /// <param name="_aggro"></param>
    /// <param name="_backAttackEnabled"></param>
    /// <param name="_backAttackTime"></param>
    public void Init(PlayerManager _player, int _damage, float _aggro, 
        bool _backAttackEnabled = false, int _backAttackTime = 3)
    {
        transform.position = _player.RangeAttackStartTr.position;
        transform.rotation = _player.transform.rotation;

        owner = _player;
        damage = _damage;
        aggro = _aggro;
        backAttackEnabled = _backAttackEnabled;
        backAttackTime = _backAttackTime;

        if(trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    /// <summary>
    /// �߻�ü�� �߻��Ű�� �Լ�
    /// </summary>
    /// <param name="_duration">�߻� ���� �ð�. �� �ð��� ���� �� �浹�� ���ٸ� �ٽ� Ǯ�� ���ư�.</param>
    /// <param name="_speed">�߻� �ӵ�.</param>
    public void ShootProjectile(float _duration, float _speed)
    {
        StartCoroutine(ShootCoroutine(_duration, _speed));
    }

    private void OnTriggerEnter(Collider other)
    {
        // �߻�ü�� �̵� �ڷ�ƾ�� ������Ŵ.
        // Projectile ���̾�� Boss ���̾ �����ϵ��� �Ǿ�����.
        // ���� ���� �浹�� �����Ǿ��ٸ� ������ Boss��.
        StopAllCoroutines();

        // ���� �� �߻�ü�� ������� �����ϰ�, ������ ������ ����� ������ �ִٸ� ����� ������ ����.
        if (backAttackEnabled && IsHitBehindBoss(other.transform))
        {
            GameManager.Instance.DamageToBoss(owner, damage * backAttackTime, aggro);
        }
        // �ƴϸ� �׳� ������ ����.
        else
        {
            GameManager.Instance.DamageToBoss(owner, damage, aggro);
        }

        // ������ �浹�����Ƿ� �ٽ� Ǯ�� ������.
        ReturnToPool();
    }

    /// <summary>
    /// �߻�ü�� Ʈ�������� ���� �������� �߻��Ű�� �ڷ�ƾ.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShootCoroutine(float _duration, float _speed)
    {
        float currentTime = 0f;

        // �߻� ������ Ʈ�������� ����.
        Vector3 direction = owner.transform.forward;

        // �߻� ���� �ð����� �� �����Ӹ��� �̵���.
        while(currentTime <= _duration)
        {
            transform.position += (direction * _speed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        // ������ �ð��� ���� �Ŀ��� �ٽ� ������Ʈ Ǯ�� ���ư�.
        ReturnToPool();
    }

    /// <summary>
    /// �� ������Ʈ�� �浹ü�� ����� ��ġ�� �ִ����� ���� ���θ� �Ǵ���.
    /// </summary>
    /// <param name="_targetTr"></param>
    /// <returns></returns>
    private bool IsHitBehindBoss(Transform _targetTr)
    {
        if (Vector3.Angle(
            transform.forward,
            _targetTr.forward) < 80f)
        {
            return true;
        }

        return false;
    }
}
