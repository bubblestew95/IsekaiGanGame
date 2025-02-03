using UnityEngine;

using EnumTypes;

public class SkillUI_Direction : SkillUI_Base
{
    [SerializeField]
    private RectTransform arrowImagePivot = null;

    public override void AimSkill(Vector3 _aim)
    {
        SetDirection(_aim);
    }

    public override void SetEnabled(bool _enabled)
    {
        base.SetEnabled(_enabled);
        arrowImagePivot.gameObject.SetActive(_enabled);
    }

    private void SetDirection(Vector3 _lookPos)
    {
        arrowImagePivot.LookAt(_lookPos);
    }
}
