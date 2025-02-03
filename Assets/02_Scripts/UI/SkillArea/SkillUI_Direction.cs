using UnityEngine;

using EnumTypes;

public class SkillUI_Direction : SkillUI_Base
{
    [SerializeField]
    private RectTransform arrowImagePivot = null;

    public override void AimSkill(float _horizontal, float _vertical)
    {
        base.AimSkill(_horizontal, _vertical);

        SetDirection(_horizontal, _vertical);
    }

    public override void SetEnabled(bool _enabled)
    {
        base.SetEnabled(_enabled);
        arrowImagePivot.gameObject.SetActive(_enabled);
    }

    private void SetDirection(float x, float z)
    {
        float angle = Mathf.Atan2(x, z) * Mathf.Rad2Deg;
        arrowImagePivot.rotation = Quaternion.Euler(0f, angle, 0f);
    }
}
