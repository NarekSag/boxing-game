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
    private Vector3 framing = Vector3.zero;

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

    //Privates
    private List<Collider> ignoreColliders = new List<Collider>();
    
    private Vector3 planarDirection; //Cameras forward on the x,z plane
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private Vector3 newPosition;
    private Quaternion newRotation;

    private float targetVerticalAngle;
    private float targetDistance;
    #endregion


    #region Methods
    private void Start()
    {
        ignoreColliders.AddRange(GetComponentsInChildren<Collider>());

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

        Vector3 focusPosition = followTransform.position + new Vector3(framing.x, framing.y, 0);

        planarDirection = Quaternion.Euler(0, mouseX, 0) * planarDirection;
        targetVerticalAngle = Mathf.Clamp(targetVerticalAngle + mouseY, minVerticalAngle, maxVerticalAngle);
        targetDistance = Mathf.Clamp(targetDistance + zoom, minDistance, maxDistance);

        Debug.DrawLine(mainCamera.transform.position, mainCamera.transform.position + planarDirection, Color.cyan);

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
    }

    private void OnValidate()
    {
        defaultDistance = Mathf.Clamp(defaultDistance, minDistance, maxDistance);
        defaultVerticalAngle = Mathf.Clamp(defaultVerticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    #endregion
}
