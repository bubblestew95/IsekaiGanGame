using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ų ���� �� ���� ���� UI ���� �����ϱ� ���� �Ŵ��� Ŭ����.
/// </summary>
public class SkillUIManager : MonoBehaviour
{
    [SerializeField]
    private List<SkillUI_Base> skillUIList = null;

    public Transform debugTr = null;

    private void Update()
    {
        ((SkillUI_AOE)skillUIList[0]).SetTargetPosition(debugTr.position);
    }
}
