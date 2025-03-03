using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EnumTypes;
using StructTypes;
using UnityEngine.UI;
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

    private PlayerHitImpact ui_PlayerHitImpact = null;
    private UIHpsManager ui_PlayerHp = null;
    private UIBossHpsManager ui_BossHp = null;
    private UIWarningManager ui_disconnect = null;

    public List<UI_GameResultManager> ui_GameResultManager = new List<UI_GameResultManager>();
    public VolumeControl ui_VolumeControl = null;
    public List<Button> ui_VolumeControlButtons = new List<Button>();
    public FloatingJoystick MoveJoystick
    {
        get { return moveJoystick; }
    }

    private void Awake()
    {
        ui_PlayerHitImpact = GetComponentInChildren<PlayerHitImpact>();
        ui_PlayerHp = GetComponentInChildren<UIHpsManager>();
        ui_BossHp = GetComponentInChildren<UIBossHpsManager>();
        ui_disconnect = GetComponentInChildren<UIWarningManager>();
        ui_GameResultManager = GetComponentsInChildren<UI_GameResultManager>(true).ToList();
        ui_VolumeControl = GetComponentInChildren<VolumeControl>();
        ui_VolumeControlButtons = ui_VolumeControl.GetComponentsInChildren<Button>(true).ToList();
    }

    private void Start()
    {
        SetupAllUI();

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        if (moveJoystick != null)
        {
            moveJoystick.gameObject.SetActive(false);
        }

        if (skillButtonsManager != null)
        {
            skillButtonsManager.DisableButtonInteractable();
        }

#endif
    }

    public void FadeInResult(bool _isWin)
    {
        foreach (UI_GameResultManager result in ui_GameResultManager)
        {
            result.StartFadeIn(_isWin);
        }
    }

    public void ShowDamagedUI()
    {
        ui_PlayerHitImpact.PlayerDamagedImpactOn();
    }

    #region Skill Button, Joysticks

    public void OnSkillJoystickDown(SkillSlot _slot)
    {
        skillUIManager.SetSkillUIEnabled(_slot, true);
    }
    public void OnSkillJoystickUp(SkillSlot _slot)
    {
        if (playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Skill
            && _slot != SkillSlot.Dash)
        {
            skillUIManager.SetSkillUIEnabled(_slot, false);
            return;
        }

        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);

        playerManager.InputManager.OnButtonInput(_slot, pointData);
        skillUIManager.SetSkillUIEnabled(_slot, false);
    }
    public void OnSkillButtonUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);
        pointData.type = SkillPointType.None;
        pointData.skillUsedPosition = GameManager.Instance.GetBossTransform().position;

        Vector3 direction = GameManager.Instance.GetBossTransform().position - playerManager.transform.position;
        direction.y = 0f;
        direction.Normalize();
        pointData.skillUsedRotation = Quaternion.LookRotation(direction);

        playerManager.InputManager.OnButtonInput(_slot, pointData);
    }

    public void SendSkillDirectionToSkillUI(SkillSlot _slot, float _horizontal, float _vertical)
    {
        skillUIManager.SetSkillAimPoint(_slot, _horizontal, _vertical);
    }

    public void SetSkillButtonEnabled(SkillSlot _slot, bool _enabled)
    {
        skillButtonsManager.SetSkillButtonEnabled(_slot, _enabled);
    }

    #endregion

    public void SetupAllUI() // ��ü������ �ѹ� �� ���ϰ� ����
    {
        ui_BossHp.SetMaxHp(1);
        ui_BossHp.HpBarUIUpdate();

        ui_PlayerHp.SetMaxHp(playerManager.StatusManager.MaxHp);
        ui_PlayerHp.HpBarUIUpdate();
        foreach (UI_GameResultManager result in ui_GameResultManager)
        {
            result.enabled = true;
        }
        ui_disconnect.ReConnection(); // ���� ���� UI ��Ȱ��ȭ
    }

    public void UpdatePlayerHp()
    {
        if(playerManager.StatusManager != null && ui_PlayerHp != null)
        {
            ui_PlayerHp.SetCurrentHp(playerManager.StatusManager.CurrentHp);
            ui_PlayerHp.HpBarUIUpdate();
        }
    }

    /// <summary>
    /// ������ Ÿ���� ��ų ��ư�� ��Ÿ�� UI�� �����Ѵ�.
    /// </summary>
    /// <param name="_slot">������ ��ų Ÿ��</param>
    /// <param name="_time">��Ÿ�� �ð�</param>
    public void ApplyCooltime(SkillSlot _slot, float _time)
    {
        skillButtonsManager.ApplyCooltime(_slot, _time);
        ui_VolumeControlButtons[1].onClick.Invoke();  // ��ư Ŭ�� �̺�Ʈ Ʈ����
    }
}
