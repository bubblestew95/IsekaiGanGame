using EnumTypes;
using System;
using UnityEngine;

namespace StructTypes
{
    public struct JoystickInputData
    {
        public float x;
        public float z;
    }

    [Serializable]
    public struct SkillUIData
    {
        public SkillType skillType;
        public SkillUI_Base skillUI;
    }
}
