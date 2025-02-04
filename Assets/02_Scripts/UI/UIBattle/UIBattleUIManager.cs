using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EnumTypes;
using StructTypes;

public class UIBattleUIManager : MonoBehaviour
{
    [SerializeField]
    private SkillUIManager skillUIManager = null;
    [SerializeField]
    private SkillButtonsManager skillButtonsManager = null;

    public PlayerManager playerManager = null;
    public List<UIHpsManager> hpList = new List<UIHpsManager>();
    public List<UIBossHpsManager> bossHpList = new List<UIBossHpsManager>();
    public List<UIWarningManager> warningList = new List<UIWarningManager>();
    public List <ButtonSetting> buttonList = new List<ButtonSetting>();



    public int bossMaxHp = 1;
    public int playerMaxHp = 1;


    private void Awake()
    {
        hpList = GetComponentsInChildren<UIHpsManager>().ToList();
        bossHpList = GetComponentsInChildren<UIBossHpsManager>().ToList();
        warningList = GetComponentsInChildren<UIWarningManager>().ToList();
        buttonList = GetComponentsInChildren<ButtonSetting>().ToList();

        SetupAllUI();

        // skillButtonsManager = GetComponentInChildren<SkillButtonsManager>();
    }

    /// <summary>
    /// 입력받은 스킬 버튼을 플레이어 매니저에게 전달한다.
    /// </summary>
    /// <param name="_slot">입력받은 스킬 버튼의 스킬 타입</param>
    public void OnClickedSkillButton(SkillSlot _slot)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("Player Manager is Null!");
            return;
        }

        // playerManager.OnButtonInput(_type);
    }
    public void OnSkillButtonDown(SkillSlot _slot)
    {
        skillUIManager.SetSkillUIEnabled(_slot, true);
    }
    public void OnSkillButtonUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);

        playerManager.OnButtonInput(_slot, pointData);
        // playerManager.TryUseSkill(_type);
        skillUIManager.SetSkillUIEnabled(_slot, false);
    }

    public void SendSkillDirectionToSkillUI(SkillSlot _slot, float _horizontal, float _vertical)
    {
        skillUIManager.SetSkillAimPoint(_slot, _horizontal, _vertical);
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
    /// <param name="_slot">지정할 스킬 타입</param>
    /// <param name="_time">쿨타임 시간</param>
    public void ApplyCooltime(SkillSlot _slot, float _time)
    {
        skillButtonsManager.ApplyCooltime(_slot, _time);
    }
}
