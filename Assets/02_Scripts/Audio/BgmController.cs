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
            curPlayingAudio.Stop();
        }
        //ĳ���� �׸� ���
        curPlayingAudio = bgmCharacterThemes[curCharacterIndex].GetComponent<AudioSource>();
        curPlayingAudio.Play();
    }
    private void PlayBossBgm()      //���� �׸� ���
    {
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        //���� �׸� ���
        curPlayingAudio = bgmBossThemes[curBgmBossIndex].GetComponent<AudioSource>();
        curPlayingAudio.Play();
    }
    private void PlayBossRageBgm()        //���� �׸� �ε���+1 ���
    {
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        //���� �׸� �ε���+1 ���
        curPlayingAudio = bgmBossThemes[curBgmBossIndex+1].GetComponent<AudioSource>();
        curPlayingAudio.Play();
    }

    private void PlayVictory()
    {
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        curPlayingAudio = bgmVictory.GetComponent<AudioSource>();
        curPlayingAudio.Play();
    } // �¸� bgm ���
    private void PlayDefeat()
    {
        // ���� ������� ���߰�
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        curPlayingAudio = bgmDefeat.GetComponent<AudioSource>();
        curPlayingAudio.Play();
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
            audioSource.volume = volume;
        }
    } //  1~100 ���� �Ǽ��� �޾Ƽ� ��������
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

