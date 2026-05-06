using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 0.125f; // Adjust between 0 and 1. Lower = more delay.
    public Vector3 offset; // Usually (0, 0, -10) for ortho

    void LateUpdate()
    {
        // The position we want the camera to be at
        Vector3 desiredPosition = player.position + offset;
        
        // Smoothly interpolate between current and desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Apply to camera
        transform.position = smoothedPosition;
    }
}
