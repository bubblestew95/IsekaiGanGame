using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public static GameManager Instance
    {
        get { return instance; }
    }

    private GameObject bossObject = null;

    #region Public Functions
    public Transform GetBossTransform()
    {
        return bossObject.transform;
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
