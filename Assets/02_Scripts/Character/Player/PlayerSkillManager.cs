using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    private List<PlayerSkillData> skillDatas = null;

    private Animator animator = null;

    private float[] coolTimeArr = null;

    #region Public Functions

    /// <summary>
    /// �÷��̾� ��ų �Ŵ����� �ʱ�ȭ�Ѵ�.
    /// </summary>
    /// <param name="_skilldataList">��ų ������ ����Ʈ</param>
    public void Init()
    {
        skillDatas = GetComponent<PlayerManager>().PlayerData.skills;

        coolTimeArr = new float[skillDatas.Count];
        for (int i = 0; i < coolTimeArr.Length; ++i)
            coolTimeArr[i] = 0f;
    }

    /// <summary>
    /// ��ų ����� �õ��Ѵ�.
    /// </summary>
    /// <param name="_skillIdx">����� ��ų�� ��ų ����Ʈ �� �ε���</param>
    public void TryUseSkill(int _skillIdx)
    {
        if(skillDatas == null || coolTimeArr == null)
        {
            Debug.LogWarning("Skill List is not valid!");
            return;
        }

        // ��ų �ε����� ��ų ����Ʈ�� ũ�⺸�� �� ũ�ٸ� ���� ��� �� ����.
        if (skillDatas.Count < _skillIdx)
        {
            Debug.LogWarningFormat("Index {0} Skill that trying to use is not valid index!", _skillIdx);
            return;
        }

        // ��ų�� ���� ��� �������� üũ��.
        if (!IsSkillUsable(_skillIdx))
        {
            Debug.Log("Index {0} Skill is coolTime!");
            return;
        }

        Debug.Log("Use Skill!");

        animator.SetTrigger("Skill01");
    }

    public void TestRaycast()
    {
        Debug.Log("Raycast!");
        Debug.DrawLine(transform.position, transform.forward + transform.position, Color.red, 2f);

        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 30f))
        {
            Debug.Log(hit.transform.name);

            // ����� üũ.
            // ĳ������ ���� �������� ����� �����Ը� �����س���. ���ʿ� ����� üũ ������� ���� ĳ���� �������� ���ϰ�
            // ���� ĳ������ forward �� �浹ü�� forward �� ������ ����� ���θ� üũ�ϴ� ��.
            if (Vector3.Angle(transform.forward, hit.transform.forward) < 80f)
                Debug.Log("BackAttack!");
        }
    }

    #endregion

    #region Private Functions

    /// <summary>
    /// ���� ������ ��ų �ε����� ��ų�� ��� ������ �� üũ�Ѵ�.
    /// </summary>
    /// <param name="_skillIdx">üũ�� ��ų�� �ε���</param>
    /// <returns></returns>
    private bool IsSkillUsable(int _skillIdx)
    {
        // ��Ÿ�� üũ
        if (coolTimeArr[_skillIdx] > 0f)
        {
            Debug.Log("Index {0} Skill is coolTime!");
            return false;
        }

        return true;
    }

    #endregion

    #region Unity Callback
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    #endregion
}
