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
        // --- ปุ่มคีย์บอร์ด: ใช้ได้ทุกที่ (Game view + Simulator) ---
        // ย้ายออกมานอก #if แล้ว เพื่อให้กด A/D เทได้แม้อยู่ใน Device Simulator
        Debug.Log("กด A ติด!");
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            return -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            return 1f;

        // --- การเอียงเครื่อง (accelerometer) สำหรับมือถือจริง ---
        if (!calibrated)
            CalibrateNeutral();

        float relativeX = Input.acceleration.x - neutralAcceleration.x;
        const float deadZone = 0.04f;
        if (Mathf.Abs(relativeX) < deadZone)
            return 0f;

        return relativeX;
    }
}