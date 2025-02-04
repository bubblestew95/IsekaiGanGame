using UnityEngine;

using EnumTypes;
using StructTypes;

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
        SetTargetPosition(new Vector3(_horizontal, defaultY, _vertical));
    }

    public override SkillPointData GetSkillAimPoint()
    {
        SkillPointData pointData = new SkillPointData();
        pointData.type = SkillPointType.Area;
        pointData.point = targetRectTr.position;
        return pointData;
    }

    public override void SetEnabled(bool _enabled)
    {
        targetImageObj.SetActive(_enabled);
        rangeImageObj.SetActive(_enabled);
    }

    /// <summary>
    /// ��� �������� Ÿ�� ���� ��ų UI�� �ű��.
    /// </summary>
    /// <param name="_pos">�ű���� �ϴ� ��� ����</param>
    private void SetTargetPosition(Vector3 _pos)
    {
        _pos *= maxRangeRadius;
        _pos += transform.position;

        targetRectTr.position = _pos;
    }

    private void Start()
    {
        rangeImageObj.GetComponent<RectTransform>().sizeDelta = new Vector2(maxRangeRadius, maxRangeRadius);
    }
}
