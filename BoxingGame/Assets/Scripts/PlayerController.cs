using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerStance { Standing, Crouching, Proning}
public class PlayerController : MonoBehaviour
{
    #region Const
    private const string STAND_TO_CROUCH = "Base Layer.Base Crouching";
    private const string STAND_TO_PRONE = "Base Layer.Stand To Prone";
    private const string CROUCH_TO_STAND = "Base Layer.Base Standing";
    private const string CROUCH_TO_PRONE = "Base Layer.Crouch To Prone";
    private const string PRONE_TO_STAND = "Base Layer.Prone To Stand";
    private const string PRONE_TO_CROUCH = "Base Layer.Prone To Crouch";
    #endregion

    #region Variables

    [Header("Speed (Normal, Sprinting)")]
    [SerializeField]
    public Vector2 standingSpeed = Vector2.zero;
    [SerializeField]
    public Vector2 crouchingSpeed = Vector2.zero;
    [SerializeField]
    public Vector2 proningSpeed = Vector2.zero;

    [Header("Capsule (Radius, Height, YOffset)")]
    [SerializeField]
    private Vector3 standingCapsule = Vector3.zero;
    [SerializeField]
    private Vector3 crouchingCapsule = Vector3.zero;
    [SerializeField]
    private Vector3 proningCapsule = Vector3.zero;

    [Header("Sharpness")]
    [SerializeField]
    private float standingRotationSharpness = 10f;
    [SerializeField]
    private float crouchingRotationSharpness = 10f;
    [SerializeField]
    private float proningRotationSharpness = 10f;
    [SerializeField]
    private float moveSharpness = 10f;

    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private PlayerInput input;
    private CameraController cameraController;

    private float runSpeed;
    private float sprintSpeed;
    private float rotationSharpness;
    private LayerMask layerMask;
    private PlayerStance stance;
    private Collider[] obstructions = new Collider[8];

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
        capsuleCollider = GetComponent<CapsuleCollider>();
        input = GetComponent<PlayerInput>();
        cameraController = GetComponent<CameraController>();

        //Set default values
        runSpeed = standingSpeed.x;
        sprintSpeed = standingSpeed.y;
        rotationSharpness = standingRotationSharpness;
        stance = PlayerStance.Standing;
        SetCapsuleDimensions(standingCapsule);

        int mask = 0;
        for(int i = 0; i < 32; i++) //Go through 32 possible layers (Max Count)
        {
            if(!(Physics.GetIgnoreLayerCollision(gameObject.layer,i)))
            {
                mask |= 1 << i;
            }
        }
        layerMask = mask;

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
        if(input.Sprint.Pressed())
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

    private void LateUpdate()
    {
        switch(stance)
        {
            case PlayerStance.Standing:
                if(input.Crouching.PressedDown())
                {
                    RequestStanceChange(PlayerStance.Crouching);
                }
                else if(input.Proning.PressedDown())
                {
                    RequestStanceChange(PlayerStance.Proning);
                }
                break;
            case PlayerStance.Crouching:
                if (input.Crouching.PressedDown())
                {
                    RequestStanceChange(PlayerStance.Standing);
                }
                else if (input.Proning.PressedDown())
                {
                    RequestStanceChange(PlayerStance.Proning);
                }
                break;
            case PlayerStance.Proning:
                if (input.Crouching.PressedDown())
                {
                    RequestStanceChange(PlayerStance.Crouching);
                }
                else if (input.Proning.PressedDown())
                {
                    RequestStanceChange(PlayerStance.Standing);
                }
                break;
        }
    }

