using UnityEngine;

public class BossAttackCollider : MonoBehaviour
{
    private float damage = 0f;
    private string skillName = string.Empty;

    public float Damage 
    { 
        get { return damage; } 
        set { damage = value; } 
    }

    public string SkillName
    {
        get { return skillName; }
        set { skillName = value; }
    }
}
