using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public const string MOUSE_X_STRING = "Mouse X";
    public const string MOUSE_Y_STRING = "Mouse Y";
    public const string MOUSE_SCROLL_STRING = "Mouse ScrollWheel";

    public static float MouseXInput { get => Input.GetAxis(MOUSE_X_STRING); }
    public static float MouseYInput { get => Input.GetAxis(MOUSE_Y_STRING); }
    public static float MouseScrollInput { get => Input.GetAxis(MOUSE_SCROLL_STRING); }

}
