using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    public ParticleSystem attack1;
    public ParticleSystem attack2;
    public ParticleSystem attack3;
    public ParticleSystem attack4;
    public ParticleSystem attack5;
    public ParticleSystem attack6;
    public ParticleSystem attack7;
    public ParticleSystem attack8;
    public ParticleSystem attack9;

    public ParticleSystem phase2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayParticle(ParticleSystem particle, Vector3 position)
    {
        if (particle != null)
        {
            ParticleSystem newParticle = Instantiate(particle, position, Quaternion.identity);
            newParticle.Play();
            Destroy(newParticle.gameObject, newParticle.main.duration + newParticle.main.startLifetime.constantMax); // ��ƼŬ �ڵ� ����
        }
    }
}

