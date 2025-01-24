using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill Data", menuName = "Scriptable Object/BossSkill Data", order = int.MaxValue)]
public class BossSkillData : ScriptableObject
{
    [SerializeField]
    private string skillName;

    public string SkillName { get { return skillName; } }

    [SerializeField]
    private float coolDown;

    public float CoolDown { get { return coolDown; } }


    [SerializeField]
    private float damage;

    public float Damage { get { return damage; } }

    [SerializeField]
    private float minRange;

    public float MinRange { get { return minRange; } }


    [SerializeField]
    private float maxRange;

    public float MaxRange { get { return maxRange; } }


}
