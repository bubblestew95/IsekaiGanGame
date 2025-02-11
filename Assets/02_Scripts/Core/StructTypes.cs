using System;
using UnityEngine;

using EnumTypes;

namespace StructTypes
{
    public struct JoystickInputData
    {
        public float x;
        public float z;
    }

    [System.Serializable]
    public struct SkillUIData
    {
        public SkillSlot skillType;
        public SkillUI_Base skillUI;
    }

    public struct InputBufferData
    {
        public SkillSlot skillType;
        public SkillPointData pointData;
    }

    public struct SkillPointData
    {
        public SkillPointType type;

        // 스킬 사용 범위 위치값
        public Vector3 skillUsedPosition;

        // 스킬 사용 방향 회전값
        public Quaternion skillUsedRotation;
    }

    [Serializable]
    public struct PlayerParticleData
    {
        public string particleName;
        public GameObject particlePrefab;
    }
}
