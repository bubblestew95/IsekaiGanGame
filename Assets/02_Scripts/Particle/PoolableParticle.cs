using System.Collections;
using UnityEngine;

public class PoolableParticle : PoolableObject<PoolableParticle>
{
    public void Init(Vector3 _spawnPos, Quaternion _spawnRot, float _autoReturnTime)
    {
        transform.position = _spawnPos;
        transform.rotation = _spawnRot;
        StartCoroutine(ReturnToPoolCoroutine(_autoReturnTime));
    }

    /// <summary>
    /// ��� ������Ʈ Ǯ�� ��ƼŬ�� ��ȯ��.
    /// </summary>
    public void InstantReturnToPool()
    {
        StopAllCoroutines();
        ReturnToPool();
    }

    /// <summary>
    /// ������ �ð� �� ������Ʈ Ǯ�� ��ƼŬ�� ��ȯ��.
    /// </summary>
    /// <param name="_returnTime"></param>
    /// <returns></returns>
    private IEnumerator ReturnToPoolCoroutine(float _returnTime)
    {
        yield return new WaitForSeconds(_returnTime);

        ReturnToPool();
    }
}
