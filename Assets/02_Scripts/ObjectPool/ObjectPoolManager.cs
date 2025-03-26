using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 오브젝트 풀링을 사용하기 위한 매니저 클래스.
/// 해당 클래스는 PoolableObject 컴포넌트를 가진 오브젝트 하나에 대해서 오브젝트 풀링을 지원해준다.
/// </summary>
/// <typeparam name="T">PoolableObject를 상속하고 있는 클래스</typeparam>
public class ObjectPoolManager<T> where T : PoolableObject<T>
{
    // 유니티에서 지원하는 오브젝트 풀.
    private ObjectPool<T> pool;
    private T prefab;
    private Transform parent = null;

    public ObjectPoolManager(T _prefab, int _defaultCapacity = 10, int _maxSize = 20, Transform _parent = null)
    {
        prefab = _prefab;
        parent = _parent;
        // 생성자에서 입력받은 값을 바탕으로 유니티에서 지원하는 오브젝트 풀을 만든다.
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
    /// PoolableObject 컴포넌트를 가진 오브젝트를 생성함.
    /// 만약 prefab 이 null일 경우 생성하지 않고 null을 리턴함.
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
    /// 오브젝트 풀에서 오브젝트를 하나 꺼내온다.
    /// 오브젝트를 꺼내오면서 해당 오브젝트 내에 있는 PoolableObject 컴포넌트에서 오브젝트 풀을 참조하도록 지정한다.
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        T obj = pool.Get();
        obj.SetPool(pool);
        return obj;
    }
}
