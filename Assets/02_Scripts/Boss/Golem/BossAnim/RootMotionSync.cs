using NUnit.Framework;
using UnityEngine;



public class RootMotionSync : MonoBehaviour
{
    [System.Serializable]
    public struct SetAnim
    {
        public string name;
        public float moveMultiplier;
    }

    public Animator animator;
    public SetAnim[] anims;

    private float curMoveMultiplier;

    void OnAnimatorMove()
    {
        // ���� �ִϸ����� ������ �ִϸ��̼� �̸��� Ȯ��
        if (animator.applyRootMotion && CheckAnim())
        {
            // �ִϸ��̼��� �̵� ���� �����ͼ� ���� ����
            Vector3 newPosition = transform.position + animator.deltaPosition * curMoveMultiplier;

            // �̵� ����
            transform.position = newPosition;

            // ȸ���� ����ȭ
            transform.rotation *= animator.deltaRotation;
        }
    }

    // ������ �ִϸ��̼����� check
    private bool CheckAnim()
    {
        foreach (SetAnim anim in anims)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(anim.name))
            {
                curMoveMultiplier = anim.moveMultiplier;
                return true;
            }
        }
        return false;
    }
}
