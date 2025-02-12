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

        audioSource.loop = false; // 반복 재생 여부
        audioSource.playOnAwake = false; // 자동 재생 비활성화 (수동으로 제어)
        audioSource.spatialBlend = 1f; // 3D 사운드 (0 = 2D, 1 = 3D)
        audioSource.rolloffMode = AudioRolloffMode.Linear; // 소리가 거리 비례 감소
        audioSource.minDistance = 15f; // 이 거리까지는 볼륨이 유지됨
        audioSource.maxDistance = 40f; // 이 거리 이상에서는 소리가 거의 안 들림
    }

    // 오디오 재생 함수
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

    // 오디오 정지 함수
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

    // 페이드인 효과 (점점 소리 커짐)
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

    // 페이드아웃 효과 (점점 소리 작아짐)
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
