using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EnumTypes;

public class UIBattleUIManager : MonoBehaviour
{
    public PlayerManager playerManager = null;
    public List<UIHpsManager> hpList = new List<UIHpsManager>();
    public List<UIBossHpsManager> bossHpList = new List<UIBossHpsManager>();
    public List<UIWarningManager> warningList = new List<UIWarningManager>();
    public List <ButtonSetting> buttonList = new List<ButtonSetting>();
    public FloatingJoystick joyStick = null;
    public List<float> cooltimeList = new List<float>(); //평타,회피,스킬1,스킬2,스킬3 순서

    public int bossMaxHp = 1;
    public int playerMaxHp = 1;

    private SkillButtonsManager skillButtonsManager = null;

    private void Awake()
    {
        hpList = GetComponentsInChildren<UIHpsManager>().ToList();
        bossHpList = GetComponentsInChildren<UIBossHpsManager>().ToList();
        warningList = GetComponentsInChildren<UIWarningManager>().ToList();
        buttonList = GetComponentsInChildren<ButtonSetting>().ToList();
        joyStick = GetComponentInChildren<FloatingJoystick>();

        //테스트 용 쿨타임
        if (cooltimeList.Count <= 4)
        {
            List<float> testCoolValue = new List<float> { 1, 1, 1, 1, 1 };
            cooltimeList = testCoolValue;
            if (cooltimeList.Count == 0)
                Debug.Log("쿨타임 리스트가 비어있어 테스트 쿨타임으로 시작합니다.");
            if (cooltimeList.Count < 0)
                Debug.Log("쿨타임 리스트가 5개보다 적어 테스트 쿨타임으로 시작합니다.");
        }
        //
        SetupAllUI();

        skillButtonsManager = GetComponentInChildren<SkillButtonsManager>();
    }

    /// <summary>
    /// 입력받은 스킬 버튼을 플레이어 매니저에게 전달한다.
    /// </summary>
    /// <param name="_type">입력받은 스킬 버튼의 스킬 타입</param>
    public void OnClickedSkillButton(SkillType _type)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("Player Manager is Null!");
            return;
        }

        playerManager.OnButtonInput(_type);
    }

    public void CooltimeListSetting(List<float> _timeList) // 평타,회피,스킬1,스킬2,스킬3 순서
    {
        for (int i = 0; i < _timeList.Count; i++)
        {
            cooltimeList[i] = _timeList[i];
        }
    }

    public void SetupAllUI() // 전체적으로 한번 싹 정하고 시작
    {
        foreach (UIBossHpsManager hp in bossHpList) // 보스 최대체력 설정
        {
            hp.SetMaxHp(bossMaxHp);
            hp.HpBarUIUpdate();
        }        
        foreach (UIHpsManager hp in hpList) // 플레이어 최대체력 설정
        {
            hp.SetMaxHp(playerMaxHp);
            hp.HpBarUIUpdate();
        }
        warningList[0].ReConnection(); // 연결 오류 UI 비활성화

        for (int i = 0; i < buttonList.Count; i++) // 쿨타임 리스트에 있는 쿨타임들 각각 버튼에 설정// 평타,회피,스킬1,스킬2,스킬3 순서
        {
            ButtonSetting buttonSetting = buttonList[i];
            buttonSetting.SetCooltime(cooltimeList[i]);
        }
    }
}
