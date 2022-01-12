using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Variables
    [Header("Framing")]
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Transform followTransform;
    [SerializeField]
    private Vector3 framingNormal = Vector3.zero;

    [Header("Distance")]
    [SerializeField]
    private float zoomSpeed = 5f;
    [SerializeField]
    private float minDistance = 0f;
    [SerializeField]
    private float maxDistance = 10f;
    [SerializeField]
    private float defaultDistance = 5f;

    [Header("Rotation")]
    [SerializeField]
    private bool invertX = false;
    [SerializeField]
    private bool invertY = false;
    [SerializeField]
    private float rotationSharpness = 50f;
    [SerializeField]
    private float defaultVerticalAngle = 20f;
    [SerializeField]
    [Range(-90, 90)] private float minVerticalAngle = -90;
    [SerializeField]
    [Range(-90, 90)] private float maxVerticalAngle = 90;

    [Header("Obstructions")]
    [SerializeField]
    private float checkRadius = 0.2f;
    [SerializeField]
    private LayerMask obstructionLayers = -1;

    [Header("Lock On")]
    [SerializeField]
    private float lockOnLossTime = 1;
    [SerializeField]
    private float lockOnDistance = 15;
    [SerializeField]
    private LayerMask lockOnLayers = -1;
    [SerializeField]
    private Vector3 lockOnFraming = Vector3.zero;
    [SerializeField]
    [Range(1, 179)] private float lockOnFOV = 40;



    public bool LockedOn { get => lockedOn; }
    public ITargetable Target { get => target; }

    public Vector3 CameraPlanarDirection { get => planarDirection; }

    //Privates
    private List<Collider> ignoreColliders = new List<Collider>();
    
    private Vector3 planarDirection; //Cameras forward on the x,z plane
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private Vector3 newPosition;
    private Quaternion newRotation;

    private float targetVerticalAngle;
    private float targetDistance;

    private bool lockedOn;
    private float lockOnLossTimeCurrent;
    private ITargetable target;

    private float fovNormal;
    private float framingLerp;
    #endregion


    #region Methods
    private void Start()
    {
        ignoreColliders.AddRange(GetComponentsInChildren<Collider>());

        fovNormal = mainCamera.fieldOfView;
        planarDirection = followTransform.forward;

        targetDistance = defaultDistance;
        targetVerticalAngle = defaultVerticalAngle;

        targetPosition = followTransform.position - (targetRotation * Vector3.forward) * targetDistance;
        targetRotation = Quaternion.LookRotation(planarDirection) * Quaternion.Euler(targetVerticalAngle, 0, 0);

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if(Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        float mouseX = PlayerInput.MouseXInput;
        float mouseY = -PlayerInput.MouseYInput;
        float zoom = -PlayerInput.MouseScrollInput * zoomSpeed;

        if(invertX)
        {
            mouseX *= -1f;
        }
        if(invertY)
        {
            mouseY *= -1f;
        }

        Vector3 framing = Vector3.Lerp(framingNormal, lockOnFraming, framingLerp);
        Vector3 focusPosition = followTransform.position + followTransform.TransformDirection(framing);
        float fov = Mathf.Lerp(fovNormal, lockOnFOV, framingLerp);
        mainCamera.fieldOfView = fov;

        if(lockedOn && target != null)
        {
            Vector3 camToTarget = target.TargetTransform.position - mainCamera.transform.position;
            Vector3 planarCamToTarget = Vector3.ProjectOnPlane(camToTarget, Vector3.up);
            Quaternion lookRotation = Quaternion.LookRotation(camToTarget, Vector3.up);

            framingLerp = Mathf.Clamp01(framingLerp + Time.deltaTime * 4);
            planarDirection = planarCamToTarget != Vector3.zero ? planarCamToTarget.normalized : planarCamToTarget;
            targetDistance = Mathf.Clamp(targetDistance + zoom, minDistance, maxDistance);
            targetVerticalAngle = Mathf.Clamp(lookRotation.eulerAngles.x, minVerticalAngle, maxVerticalAngle);
        }
        else
        {
            framingLerp = Mathf.Clamp01(framingLerp - Time.deltaTime * 4);
            planarDirection = Quaternion.Euler(0, mouseX, 0) * planarDirection;
            targetVerticalAngle = Mathf.Clamp(targetVerticalAngle + mouseY, minVerticalAngle, maxVerticalAngle);
            targetDistance = Mathf.Clamp(targetDistance + zoom, minDistance, maxDistance);
        }

        //Handle Obstructions
        float smallestDistance = targetDistance;
        RaycastHit[] hits = Physics.SphereCastAll(focusPosition, checkRadius, targetRotation * -Vector3.forward, targetDistance, obstructionLayers);
        if(hits.Length != 0)
        {
            foreach(RaycastHit hit in hits)
            {
                if(!ignoreColliders.Contains(hit.collider))
                {
                    if(hit.distance < smallestDistance)
                    {
                        smallestDistance = hit.distance;
                    }
                }
            }
        }

        //Final Targets
        targetPosition = focusPosition - (targetRotation * Vector3.forward) * smallestDistance;
        targetRotation = Quaternion.LookRotation(planarDirection) * Quaternion.Euler(targetVerticalAngle,0,0);

        //HandleSmoothing
        newPosition = Vector3.Slerp(mainCamera.transform.position, targetPosition, Time.deltaTime * rotationSharpness);
        newRotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * rotationSharpness);

        //Apply Targets
        mainCamera.transform.position = newPosition;
        mainCamera.transform.rotation = newRotation;

        if(lockedOn && target != null)
        {
            bool valid = target.Targetable && InDistance(target) 
                && InScreen(target) && NotBlocked(target);

            if(valid)
            {
                lockOnLossTimeCurrent = 0;
            }
            else
            {
                lockOnLossTimeCurrent = Mathf.Clamp(lockOnLossTimeCurrent + Time.deltaTime, 0, lockOnLossTime);
            }

            if(lockOnLossTimeCurrent == lockOnLossTime)
            {
                lockedOn = false;
            }
        }
    }

    public void ToggleLock(bool toggle)
    {
        if(toggle == lockedOn)
        {
            return;
        }

        //Toggle
        lockedOn = !lockedOn;

        //Find a lock on target
        if(lockedOn)
        {
            List<ITargetable> targetables = new List<ITargetable>();
            Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnDistance, lockOnLayers);
            foreach(Collider collider in colliders)
            {
                ITargetable targetable = collider.GetComponent<ITargetable>();
                if(targetable != null)
                {
                    if(targetable.Targetable)
                    {
                        if(InScreen(targetable))
                        {
                            if(NotBlocked(targetable))
                            {
                                targetables.Add(targetable);
                            }
                        }
                    }
                }
            }

            //Find Closest (if more than 1)
            float hypotenuse;
            float smallestDistance = Mathf.Infinity;
            ITargetable closestTargetable = null;
            foreach (ITargetable t in targetables)
            {
                hypotenuse = CalculateHypotenuse(t.TargetTransform.position);
                if(smallestDistance > hypotenuse)
                {
                    closestTargetable = t;
                    smallestDistance = hypotenuse;
                }
            }

            //Final
            target = closestTargetable;
            lockedOn = closestTargetable != null;
        }
    }

    private bool InDistance(ITargetable targetable)
    {
        float distance = Vector3.Distance(transform.position, targetable.TargetTransform.position);
        return distance <= lockOnDistance;
    }

    private bool InScreen(ITargetable targetable)
    {
        Vector3 viewPortPosition = mainCamera.WorldToViewportPoint(targetable.TargetTransform.position);

        if(!(viewPortPosition.x > 0) || !(viewPortPosition.x < 1))
        {
            return false;
        }
        if (!(viewPortPosition.y > 0) || !(viewPortPosition.y < 1))
        {
            return false;
        }
        if (!(viewPortPosition.z > 0))
        {
            return false;
        }

        return true;
    }

    private bool NotBlocked(ITargetable targetable)
    {
        Vector3 origin = mainCamera.transform.position;
        Vector3 direction = targetable.TargetTransform.position - origin;

        float radius = 0.15f;
        float distance = direction.magnitude;

        bool notBlocked = !Physics.SphereCast(origin, radius, direction, out RaycastHit hit, distance, obstructionLayers);

        return notBlocked;
    }

    private float CalculateHypotenuse(Vector3 position)
    {
        float screenCenterX = mainCamera.pixelWidth / 2;
        float screenCenterY = mainCamera.pixelHeight / 2;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(position);
        float xDelta = screenCenterX - screenPosition.x;
        float yDelta = screenCenterY - screenPosition.y;
        float hypotenuse = Mathf.Sqrt(Mathf.Pow(xDelta, 2) + Mathf.Pow(yDelta, 2));

        return hypotenuse;
    }

    private void OnValidate()
    {
        defaultDistance = Mathf.Clamp(defaultDistance, minDistance, maxDistance);
        defaultVerticalAngle = Mathf.Clamp(defaultVerticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    #endregion
}
