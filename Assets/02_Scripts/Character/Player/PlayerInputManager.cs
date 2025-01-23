using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class PlayerInputManager : MonoBehaviour
{
    /// <summary>
    /// 가상 조이스틱 컴포넌트 지정.
    /// </summary>
    [SerializeField]
    private FloatingJoystick joystick = null;

    [SerializeField]
    private float dequeueTime = 0.05f;

    private Queue<InputButtonType> inputButtonBuffer = new Queue<InputButtonType>();

    private float beforeDequeueTime = 0f;

    #region Public Func
    
    public void OnButtonInput(InputButtonType _input)
    {
        inputButtonBuffer.Enqueue(_input);
    }

    #endregion

    #region Private Func
    #endregion

    #region Unity Callback

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    #endregion
}
