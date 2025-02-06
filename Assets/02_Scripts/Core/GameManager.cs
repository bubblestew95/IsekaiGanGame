using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton Variable, Properties
    private static GameManager instance = null;

    public static GameManager Instance
    {
        get { return instance; }
    }
    #endregion

    #region Inspector Variables
    #endregion

    #region Private Variables

    private BossStateManager bossStateManager = null;

    #endregion


    #region Public Functions
    public Transform GetBossTransform()
    {
        return bossStateManager.transform;
    }

    public int GetBossHp()
    {
        return -1;
    }

    /// <summary>
    /// 플레이어가 보스에게 데미지와 어그로 수치를 더함.
    /// </summary>
    /// <param name="_damageSource">보스에게 데미지를 주는 플레이어</param>
    public void DamageToBoss(PlayerManager _damageSource, int _damage, float _aggro)
    {
        Debug.LogFormat("Player deal to boss! damage : {0}, aggro : {1}", _damage, _aggro);
    }

    /// <summary>
    /// 보스가 플레이어에게 데미지를 가함.
    /// </summary>
    /// <param name="_damageReceiver"></param>
    public void DamageToPlayer(PlayerManager _damageReceiver, int _damage)
    {
        _damageReceiver.TakeDamage(_damage);
    }

    #endregion

    #region Private Functions
    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        bossStateManager = FindAnyObjectByType<BossStateManager>();
    }

    #endregion
}
