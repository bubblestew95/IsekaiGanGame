using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

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
    public AudioClip Phase2;
    public AudioMixer audioMixer; // ���� �߰� Ȳ�¿� 02.14 ����� �ͼ�

    private AudioSource audioSource;
    private Coroutine curCoroutine;

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

        AudioMixerGroup[] mixerGroups = audioMixer.FindMatchingGroups("SFX"); // ���� �߰� Ȳ�¿� 02.14 ����� �ͼ� SFX �׷� ã��
        audioSource.outputAudioMixerGroup = mixerGroups[0]; // ���� �߰� Ȳ�¿� 02.14 ����� �ͼ� out put ����
    }

    // ����� ��� �Լ�
    public void AudioPlay(AudioClip _audioClip)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = _audioClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    // ����� ��� �Լ� ������ ����
    public void AudioPlay(AudioClip _audioClip, float _delay)
    {
        curCoroutine = StartCoroutine(AudioPlayDelay(_audioClip, _delay));
    }

    // ���� Ŀ���ٰ� �۾����� ����� ��� �Լ�
    public void AudioPlayFadeInAndOut(AudioClip _audioClip, float _duration, float _fadeIn, float _fadeOut)
    {
        curCoroutine = StartCoroutine(FadeInAndOut(_audioClip, _duration, _fadeIn, _fadeOut));
    }

    // ����� ���� �Լ�
    public void AudioStop(bool isStop = true, float fadeOutTime = 0.5f)
    {
        if (isStop)
        {
            audioSource.Stop();
        }
        else
        {
            StartCoroutine(FadeOut(fadeOutTime));
        }
    }

    // ����� �ڷ�ƾ ���� �Լ�
    public void StopAudioCoroutine()
    {
        if (curCoroutine != null)
        {
            StopCoroutine(curCoroutine);
            curCoroutine = null;
            audioSource.volume = 1f;
        }
    }

    // ���̵��� ȿ�� (���� �Ҹ� Ŀ��)
    private IEnumerator FadeIn(AudioClip _audioClip, float duration)
    {
        float startVolume = 0f;
        audioSource.volume = startVolume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 1f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 1f;
    }

    // ���̵�ƿ� ȿ�� (���� �Ҹ� �۾���)
    private IEnumerator FadeOut(float duration)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    // ���̵��� + ���̵�ƿ�
    private IEnumerator FadeInAndOut(AudioClip _audioClip, float _duration, float fadeInDuration, float fadeOutDuration)
    {
        // ���̵��� ����
        float startVolume = 0f;
        audioSource.volume = startVolume;
        float elapsedTime = 0f;

        audioSource.clip = _audioClip;
        audioSource.loop = false;
        audioSource.Play();

        while (elapsedTime < fadeInDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 1f, elapsedTime / fadeInDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 1f;

        yield return new WaitForSeconds(_duration);

        startVolume = audioSource.volume;
        elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();

        audioSource.volume = 1f;


    }

    private IEnumerator AudioPlayDelay(AudioClip _audioClip, float _delay)
    {
        yield return new WaitForSeconds(_delay);

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = _audioClip;
        audioSource.loop = false;
        audioSource.Play();
    }
}
