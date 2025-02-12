using UnityEngine;
using System.Collections;

public class BossAudioManager : MonoBehaviour
{
    public static BossAudioManager Instance;

    public AudioClip Attack1;
    public AudioClip Attack2;
    public AudioClip Attack3;
    public AudioClip Attack4;
    public AudioClip Attack5;
    public AudioClip Attack6;
    public AudioClip Attack7;
    public AudioClip Attack8;
    public AudioClip Attack9;
    public AudioClip SpecialAttack;

    private AudioSource audioSource;

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

    private void Start()
    {
        audioSource = FindAnyObjectByType<BossStateManager>().GetComponent<AudioSource>();

        audioSource.loop = false; // �ݺ� ��� ����
        audioSource.playOnAwake = false; // �ڵ� ��� ��Ȱ��ȭ (�������� ����)
        audioSource.spatialBlend = 1f; // 3D ���� (0 = 2D, 1 = 3D)
        audioSource.rolloffMode = AudioRolloffMode.Linear; // �Ҹ��� �Ÿ� ��� ����
        audioSource.minDistance = 15f; // �� �Ÿ������� ������ ������
        audioSource.maxDistance = 40f; // �� �Ÿ� �̻󿡼��� �Ҹ��� ���� �� �鸲
    }

    // ����� ��� �Լ�
    public void AudioPlay(AudioClip _audioClip, bool isLoop = false, float fadeInTime = 0.5f)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = _audioClip;
        audioSource.loop = isLoop;
        audioSource.Play();

        //if (fadeInTime > 0)
        //{
        //    StartCoroutine(FadeIn(audioSource, fadeInTime));
        //}
    }

    // ����� ���� �Լ�
    public void AudioStop(float fadeOutTime = 0.5f)
    {
        audioSource.Stop();

        //if (fadeOutTime > 0)
        //{
        //    StartCoroutine(FadeOut(audioSource, fadeOutTime));
        //}
        //else
        //{
        //    audioSource.Stop();
        //}
    }

    // ���̵��� ȿ�� (���� �Ҹ� Ŀ��)
    private IEnumerator FadeIn(AudioSource source, float duration)
    {
        float startVolume = 0f;
        source.volume = startVolume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 1f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        source.volume = 1f;
    }

    // ���̵�ƿ� ȿ�� (���� �Ҹ� �۾���)
    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }
}
