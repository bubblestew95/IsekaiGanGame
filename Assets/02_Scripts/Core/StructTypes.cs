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
        public Vector3 point;
    }
}
