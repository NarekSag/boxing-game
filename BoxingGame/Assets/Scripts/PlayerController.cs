using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Movement")]
    [SerializeField]
    private float walkSpeed = 2f;
    [SerializeField]
    private float runSpeed = 6f;
    [SerializeField]
    private float sprintSpeed = 8f;

    [Header("Sharpness")]
    [SerializeField]
    private float rotationSharpness = 10f;
    [SerializeField]
    private float moveSharpness = 10f;

    private Animator animator;
    private PlayerInput input;
    private CameraController cameraController;

    private bool strafing;
    private bool sprinting;
    private float strafeParameter;
    private Vector3 strafeParameterXZ;

    private float targetSpeed;
    private Quaternion targetRotation;

    private float newSpeed;
    private Vector3 newVelocity;
    private Quaternion newRotation;

    #endregion

    #region Methods

    private void Start()
    {
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        cameraController = GetComponent<CameraController>();

        animator.applyRootMotion = false;
    }

    private void Update()
    {
        Vector3 moveInputVector = new Vector3(input.MoveAxisRightRaw, 0, input.MoveAxisForwardRaw);
        Vector3 cameraPlanarDirection = cameraController.CameraPlanarDirection;
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection);

        Vector3 moveInputVectorOriented = cameraPlanarRotation * moveInputVector.normalized;

        if(strafing)
        {
            sprinting = input.Sprint.PressedDown() && (moveInputVector != Vector3.zero);
            strafing = input.Aim.Pressed() && !sprinting;
        }
        else
        {
            sprinting = input.Sprint.Pressed() && (moveInputVector != Vector3.zero);
            strafing = input.Aim.PressedDown();
        }

        //Move speed
        if(sprinting)
        {
            targetSpeed = moveInputVector != Vector3.zero ? sprintSpeed : 0;
        }
        else if(strafing)
        {
            targetSpeed = moveInputVector != Vector3.zero ? walkSpeed : 0;
        }
        else 
        {
            targetSpeed = moveInputVector != Vector3.zero ? runSpeed : 0;
        }
        newSpeed = Mathf.Lerp(newSpeed, targetSpeed, Time.deltaTime * moveSharpness);

        //Velocity
        newVelocity = moveInputVectorOriented * newSpeed;
        transform.Translate(newVelocity * Time.deltaTime, Space.World);

        //Rotation
        if (strafing)
        {
            targetRotation = Quaternion.LookRotation(cameraPlanarDirection);
            newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness);
            transform.rotation = newRotation;
        }
        else if (targetSpeed != 0)
        {
            targetRotation = Quaternion.LookRotation(moveInputVectorOriented);
            newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness);
            transform.rotation = newRotation;
        }

        //Animations
        if(strafing)
        {
            strafeParameter = Mathf.Clamp01(strafeParameter + Time.deltaTime * 4);
            strafeParameterXZ = Vector3.Lerp(strafeParameterXZ, moveInputVector * newSpeed, moveSharpness * Time.deltaTime);
        }
        else
        {
            strafeParameter = Mathf.Clamp01(strafeParameter - Time.deltaTime * 4);
            strafeParameterXZ = Vector3.Lerp(strafeParameterXZ, Vector3.forward * newSpeed, moveSharpness * Time.deltaTime);
        }
        animator.SetFloat("Strafing", strafeParameter);
        animator.SetFloat("StrafingX", Mathf.Round(strafeParameterXZ.x * 100f) / 100f);
        animator.SetFloat("StrafingZ", Mathf.Round(strafeParameterXZ.z * 100f) / 100f);
    }

    #endregion
}
