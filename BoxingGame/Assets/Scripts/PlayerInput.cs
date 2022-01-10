using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Input
{
    public KeyCode primary;
    public KeyCode alternate;

    public bool Pressed()
    {
        return UnityEngine.Input.GetKey(primary) || UnityEngine.Input.GetKey(alternate);
    }

    public bool PressedDown()
    {
        return UnityEngine.Input.GetKeyDown(primary) || UnityEngine.Input.GetKeyDown(alternate);
    }

    public bool PressedUp()
    {
        return UnityEngine.Input.GetKeyUp(primary) || UnityEngine.Input.GetKeyUp(alternate);
    }
}

public class PlayerInput : MonoBehaviour
{
    public Input Forward;
    public Input Backward;
    public Input Left;
    public Input Right;
    public Input Sprint;
    public Input Crouching;
    public Input Proning;

    public int MoveAxisForwardRaw
    {
        get
        {
            if (Forward.Pressed() && Backward.Pressed())
            {
                return 0;
            }
            else if (Forward.Pressed())
            {
                return 1;
            }
            else if (Backward.Pressed())
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public int MoveAxisRightRaw
    {
        get
        {
            if (Right.Pressed() &&Left.Pressed())
            {
                return 0;
            }
            else if (Right.Pressed())
            {
                return 1;
            }
            else if (Left.Pressed())
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public const string MOUSE_X_STRING = "Mouse X";
    public const string MOUSE_Y_STRING = "Mouse Y";
    public const string MOUSE_SCROLL_STRING = "Mouse ScrollWheel";

    public static float MouseXInput { get => UnityEngine.Input.GetAxis(MOUSE_X_STRING); }
    public static float MouseYInput { get => UnityEngine.Input.GetAxis(MOUSE_Y_STRING); }
    public static float MouseScrollInput { get => UnityEngine.Input.GetAxis(MOUSE_SCROLL_STRING); }

}
