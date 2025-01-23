using System.Collections.Generic;
using UnityEngine;

using EnumTypes;

public class PlayerInputManager : MonoBehaviour
{
    /// <summary>
    /// ���� ���̽�ƽ ������Ʈ ����.
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
