using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float smoothSpeed = 0.125f; 
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minSize = 2f;
    public float maxSize = 15f;
    public float zoomSmoothness = 10f;

    private Camera cam;
    private float targetSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            targetSize = cam.orthographicSize;
        }
    }

    void LateUpdate()
    {
        if (player == null || cam == null) return;

        // --- POSITION FOLLOWING ---
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // --- SCROLL ZOOMING ---
        // Get mouse scroll input (typically -1 to 1)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0)
        {
            // Calculate new target size, clamped between min and max
            targetSize -= scrollInput * zoomSpeed;
            targetSize = Mathf.Clamp(targetSize, minSize, maxSize);
        }

        // Smoothly interpolate the camera's orthographic size
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSmoothness);
    }
}
