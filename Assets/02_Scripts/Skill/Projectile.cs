using System.Collections;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링을 이용하는 발사체 클래스.
/// </summary>
public class Projectile : PoolableObject<Projectile>
{
    /// <summary>
    /// 발사체의 궤적 컴포넌트.
    /// 발사체가 다시 오브젝트 풀로 돌아가고 다시 나올 때 궤적이 이상하게 표현되는 것을 막기 위해 설정.
    /// TrailRenderer 컴포넌트가 없다면 그냥 null로 두면 됨.
    /// </summary>
    [SerializeField]
    private TrailRenderer trailRenderer = null;

    private PlayerManager owner = null;
    private int damage = 1;
    private float aggro = 1f;
    private bool backAttackEnabled = false;
    private int backAttackTime = 3;

    /// <summary>
    /// 발사체의 정보 초기화 함수.
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
    /// 발사체를 발사시키는 함수
    /// </summary>
    /// <param name="_duration">발사 지속 시간. 이 시간이 지난 후 충돌이 없다면 다시 풀로 돌아감.</param>
    /// <param name="_speed">발사 속도.</param>
    public void ShootProjectile(float _duration, float _speed)
    {
        StartCoroutine(ShootCoroutine(_duration, _speed));
    }

    private void OnTriggerEnter(Collider other)
    {
        // 발사체의 이동 코루틴을 정지시킴.
        // Projectile 레이어는 Boss 레이어만 감지하도록 되어있음.
        // 따라서 뭔가 충돌이 감지되었다면 무조건 Boss임.
        StopAllCoroutines();

        // 만약 이 발사체가 백어택이 가능하고, 실제로 보스의 백어택 부위에 있다면 백어택 데미지 적용.
        if (backAttackEnabled && IsHitBehindBoss(other.transform))
        {
            GameManager.Instance.DamageToBoss(owner, damage * backAttackTime, aggro);
        }
        // 아니면 그냥 데미지 적용.
        else
        {
            GameManager.Instance.DamageToBoss(owner, damage, aggro);
        }

        // 보스와 충돌했으므로 다시 풀로 보낸다.
        ReturnToPool();
    }

    /// <summary>
    /// 발사체를 트랜스폼의 전방 방향으로 발사시키는 코루틴.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShootCoroutine(float _duration, float _speed)
    {
        float currentTime = 0f;

        // 발사 방향은 트랜스폼의 전방.
        Vector3 direction = owner.transform.forward;

        // 발사 지속 시간동안 매 프레임마다 이동함.
        while(currentTime <= _duration)
        {
            transform.position += (direction * _speed * Time.deltaTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        // 지정된 시간이 지난 후에는 다시 오브젝트 풀로 돌아감.
        ReturnToPool();
    }

    /// <summary>
    /// 이 오브젝트가 충돌체의 백어택 위치에 있는지에 대한 여부를 판단함.
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
