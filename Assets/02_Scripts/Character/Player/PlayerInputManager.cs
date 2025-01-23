using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class PlayerInputManager
{
    /// <summary>
    /// 가상 조이스틱 컴포넌트 지정.
    /// </summary>
    private FloatingJoystick joystick = null;

    private float dequeueTime = 0.05f;

    private Queue<SkillType> inputButtonBuffer = new Queue<SkillType>();

    private float beforeDequeueTime = 0f;

    #region Public Func
    
    public void OnButtonInput(SkillType _input)
    {
        inputButtonBuffer.Enqueue(_input);
    }

    #endregion

    #region Private Func
    #endregion
}
