using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IHitResponder
{
    private const string animJab = "Base Layer.Boxing Jab Left";

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

    [Header("Attacking")]
    [SerializeField]
    private int damage = 10;
    [SerializeField]
    private HitBox hitBoxLeftFist;

    private Animator animator;
    private PlayerInput input;
    private CameraController cameraController;
    private SMBEventCurrator eventCurrator;

    private bool strafing;
    private bool sprinting;
    private float strafeParameter;
    private Vector3 strafeParameterXZ;

    private float targetSpeed;
    private Quaternion targetRotation;

    private float newSpeed;
    private Vector3 newVelocity;
    private Quaternion newRotation;

    private bool isJabbing;
    private bool inAnimation;
    private Vector3 animatorVelocity;
    private Quaternion animatorDeltaRotation;
    private List<GameObject> objectHit = new List<GameObject>();

    int IHitResponder.Damage {get => damage;}

    #endregion

    #region Methods

    private void Start()
    {
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        cameraController = GetComponent<CameraController>();
        eventCurrator = GetComponent<SMBEventCurrator>();

        hitBoxLeftFist.HitResponder = this;
        eventCurrator.EventString.AddListener(OnSMBEvent);
    }

    private void Update()
    {
        Vector3 moveInputVector = new Vector3(input.MoveAxisRightRaw, 0, input.MoveAxisForwardRaw);
        Vector3 cameraPlanarDirection = cameraController.CameraPlanarDirection;
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection);

        Vector3 moveInputVectorOriented = cameraPlanarRotation * moveInputVector.normalized;

        strafing = cameraController.LockedOn;
        if(strafing)
        {
            sprinting = input.Sprint.PressedDown() && (moveInputVector != Vector3.zero);
        }
        else
        {
            sprinting = input.Sprint.Pressed() && (moveInputVector != Vector3.zero);
        }
        if(sprinting)
        {
            cameraController.ToggleLock(false);
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
        if(inAnimation)
        {
            newVelocity = animator.velocity;
        }
        else
        {
            newVelocity = moveInputVectorOriented * newSpeed;
        }
        transform.Translate(newVelocity * Time.deltaTime, Space.World);

        //Rotation
        if (inAnimation)
        {
            transform.rotation *= animatorDeltaRotation;
        }
        else if (strafing)
        {
            Vector3 toTarget = cameraController.Target.TargetTransform.position - transform.position;
            Vector3 planarToTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);

            targetRotation = Quaternion.LookRotation(planarToTarget);
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

        //Lock On
        if(input.LockOn.PressedDown())
        {
            cameraController.ToggleLock(!cameraController.LockedOn);
        }

        if(!inAnimation)
        {
            if(input.Attack.PressedDown())
            {
                inAnimation = true;
                animator.CrossFadeInFixedTime(animJab, 0.1f, 0, 0);
            }
        }

        if(isJabbing)
        {
            hitBoxLeftFist.CheckHit();
        }
    }

    private void OnAnimatorMove()
    {
        if(inAnimation)
        {
            animatorVelocity = animator.velocity;
            animatorDeltaRotation = animator.deltaRotation;
        }    
    }

    public void OnSMBEvent(string eventName)
    {
        switch(eventName)
        {
            case "JabStart":
                objectHit.Clear();
                isJabbing = true;
                Debug.Log("JabStarted");
                break;
            case "JabEnd":
                isJabbing = false;
                break;
            case "AnimationEnd":
                inAnimation = false;
                break;
        }
    }

    bool IHitResponder.CheckHit(HitData data)
    {
        if (data.hurtbox.Owner == gameObject)
        {
            return false;
        }
        else if(objectHit.Contains(data.hurtbox.Owner))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    void IHitResponder.Response(HitData data)
    {
        objectHit.Add(data.hurtbox.Owner);
    }

    #endregion
}
