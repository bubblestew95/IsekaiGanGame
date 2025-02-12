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

        // ��ų ��� ���� ��ġ��
        public Vector3 skillUsedPosition;

        // ��ų ��� ���� ȸ����
        public Quaternion skillUsedRotation;
    }

    [Serializable]
    public struct PlayerParticleData
    {
        public string particleName;
        public GameObject particlePrefab;
    }
}
