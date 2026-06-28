using UnityEngine;

public class BottleTilt : MonoBehaviour
{
    public bool canTilt = true;
    public float speed = 60f;

    void Update()
    {
        if (!canTilt)
            return;

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.back * speed * Time.deltaTime, Space.Self);
        }
    }
}