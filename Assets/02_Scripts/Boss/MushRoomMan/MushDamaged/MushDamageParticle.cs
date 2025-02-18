using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class MushDamageParticle : MonoBehaviour
{
    public ParticleSystem P_Particle;
    public ParticleSystem P_ParticleMine;
    public MushStateManager mushStateManager;
    public ParticleSystem[] digitParticles;
    public List<Sprite> numberSprites;
    public float damage;

    private Dictionary<int, Sprite> spriteDict;
    private ParticleSystemRenderer[] particleRenderers;

    void Awake()
    {
        // ���ں� ��������Ʈ ����
        spriteDict = new Dictionary<int, Sprite>();
        for (int i = 0; i < numberSprites.Count; i++)
        {
            spriteDict[i] = numberSprites[i];
        }
    }

    // �������� �´� ��ƼŬ ����
    public void SetupAndPlayParticles(float _damage)
    {
        // ��ƼŬ �ý��� ����
        ParticleSystem particle = Instantiate(P_Particle, new Vector3(mushStateManager.Boss.transform.position.x, mushStateManager.Boss.transform.position.y + 1f, mushStateManager.Boss.transform.position.z), Quaternion.identity);

        // ��ƼŬ �ý��� �ڽĵ� ������.
        digitParticles = particle.GetComponentsInChildren<ParticleSystem>().Where(ps => ps != particle).ToArray();

        // �� ��ƼŬ �ý����� Renderer ����
        particleRenderers = new ParticleSystemRenderer[digitParticles.Length];
        for (int i = 0; i < digitParticles.Length; i++)
        {
            particleRenderers[i] = digitParticles[i].GetComponent<ParticleSystemRenderer>();
        }

        // �������� �°� ��������Ʈ ����
        SetDamage(_damage);

        // ��ƼŬ ���
        particle.Play();

        Destroy(particle.gameObject, particle.main.duration + particle.main.startLifetime.constantMax);
    }

    // �������� �´� ��ƼŬ ���� (��)
    public void SetupAndPlayParticlesMine(float _damage)
    {
        // ��ƼŬ �ý��� ����
        ParticleSystem particle = Instantiate(P_ParticleMine, new Vector3(mushStateManager.Boss.transform.position.x, mushStateManager.Boss.transform.position.y + 1f, mushStateManager.Boss.transform.position.z), Quaternion.identity);

        // ��ƼŬ �ý��� �ڽĵ� ������.
        digitParticles = particle.GetComponentsInChildren<ParticleSystem>().Where(ps => ps != particle).ToArray();

        // �� ��ƼŬ �ý����� Renderer ����
        particleRenderers = new ParticleSystemRenderer[digitParticles.Length];
        for (int i = 0; i < digitParticles.Length; i++)
        {
            particleRenderers[i] = digitParticles[i].GetComponent<ParticleSystemRenderer>();
        }

        // �������� �°� ��������Ʈ ����
        SetDamage(_damage);

        // ��ƼŬ ���
        particle.Play();

        Destroy(particle.gameObject, particle.main.duration + particle.main.startLifetime.constantMax);
    }

    // �������� �°� ��������Ʈ ����
    private void SetDamage(float damage)
    {
        string damageStr = damage.ToString();
        int length = damageStr.Length;

        // �� �ڸ����� �°� ��ƼŬ �ý��� ����
        for (int i = 0; i < digitParticles.Length; i++)
        {
            if (i < length)
            {
                int digit = int.Parse(damageStr[length - 1 - i].ToString());
                var textureSheet = digitParticles[i].textureSheetAnimation;
                textureSheet.SetSprite(0, spriteDict[digit]);

                particleRenderers[i].enabled = true;
            }
            else
            {
                particleRenderers[i].enabled = false;
            }
        }
    }

}
