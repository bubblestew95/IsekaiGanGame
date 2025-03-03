using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 오브젝트 풀링 기능을 지원하는 추상 컴포넌트 클래스.
/// 오브젝트 풀링을 이용하려는 오브젝트들은 이 클래스를 상속하는 컴포넌트를 가져야 함.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PoolableObject<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// 이 오브젝트가 담겨질 오브젝트 풀 참조 변수.
    /// </summary>
    private ObjectPool<T> _pool;

    /// <summary>
    /// 이 오브젝트가 담겨질 오브젝트 풀을 지정한다.
    /// </summary>
    /// <param name="pool"></param>
    public void SetPool(ObjectPool<T> pool)
    {
        _pool = pool;
    }

    /// <summary>
    /// 오브젝트 풀로 오브젝트를 반환하는 함수.
    /// 만약 오브젝트 풀을 지정하지 않았다면 경고 로그 출력 후 아무런 행동도 하지 않음.
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