    public bool RequestStanceChange(PlayerStance newStance)
    {
        if (stance == newStance)
        {
            return true;
        }

        switch(stance)
        {
            case PlayerStance.Standing:
                if(newStance == PlayerStance.Crouching)
                {
                    if(!CharacterOverlap(crouchingCapsule))
                    {
                        runSpeed = crouchingSpeed.x;
                        sprintSpeed = crouchingSpeed.y;
                        rotationSharpness = crouchingRotationSharpness;
                        stance = newStance;
                        animator.CrossFadeInFixedTime(STAND_TO_CROUCH, 0.5f);
                        SetCapsuleDimensions(crouchingCapsule);
                        return true;
                    }
                }
                else if (newStance == PlayerStance.Proning)
                {
                    if (!CharacterOverlap(proningCapsule))
                    {
                        runSpeed = proningSpeed.x;
                        sprintSpeed = proningSpeed.y;
                        rotationSharpness = proningRotationSharpness;
                        stance = newStance;
                        animator.CrossFadeInFixedTime(STAND_TO_PRONE, 0.25f);
                        SetCapsuleDimensions(proningCapsule);
                        return true;
                    }
                }
                break;
            case PlayerStance.Crouching:
                if (newStance == PlayerStance.Standing)
                {
                    if (!CharacterOverlap(standingCapsule))
                    {
                        runSpeed = standingSpeed.x;
                        sprintSpeed = standingSpeed.y;
                        rotationSharpness = standingRotationSharpness;
                        stance = newStance;
                        animator.CrossFadeInFixedTime(CROUCH_TO_STAND, 0.5f);
                        SetCapsuleDimensions(standingCapsule);
                        return true;
                    }
                }
                else if (newStance == PlayerStance.Proning)
                {
                    if (!CharacterOverlap(proningCapsule))
                    {
                        runSpeed = proningSpeed.x;
                        sprintSpeed = proningSpeed.y;
                        rotationSharpness = proningRotationSharpness;
                        stance = newStance;
                        animator.CrossFadeInFixedTime(CROUCH_TO_PRONE, 0.25f);
                        SetCapsuleDimensions(proningCapsule);
                        return true;
                    }
                }
                break;
            case PlayerStance.Proning:
                if (newStance == PlayerStance.Standing)
                {
                    if (!CharacterOverlap(standingCapsule))
                    {
                        runSpeed = standingSpeed.x;
                        sprintSpeed = standingSpeed.y;
                        rotationSharpness = standingRotationSharpness;
                        stance = newStance;
                        animator.CrossFadeInFixedTime(PRONE_TO_STAND, 0.25f);
                        SetCapsuleDimensions(standingCapsule);
                        return true;
                    }
                }
                else if (newStance == PlayerStance.Crouching)
                {
                    if (!CharacterOverlap(crouchingCapsule))
                    {
                        runSpeed = crouchingSpeed.x;
                        sprintSpeed = crouchingSpeed.y;
                        rotationSharpness = crouchingRotationSharpness;
                        stance = newStance;
                        animator.CrossFadeInFixedTime(PRONE_TO_CROUCH, 0.25f);
                        SetCapsuleDimensions(crouchingCapsule);
                        return true;
                    }
                }
                break;
        }
        return false;
    }

    private bool CharacterOverlap(Vector3 dimensions)
    {
        float radius = dimensions.x;
        float height = dimensions.y;
        Vector3 center = new Vector3(capsuleCollider.center.x, dimensions.z, capsuleCollider.center.z);

        Vector3 point0;
        Vector3 point1;
        if(height < radius * 2)
        {
            point0 = transform.position + center;
            point1 = transform.position + center;
        }
        else
        {
            point0 = transform.position + center + (transform.up * (height * 0.5f - radius));
            point1 = transform.position + center - (transform.up * (height * 0.5f - radius));
        }

        int numOverlaps = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, obstructions, layerMask);
        for(int i = 0; i < numOverlaps; i++)
        {
            if(obstructions[i] == capsuleCollider)
            {
                numOverlaps--;
            }
        }

        return numOverlaps > 0;
    }

    private void SetCapsuleDimensions(Vector3 dimensions)
    {
        capsuleCollider.center = new Vector3(capsuleCollider.center.x, dimensions.z, capsuleCollider.center.z);
        capsuleCollider.radius = dimensions.x;
        capsuleCollider.height = dimensions.y;
    }

    #endregion
}
