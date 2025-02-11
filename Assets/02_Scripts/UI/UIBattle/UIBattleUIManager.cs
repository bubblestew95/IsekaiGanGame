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
    [SerializeField]
    private FloatingJoystick moveJoystick = null;
    [SerializeField]
    private PlayerManager playerManager = null;

    private UIHpsManager ui_PlayerHp = null;
    private UIBossHpsManager ui_BossHp = null;
    private UIWarningManager ui_disconnect = null;

    public FloatingJoystick MoveJoystick
    {
        get { return moveJoystick; }
    }

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

    #region Skill Button, Joysticks

    public void OnSkillJoystickDown(SkillSlot _slot)
    {
        skillUIManager.SetSkillUIEnabled(_slot, true);
    }
    public void OnSkillJoystickUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);

        playerManager.InputManager.OnButtonInput(_slot, pointData);
        skillUIManager.SetSkillUIEnabled(_slot, false);
    }
    public void OnSkillButtonUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);
        pointData.type = SkillPointType.None;
        pointData.skillUsedPosition = GameManager.Instance.GetBossTransform().position;

        playerManager.InputManager.OnButtonInput(_slot, pointData);
    }

    public void SendSkillDirectionToSkillUI(SkillSlot _slot, float _horizontal, float _vertical)
    {
        skillUIManager.SetSkillAimPoint(_slot, _horizontal, _vertical);
    }

    #endregion

    public void SetupAllUI() // 전체적으로 한번 싹 정하고 시작
    {
        ui_BossHp.SetMaxHp(GameManager.Instance.GetBossHp());
        ui_BossHp.HpBarUIUpdate();

        ui_PlayerHp.SetMaxHp(playerManager.StatusManager.MaxHp);
        ui_PlayerHp.HpBarUIUpdate();

        ui_disconnect.ReConnection(); // 연결 오류 UI 비활성화
    }

    public void UpdatePlayerHp()
    {
        ui_PlayerHp.SetCurrentHp(playerManager.StatusManager.CurrentHp);
        ui_PlayerHp.HpBarUIUpdate();
    }

    public void UpdateBossHp()
    {
        ui_BossHp.SetCurrentHp(GameManager.Instance.GetBossHp());
        ui_BossHp.HpBarUIUpdate();
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
