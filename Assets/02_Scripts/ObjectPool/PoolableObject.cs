using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolableObject<T> : MonoBehaviour where T : MonoBehaviour
{
    private ObjectPool<T> _pool;


    public void SetPool(ObjectPool<T> pool)
    {
        _pool = pool;
    }

    
    protected virtual void OnEnable()
    {
    }

    /// <summary>
    /// ������Ʈ Ǯ�� ������Ʈ�� ��ȯ�ϴ� �Լ�.
    /// </summary>
    public void ReturnToPool()
    {
        _pool.Release(this as T);
    }
}
