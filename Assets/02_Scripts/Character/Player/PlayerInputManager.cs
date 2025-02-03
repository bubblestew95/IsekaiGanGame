using UnityEngine;

using StructTypes;

public class PlayerInputManager
{
    /// <summary>
    /// 가상 조이스틱 컴포넌트 지정.
    /// </summary>
    private FloatingJoystick joystick = null;

    #region Public Func
    
    public void Init(FloatingJoystick _joystick)
    {
        joystick = _joystick;
    }

    public void GetJoystickInputValue(out JoystickInputData _inputData)
    {
        _inputData.x = joystick.Horizontal;
        _inputData.z = joystick.Vertical;
    }

    #endregion

    #region Private Func
    #endregion
}
