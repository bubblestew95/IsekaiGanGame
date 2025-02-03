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
    // public List<float> cooltimeList = new List<float>(); //평타,회피,스킬1,스킬2,스킬3 순서

    public int bossMaxHp = 1;
    public int playerMaxHp = 1;

    private SkillButtonsManager skillButtonsManager = null;
    private SkillUIManager skillUIManager = null;

    private void Awake()
    {
        hpList = GetComponentsInChildren<UIHpsManager>().ToList();
        bossHpList = GetComponentsInChildren<UIBossHpsManager>().ToList();
        warningList = GetComponentsInChildren<UIWarningManager>().ToList();
        buttonList = GetComponentsInChildren<ButtonSetting>().ToList();
        joyStick = GetComponentInChildren<FloatingJoystick>();

        SetupAllUI();

        skillButtonsManager = GetComponentInChildren<SkillButtonsManager>();
        skillUIManager = transform.parent.GetComponentInChildren<SkillUIManager>();
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

        // playerManager.OnButtonInput(_type);
    }
    public void OnSkillButtonDown(SkillType _type)
    {
        skillUIManager.SetSkillUIEnabled(_type, true);
    }
    public void OnSkillButtonUp(SkillType _type)
    {
        Vector3 point = skillUIManager.GetSkillAimPoint(_type);

        // playerManager.OnButtonInput(_type);
        playerManager.TryUseSkill(_type, point);
        skillUIManager.SetSkillUIEnabled(_type, false);
    }
    public void OnSkillButtonExit(SkillType _type)
    {

    }

    public void SendSkillDirectionToSkillUI(SkillType _type, float _horizontal, float _vertical)
    {
        skillUIManager.SetSkillAimPoint(_type, _horizontal, _vertical);
    }

    //public void CooltimeListSetting(List<float> _timeList) // 평타,회피,스킬1,스킬2,스킬3 순서
    //{
    //    for (int i = 0; i < _timeList.Count; i++)
    //    {
    //        cooltimeList[i] = _timeList[i];
    //    }
    //}

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
        }
    }

    /// <summary>
    /// 지정한 타입의 스킬 버튼에 쿨타임 UI를 적용한다.
    /// </summary>
    /// <param name="_type">지정할 스킬 타입</param>
    /// <param name="_time">쿨타임 시간</param>
    public void ApplyCooltime(SkillType _type, float _time)
    {
        skillButtonsManager.ApplyCooltime(_type, _time);
    }
}
