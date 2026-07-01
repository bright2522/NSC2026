using UnityEngine;

public class BottleTilt : MonoBehaviour
{
    public bool canTilt = false;

    public ParticleSystem oilParticle;

    public float tiltAngle = 70f;
    public float tiltSpeed = 5f;

    // เอียงเกินกี่องศาถึงเริ่มเท
    public float pourThreshold = 35f;

    void Update()
    {
        if (!canTilt)
            return;

        float input = 0f;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            input = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            input = 1f;
#else
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

        float currentAngle = Mathf.Abs(transform.eulerAngles.z);

        if (currentAngle > 180)
            currentAngle = 360 - currentAngle;

        if (currentAngle > pourThreshold)
        {
            if (!oilParticle.isPlaying)
                oilParticle.Play();
        }
        else
        {
            if (oilParticle.isPlaying)
                oilParticle.Stop();
        }
    }
}