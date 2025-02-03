using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public int maxHp = 100;

    public float walkSpeed = 3f;

    public List<PlayerSkillBase> skills = null;
}
