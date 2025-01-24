using UnityEngine;

[CreateAssetMenu(fileName = "BossSkill Data", menuName = "Scriptable Object/BossSkill Data", order = int.MaxValue)]
public class BossSkillData : ScriptableObject
{
    [SerializeField]
    private string skillName;

    public string SkillName { get { return skillName; } }

    [SerializeField]
    private int coolDown;

    public int CoolDown { get { return coolDown; } }


    [SerializeField]
    private int damage;

    public int Damage { get { return damage; } }


}
