using UnityEngine;

public class BossSkillCooldown : MonoBehaviour
{
    public BossSkillData bossSkillData;
    private float cooldownTime;
    private bool isCooldown = false;
    private float currentCooldownTime;

    private void Start()
    {
        cooldownTime = bossSkillData.CoolDown;
    }

    // ��ų ����� ȣ��
    public void StartCooldown()
    {
        Debug.Log("��ų ��ٿ� ����");
        isCooldown = true;
        currentCooldownTime = cooldownTime + Time.time;
    }

    // ��Ÿ�� üũ�Ҷ� ȣ�� true�� ��Ÿ�� ��, false�� ���� �ȵ�.
    public bool IsCooldownOver()
    {
        if (isCooldown)
        {
            if (currentCooldownTime - Time.time <= 0)
            {
                isCooldown = false;
                currentCooldownTime = 0;
            }
        }
        return !isCooldown;
    }
}
