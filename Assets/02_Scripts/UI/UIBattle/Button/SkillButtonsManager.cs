using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class SkillButtonsManager : MonoBehaviour
{
    private UIBattleUIManager battleUIManager = null;

    private Dictionary<SkillSlot, ButtonSetting> skillButtonMap = null;

    public void OnSkillButtonClickDown(SkillSlot _type)
    {
        if (battleUIManager == null)
        {
            Debug.LogWarning("Battle UI Manager is Null!");
            return;
        }

        battleUIManager.OnSkillButtonDown(_type);
    }

    public void OnSkillButtonClickUp(SkillSlot _type)
    {
        if (battleUIManager == null)
        {
            Debug.LogWarning("Battle UI Manager is Null!");
            return;
        }

        battleUIManager.OnSkillButtonUp(_type);
    }

    public void ApplyCooltime(SkillSlot _type, float _time)
    {
        skillButtonMap[_type].SetCooltime(_time);
    }

    public void SendSkillDirection(SkillSlot _type, float _horizontal, float _vertical)
    {
        if (battleUIManager == null)
        {
            Debug.LogWarning("Battle UI Manager is Null!");
            return;
        }

        battleUIManager.SendSkillDirectionToSkillUI(_type, _horizontal, _vertical);
    }

    private void Awake()
    {
        battleUIManager = GetComponentInParent<UIBattleUIManager>();
        skillButtonMap = new Dictionary<SkillSlot, ButtonSetting>();

        ButtonSetting[] skillButtons = GetComponentsInChildren<ButtonSetting>();

        foreach(var skillButton in skillButtons)
        {
            skillButtonMap.Add(skillButton.ButtonSkillType, skillButton);
        }
    }
}
