using UnityEngine;

public class PlayerParticleController : MonoBehaviour
{
    public void SpawnParticle(GameObject _particlePrefab, Vector3 _spawnPos)
    {
        Instantiate(_particlePrefab, _spawnPos, Quaternion.identity);
    }
}
