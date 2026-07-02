using UnityEngine;

public class OilPoolController : MonoBehaviour
{
    public Transform oilSurface;

    public float maxSize = 1f;
    public float fillSpeed = 0.15f;

    float currentSize = 0;

    bool filling = false;

    void Update()
    {
        if (filling)
        {
            currentSize += fillSpeed * Time.deltaTime;

            currentSize = Mathf.Clamp(
                currentSize,
                0,
                maxSize
            );

            oilSurface.localScale =
                new Vector3(
                    currentSize,
                    1,
                    currentSize
                );
        }
    }

    public void StartFill()
    {
        filling = true;
    }

    public void StopFill()
    {
        filling = false;
    }
}