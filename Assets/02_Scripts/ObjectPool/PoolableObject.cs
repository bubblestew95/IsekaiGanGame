using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolableObject<T> : MonoBehaviour where T : MonoBehaviour
{
    private ObjectPool<T> _pool;

    public void SetPool(ObjectPool<T> pool)
    {
        _pool = pool;
    }

    // 활성화 시 초기화가 필요하면 여기서 구현 (상속받은 클래스에서 오버라이드 가능)
    protected virtual void OnEnable()
    {
    }

    public void ReturnToPool()
    {
        _pool.Release(this as T); // 풀로 반환
    }
}
