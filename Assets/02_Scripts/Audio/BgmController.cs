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
        audioList = GetComponentsInChildren<AudioSource>().ToList(); // 전체 오디오 리스트
        foreach (AudioSource audio in audioList) 
        { 
            audio.spatialBlend = 0f; // 거리 상관없이 일정한 소리로 들리게 해줌
        }
        bgmCharacterThemes = GetComponentsInChildren<BgmCharacterTheme>().ToList(); // 캐릭터 테마 리스트
        bgmBossThemes = GetComponentsInChildren<BgmBossTheme>().ToList(); // 보스 테마 리스트
        bgmExciters = GetComponentsInChildren<BgmExciter>().ToList(); // 보스 테마 효과컨트롤러 리스트 
        bgmVictory = GetComponentInChildren<BgmVictory>(); // 승리 Bgm
        bgmDefeat = GetComponentInChildren<BgmDefeat>(); // 패배 Bgm
        curBgmBossIndex *= 2; // 보스마다 분노모드 일반모드 구분을 해놓을거기 때문에 인덱스 *2 를 해줘서 관리하기 용이하게 만듬
    }

    private void Start()
    {
        // 현재 캐릭터 테마 오디오 소스 시작
        PlayCharacterBgm();
    }
    private void Update()
    {

#if DEBUG //빌드하면 비활성화되는 코드
        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayCharacterBgm();
        }// 플레이어 테마
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayBossBgm();
        }// 보스 테마
        if(Input.GetKeyDown(KeyCode.M))
        {
            PlayBossRageBgm();
        }// 보스 테마 +1
        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayDefeat();
        }// 패배 Bgm
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayVictory();
        }// 승리 Bgm
#endif
    }
    #region 재생
    private void PlayCharacterBgm()      //캐릭터 테마 재생
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        //캐릭터 테마 재생
        curPlayingAudio = bgmCharacterThemes[curCharacterIndex].GetComponent<AudioSource>();
        curPlayingAudio.Play();
    }
    private void PlayBossBgm()      //보스 테마 재생
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        //보스 테마 재생
        curPlayingAudio = bgmBossThemes[curBgmBossIndex].GetComponent<AudioSource>();
        curPlayingAudio.Play();
    }
    private void PlayBossRageBgm()        //보스 테마 인덱스+1 재생
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        //보스 테마 인덱스+1 재생
        curPlayingAudio = bgmBossThemes[curBgmBossIndex+1].GetComponent<AudioSource>();
        curPlayingAudio.Play();
    }

    private void PlayVictory()
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        curPlayingAudio = bgmVictory.GetComponent<AudioSource>();
        curPlayingAudio.Play();
    } // 승리 bgm 재생
    private void PlayDefeat()
    {
        // 현재 오디오를 멈추고
        if (curPlayingAudio)
        {
            curPlayingAudio.Stop();
        }
        curPlayingAudio = bgmDefeat.GetComponent<AudioSource>();
        curPlayingAudio.Play();
    } // 패배 bgm 재생
    #endregion
    #region 볼륨컨트롤
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
    } //  1~100 까지 실수를 받아서 볼륨제어
    #endregion
    #region excited Level Controll 보스 흥분도에 따른 bgm 음정 로우패스 필터 변화, 보스 피가 적을 수 록 증가하게 만들어주세요
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

