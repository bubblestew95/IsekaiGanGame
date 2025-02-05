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

    private GameObject bossObject = null;

    #endregion


    #region Public Functions
    public Transform GetBossTransform()
    {
        return bossObject.transform;
    }

    /// <summary>
    /// �÷��̾ �������� �������� ��׷� ��ġ�� ����.
    /// </summary>
    /// <param name="_damageSource">�������� �������� �ִ� �÷��̾�</param>
    public void DamageToBoss(PlayerManager _damageSource, int _damage, float _aggro)
    {
        Debug.LogFormat("Player deal to boss! damage : {0}, aggro : {1}", _damage, _aggro);
    }

    /// <summary>
    /// ������ �÷��̾�� �������� ����.
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
        bossObject = FindAnyObjectByType<BossBT>().gameObject;
    }

    #endregion
}
