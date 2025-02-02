using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 범위 및 방향 지정 UI 들을 관리하기 위한 매니저 클래스.
/// </summary>
public class SkillUIManager : MonoBehaviour
{
    [SerializeField]
    private List<SkillUI_Base> skillUIList = null;

    public Transform debugTr = null;

    public void SetSkillUIPosition()
    {

    }
}
