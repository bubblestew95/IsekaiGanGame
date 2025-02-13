using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BgmController : MonoBehaviour
{

    public List<BgmCharacterTheme> bgmCharacterThemes = new List<BgmCharacterTheme>();
    public int curCharacterIndex = 0;
    public List<BgmBossTheme> bgmBossThemes = new List<BgmBossTheme> ();
    public int curBgmBossIndex = 0;
    private List<BgmExciter> bgmExciters = new List<BgmExciter> ();
    public float excitedLevel = 0;
    public BgmDefeat bgmDefeat = null;
    public BgmVictory bgmVictory = null;
    public AudioSource curPlayingAudio = null;
    public List<AudioSource> audioList = null;
    public float maxVolumeSound = 1;//���� �ø��°� �ִ�ġ
    private bool isFading = false;  // ���� ���� ����
    private void Awake()
    {
        audioList = GetComponentsInChildren<AudioSource>().ToList(); // ��ü ����� ����Ʈ
        foreach (AudioSource audio in audioList) 
        { 
            audio.spatialBlend = 0f; // �Ÿ� ������� ������ �Ҹ��� �鸮�� ����
        }
        bgmCharacterThemes = GetComponentsInChildren<BgmCharacterTheme>().ToList(); // ĳ���� �׸� ����Ʈ
        bgmBossThemes = GetComponentsInChildren<BgmBossTheme>().ToList(); // ���� �׸� ����Ʈ
        bgmExciters = GetComponentsInChildren<BgmExciter>().ToList(); // ���� �׸� ȿ����Ʈ�ѷ� ����Ʈ 
        bgmVictory = GetComponentInChildren<BgmVictory>(); // �¸� Bgm
        bgmDefeat = GetComponentInChildren<BgmDefeat>(); // �й� Bgm
        curBgmBossIndex *= 2; // �������� �г��� �Ϲݸ�� ������ �س����ű� ������ �ε��� *2 �� ���༭ �����ϱ� �����ϰ� ����
    }

    private void Start()
    {
        // ���� ĳ���� �׸� ����� �ҽ� ����
        PlayCharacterBgm();
    }
    private void Update()
    {

#if DEBUG //�����ϸ� ��Ȱ��ȭ�Ǵ� �ڵ�
        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayCharacterBgm();
        }// �÷��̾� �׸�
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayBossBgm();
        }// ���� �׸�
        if(Input.GetKeyDown(KeyCode.M))
        {
            PlayBossRageBgm();
        }// ���� �׸� +1
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayDefeat();
        }// �й� Bgm
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayVictory();
        }// �¸� Bgm
#endif
    }
    #region ���
    private void PlayCharacterBgm()      //ĳ���� �׸� ���
    {
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� �ƿ�
        }
        //ĳ���� �׸� ���
        curPlayingAudio = bgmCharacterThemes[curCharacterIndex].GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� ��
        // ĳ���� �׸��� ���� �� ���� �׸� �ڵ� ���
        StartCoroutine(PlayBossBgmAfterDelay(curPlayingAudio.clip.length));

    }
    private IEnumerator PlayBossBgmAfterDelay(float delay)
    {
        // ĳ���� �׸��� ���̸�ŭ ���
        yield return new WaitForSeconds(delay);

        // ĳ���� �׸��� �����ٸ� ���� �׸� ���
        PlayBossBgm();
    }
    private void PlayBossBgm()      //���� �׸� ���
    {
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� �ƿ�
        }
        //���� �׸� ���
        curPlayingAudio = bgmBossThemes[curBgmBossIndex].GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� ��
    }
    public void PlayBossRageBgm()        //���� �׸� �ε���+1 ���
    {
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� �ƿ�
        }
        //���� �׸� �ε���+1 ���
        curPlayingAudio = bgmBossThemes[curBgmBossIndex+1].GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� ��
    }

    public void PlayVictory()
    {
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� �ƿ�
        }
        curPlayingAudio = bgmVictory.GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� ��
    } // �¸� bgm ���
    public void PlayDefeat()
    {
        // �̹� ���̵� ���̶�� ���� ó��
        if (isFading)
        {
            return; // ���� ���̵尡 ���� ���̶�� �Լ��� �����ϰ� �ٽ� �������� ����
        }
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            StartCoroutine(FadeOutAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� �ƿ�
        }
        curPlayingAudio = bgmDefeat.GetComponent<AudioSource>();
        curPlayingAudio.Play();
        StartCoroutine(FadeInAudio(curPlayingAudio, 1f)); // 1�� ���� ���̵� ��
    } // �й� bgm ���
    #endregion
    #region ������Ʈ��
    public void VolumeControl(float _level)
    {
        if(_level < 0 || _level > 100)
        {
            Debug.Log("Volume Range is 0 ~ 100");
            return;
        }
        float volume = _level/100;
        foreach (AudioSource audioSource in audioList)
        {
            audioSource.volume = volume * maxVolumeSound;
        }
    } //  1~100 ���� �Ǽ��� �޾Ƽ� ��������

    private IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        isFading = true; // ���̵� ����
        float startVolume = audioSource.volume;

        // ����� ������ ������ �ٿ��� 0���� ����
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        // ������ 0���� �����ϰ� ������� ����
        audioSource.Stop();
        audioSource.volume = startVolume; // ������ ����� �� ������ ������� �ǵ���
        isFading = false; // ���̵� �Ϸ�
    }
    private IEnumerator FadeInAudio(AudioSource audioSource, float duration)
    {
        isFading = true; // ���̵� ����
        audioSource.volume = 0; // ó���� ������ 0���� ����

        // ������ ������ ������Ŵ
        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime / duration;
            yield return null;
        }

        // ������ ������ max�� ����
        audioSource.volume = maxVolumeSound;
        isFading = false; // ���̵� �Ϸ�
    }
    #endregion
    #region excited Level Controll ���� ��е��� ���� bgm ���� �ο��н� ���� ��ȭ, ���� �ǰ� ���� �� �� �����ϰ� ������ּ���
    public void ExcitedLevel(float _level)
    {
        if(_level < 0 || _level > 1)
        {
            Debug.Log("excitedLevel Range is 0 ~ 1");
            return;
        }
        excitedLevel = _level;
        foreach(BgmExciter bgmExciter in bgmExciters)
        {
            bgmExciter.excitedLevel = excitedLevel;
        }
    }
    #endregion
}

