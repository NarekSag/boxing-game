using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Movement")]
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
        Vector3 moveInputVector = new Vector3(input.MoveAxisRightRaw, 0, input.MoveAxisForwardRaw).normalized;
        Vector3 cameraPlanarDirection = cameraController.CameraPlanarDirection;
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection);

        Debug.DrawLine(transform.position, transform.position + moveInputVector, Color.green);
        moveInputVector = cameraPlanarRotation * moveInputVector;
        Debug.DrawLine(transform.position, transform.position + moveInputVector, Color.red);

        //Move speed
        if(input.sprint.Pressed())
        {
            targetSpeed = moveInputVector != Vector3.zero ? sprintSpeed : 0;
        }
        else 
        {
            targetSpeed = moveInputVector != Vector3.zero ? runSpeed : 0;
        }

        newSpeed = Mathf.Lerp(newSpeed, targetSpeed, Time.deltaTime * moveSharpness);

        newVelocity = moveInputVector * newSpeed;
        transform.Translate(newVelocity * Time.deltaTime, Space.World);

        if(targetSpeed != 0)
        {
            targetRotation = Quaternion.LookRotation(moveInputVector);
            newRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSharpness);
            transform.rotation = newRotation;
        }

        //Animations
        animator.SetFloat("Forward", newSpeed);
    }

    #endregion
}
