using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ������Ʈ Ǯ�� ����� �����ϴ� �߻� ������Ʈ Ŭ����.
/// ������Ʈ Ǯ���� �̿��Ϸ��� ������Ʈ���� �� Ŭ������ ����ϴ� ������Ʈ�� ������ ��.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PoolableObject<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// �� ������Ʈ�� ����� ������Ʈ Ǯ ���� ����.
    /// </summary>
    private ObjectPool<T> _pool;

    /// <summary>
    /// �� ������Ʈ�� ����� ������Ʈ Ǯ�� �����Ѵ�.
    /// </summary>
    /// <param name="pool"></param>
    public void SetPool(ObjectPool<T> pool)
    {
        _pool = pool;
    }

    /// <summary>
    /// ������Ʈ Ǯ�� ������Ʈ�� ��ȯ�ϴ� �Լ�.
    /// ���� ������Ʈ Ǯ�� �������� �ʾҴٸ� ��� �α� ��� �� �ƹ��� �ൿ�� ���� ����.
    /// </summary>
    public void ReturnToPool()
    {
        if(_pool != null)
        {
            _pool.Release(this as T);
        }
        else
        {
            Debug.LogWarning("Object Pool reference is not set!");
        }
    }
}
