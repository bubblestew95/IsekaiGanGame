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

    // 스킬 사용후 호출
    public void StartCooldown()
    {
        Debug.Log("스킬 쿨다운 시작");
        isCooldown = true;
        currentCooldownTime = cooldownTime + Time.time;
    }

    // 쿨타임 체크할때 호출 true면 쿨타임 돎, false면 아직 안돎.
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
