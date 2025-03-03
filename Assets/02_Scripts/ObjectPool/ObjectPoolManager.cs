using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ������Ʈ Ǯ���� ����ϱ� ���� �Ŵ��� Ŭ����.
/// �ش� Ŭ������ PoolableObject ������Ʈ�� ���� ������Ʈ �ϳ��� ���ؼ� ������Ʈ Ǯ���� �������ش�.
/// </summary>
/// <typeparam name="T">PoolableObject�� ����ϰ� �ִ� Ŭ����</typeparam>
public class ObjectPoolManager<T> where T : PoolableObject<T>
{
    // ����Ƽ���� �����ϴ� ������Ʈ Ǯ.
    private ObjectPool<T> pool;
    private T prefab;
    private Transform parent = null;

    public ObjectPoolManager(T _prefab, int _defaultCapacity = 10, int _maxSize = 20, Transform _parent = null)
    {
        prefab = _prefab;
        parent = _parent;
        // �����ڿ��� �Է¹��� ���� �������� ����Ƽ���� �����ϴ� ������Ʈ Ǯ�� �����.
        pool = new ObjectPool<T>(
            createFunc: CreateObject,
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => Object.Destroy(obj.gameObject),
            collectionCheck: false,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
    }

    /// <summary>
    /// PoolableObject ������Ʈ�� ���� ������Ʈ�� ������.
    /// ���� prefab �� null�� ��� �������� �ʰ� null�� ������.
    /// </summary>
    /// <returns></returns>
    private T CreateObject()
    {
        if(prefab != null)
        {
            T obj = Object.Instantiate(prefab);

            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }

            return obj;
        }
        else
        {
            Debug.LogWarning("Poolable Object is not set!");
            return null;
        }
    }

    /// <summary>
    /// ������Ʈ Ǯ���� ������Ʈ�� �ϳ� �����´�.
    /// ������Ʈ�� �������鼭 �ش� ������Ʈ ���� �ִ� PoolableObject ������Ʈ���� ������Ʈ Ǯ�� �����ϵ��� �����Ѵ�.
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        T obj = pool.Get();
        obj.SetPool(pool);
        return obj;
    }
}
