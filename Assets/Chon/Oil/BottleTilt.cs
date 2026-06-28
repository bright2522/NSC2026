using UnityEngine;

public class BottleTilt : MonoBehaviour
{
    public bool canTilt = false;

    public float maxAngle = 30f;

    public float smoothSpeed = 5f;

    void Update()
    {
        if (!canTilt)
            return;

        float tilt = Input.acceleration.x;

        float targetAngle = tilt * maxAngle;

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }
}