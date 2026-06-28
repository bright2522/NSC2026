using UnityEngine;

public class BottleTilt : MonoBehaviour
{
    public float tiltAngle = 70f;
    public float tiltSpeed = 5f;

    void Update()
    {
        float input = 0f;

#if UNITY_EDITOR
        // ใช้ปุ่ม A/D จำลองการเอียงในคอม
        if (Input.GetKey(KeyCode.A))
            input = -1f;

        if (Input.GetKey(KeyCode.D))
            input = 1f;
#else
        // ใช้เซ็นเซอร์เอียงบนมือถือ
        input = Input.acceleration.x;
#endif

        Quaternion targetRotation = Quaternion.Euler(
            0,
            0,
            -input * tiltAngle
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * tiltSpeed
        );
    }
}