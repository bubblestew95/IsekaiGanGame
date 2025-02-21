using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolableObject<T> : MonoBehaviour where T : MonoBehaviour
{
    private ObjectPool<T> _pool;

    public void SetPool(ObjectPool<T> pool)
    {
        _pool = pool;
    }

    // Ȱ��ȭ �� �ʱ�ȭ�� �ʿ��ϸ� ���⼭ ���� (��ӹ��� Ŭ�������� �������̵� ����)
    protected virtual void OnEnable()
    {
    }

    public void ReturnToPool()
    {
        _pool.Release(this as T); // Ǯ�� ��ȯ
    }
}
