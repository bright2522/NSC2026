using UnityEngine;

public static class CupTiltInput
{
    private static Vector3 neutralAcceleration;
    private static bool calibrated;

    public static void EnableSensors()
    {
        Input.compensateSensors = true;
    }

    public static void CalibrateNeutral()
    {
        EnableSensors();
        neutralAcceleration = Input.acceleration;
        calibrated = true;
    }

    public static void ResetCalibration()
    {
        calibrated = false;
    }

    public static float ReadPourAxis()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            return -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            return 1f;
#endif

        if (!calibrated)
            CalibrateNeutral();

        float relativeX = Input.acceleration.x - neutralAcceleration.x;
        const float deadZone = 0.04f;
        if (Mathf.Abs(relativeX) < deadZone)
            return 0f;

        return relativeX;
    }
}
