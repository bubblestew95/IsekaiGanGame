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
    private UIHpsManager ui_PlayerHp = null;
    private UIBossHpsManager ui_BossHp = null;
    private UIWarningManager ui_disconnect = null;

    public int bossMaxHp = 1;
    public int playerMaxHp = 1;


    private void Awake()
    {
        ui_PlayerHp = GetComponentInChildren<UIHpsManager>();
        ui_BossHp = GetComponentInChildren<UIBossHpsManager>();
        ui_disconnect = GetComponentInChildren<UIWarningManager>();
    }

    private void Start()
    {
        SetupAllUI();
    }

    /// <summary>
    /// 입력받은 스킬 버튼을 플레이어 매니저에게 전달한다.
    /// </summary>
    /// <param name="_slot">입력받은 스킬 버튼의 스킬 타입</param>
    //public void OnClickedSkillButton(SkillSlot _slot)
    //{
    //    if (playerManager == null)
    //    {
    //        Debug.LogWarning("Player Manager is Null!");
    //        return;
    //    }

    //    // playerManager.OnButtonInput(_type);
    //}
    public void OnSkillJoystickDown(SkillSlot _slot)
    {
        skillUIManager.SetSkillUIEnabled(_slot, true);
    }
    public void OnSkillJoystickUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);

        playerManager.OnButtonInput(_slot, pointData);
        skillUIManager.SetSkillUIEnabled(_slot, false);
    }
    public void OnSkillButtonUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);
        pointData.type = SkillPointType.None;
        pointData.point = GameManager.Instance.GetBossTransform().position;

        playerManager.OnButtonInput(_slot, pointData);
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
        ui_BossHp.SetMaxHp(GameManager.Instance.GetBossHp());
        ui_BossHp.HpBarUIUpdate();

        ui_PlayerHp.SetMaxHp(playerManager.StatusManager.MaxHp);
        ui_PlayerHp.HpBarUIUpdate();

        ui_disconnect.ReConnection(); // 연결 오류 UI 비활성화
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
