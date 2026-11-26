
using UnityEngine;
using UnityEngine.InputSystem;

public class RTSCamera : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;

    [Header("Movement")]
    public float moveSpeed = 25f;
    public float fastMoveSpeed = 50f;
    public float movementSmoothing = 10f;

    [Header("Camera Rotation")]
    public float lookSensitivity = 0.2f;
    public float minPitch = 15f;
    public float maxPitch = 80f;

    [Header("Edge Scrolling")]
    public bool edgeScrolling = true;
    public float edgeSize = 15f;
    public float edgeSpeed = 20f;

    [Header("Camera Collision")]
    public bool cameraCollision = true;
    public float collisionRadius = 0.3f;
    public float collisionPadding = 0.2f;
    public LayerMask collisionLayers = ~0;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 60f;
    public float zoomSmoothing = 10f;

    [Header("Vertical Zoom")]
    public float verticalZoomMultiplier = 1f;

    private Vector3 targetPosition;
    private float yaw;
    private float pitch = 45f;
    private float currentZoom = 30f;
    private float targetZoom = 30f;

    void Start()
    {
        if (cameraTransform == null && transform.childCount > 0)
            cameraTransform = transform.GetChild(0);

        targetPosition = transform.position;

        if (cameraTransform != null)
        {
            Vector3 angles = cameraTransform.localEulerAngles;

            pitch = angles.x;
            yaw = transform.eulerAngles.y;

            currentZoom = cameraTransform.localPosition.magnitude;
            targetZoom = currentZoom;

            UpdateCamera();
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            movementSmoothing * Time.deltaTime
        );
    }

    void HandleMovement()
    {
        if (Keyboard.current == null)
            return;

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1;

        float speed = Keyboard.current.leftShiftKey.isPressed
            ? fastMoveSpeed : moveSpeed;

        // Movement follows the camera's horizontal direction.
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 movement =
            (forward * input.y + right * input.x).normalized;

        targetPosition += movement * speed * Time.deltaTime;

        // Edge scrolling
        if (edgeScrolling && Mouse.current != null &&
            !Mouse.current.rightButton.isPressed)
        {
            Vector2 mouse = Mouse.current.position.ReadValue();

            Vector3 edgeDirection = Vector3.zero;

            if (mouse.x >= Screen.width - edgeSize)
                edgeDirection += right;

            if (mouse.x <= edgeSize)
                edgeDirection -= right;

            if (mouse.y >= Screen.height - edgeSize)
                edgeDirection += forward;

            if (mouse.y <= edgeSize)
                edgeDirection -= forward;

            edgeDirection.y = 0;

            if (edgeDirection.sqrMagnitude > 0)
            {
                targetPosition += edgeDirection.normalized *
                                  edgeSpeed * Time.deltaTime;
            }
        }
    }

    void HandleRotation()
    {
        if (Mouse.current == null || cameraTransform == null)
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * lookSensitivity;
            pitch -= mouseDelta.y * lookSensitivity;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            UpdateCamera();
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    void HandleZoom()
    {
        if (Mouse.current == null || cameraTransform == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        targetZoom -= scroll * zoomSpeed * 0.01f;

        targetZoom = Mathf.Clamp(
            targetZoom,
            minZoom,
            maxZoom
        );

        currentZoom = Mathf.Lerp(
            currentZoom,
            targetZoom,
            zoomSmoothing * Time.deltaTime
        );

        UpdateCamera();
    }



    void UpdateCamera()
    {
        transform.rotation = Quaternion.Euler(0, yaw, 0);

        cameraTransform.localRotation = Quaternion.Euler(
            pitch, 0, 0
        );

        float height = currentZoom *
                       Mathf.Sin(pitch * Mathf.Deg2Rad) *
                       verticalZoomMultiplier;

        float distance = currentZoom *
                         Mathf.Cos(pitch * Mathf.Deg2Rad);

        Vector3 desiredPosition = new Vector3(
            0,
            height,
            -distance
        );

        if (cameraCollision)
        {
            Vector3 origin = transform.position;

            Vector3 direction =
                transform.TransformDirection(desiredPosition.normalized);

            float desiredDistance = desiredPosition.magnitude;

            if (Physics.SphereCast(
                origin,
                collisionRadius,
                direction,
                out RaycastHit hit,
                desiredDistance,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
            {
                float safeDistance = Mathf.Max(
                    0.1f,
                    hit.distance - collisionPadding
                );

                desiredPosition = direction * safeDistance;
            }
        }

        cameraTransform.localPosition = desiredPosition;
    }




    void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
