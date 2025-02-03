using UnityEngine;

using EnumTypes;

public class SkillUI_AOE : SkillUI_Base
{
    [SerializeField]
    private RectTransform targetRectTr = null;
    [SerializeField]
    private GameObject targetImageObj = null;
    [SerializeField]
    private GameObject rangeImageObj = null;

    [SerializeField]
    private float maxRangeRadius = 5f;

    private readonly float defaultY = 0.1f;

    public override void AimSkill(float _horizontal, float _vertical)
    {
        base.AimSkill(_horizontal, _vertical);

        SetTargetPosition(new Vector3(_horizontal, defaultY, _vertical));
    }
    public override void SetEnabled(bool _enabled)
    {
        base.SetEnabled(_enabled);
        targetImageObj.SetActive(enabled);
        rangeImageObj.SetActive(enabled);
    }

    /// <summary>
    /// ��� �������� Ÿ�� ���� ��ų UI�� �ű��.
    /// </summary>
    /// <param name="_pos">�ű���� �ϴ� ��� ����</param>
    private void SetTargetPosition(Vector3 _pos)
    {
        _pos *= maxRangeRadius;
        _pos += transform.position;

        targetRectTr.position = Vector3.ClampMagnitude(_pos, maxRangeRadius);
    }
}
