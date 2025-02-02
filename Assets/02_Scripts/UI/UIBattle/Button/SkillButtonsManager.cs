using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class SkillButtonsManager : MonoBehaviour
{
    private UIBattleUIManager battleUIManager = null;

    private Dictionary<SkillType, ButtonSetting> skillButtonMap = null;

    public void OnSkillButtonClickDown(SkillType _type)
    {
        if (battleUIManager == null)
        {
            Debug.LogWarning("Battle UI Manager is Null!");
            return;
        }
    }

    public void OnSkillButtonClicked(SkillType _type)
    {
        if(battleUIManager == null)
        {
            Debug.LogWarning("Battle UI Manager is Null!");
            return;
        }

        battleUIManager.OnClickedSkillButton(_type);
    }

    public void ApplyCooltime(SkillType _type, float _time)
    {
        skillButtonMap[_type].SetCooltime(_time);
    }

    private void Awake()
    {
        battleUIManager = GetComponentInParent<UIBattleUIManager>();
        skillButtonMap = new Dictionary<SkillType, ButtonSetting>();

        ButtonSetting[] skillButtons = GetComponentsInChildren<ButtonSetting>();

        foreach(var skillButton in skillButtons)
        {
            skillButtonMap.Add(skillButton.ButtonSkillType, skillButton);
        }
    }
}
