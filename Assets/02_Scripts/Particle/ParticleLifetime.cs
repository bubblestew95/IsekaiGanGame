using UnityEngine;

public class ParticleLifetime : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 2f;

    public void DestroyParticle()
    {
        Destroy(gameObject, lifeTime);
    }
}
