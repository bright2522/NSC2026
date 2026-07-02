using UnityEngine;

public class OilMeshController : MonoBehaviour
{
    public Transform oilMesh;

    public float maxLength = 2f;
    public float speed = 5f;

    private bool pouring = false;
    private float currentLength = 0f;

    void Update()
    {
        if (pouring)
        {
            currentLength = Mathf.MoveTowards(
                currentLength,
                maxLength,
                speed * Time.deltaTime
            );
        }
        else
        {
            currentLength = Mathf.MoveTowards(
                currentLength,
                0f,
                speed * Time.deltaTime
            );
        }

        oilMesh.localScale = new Vector3(
            0.03f,
            currentLength,
            0.03f
        );

        oilMesh.localPosition = new Vector3(
            0,
            -currentLength / 2f,
            0
        );
    }

    public void StartPour()
    {
        pouring = true;
    }

    public void StopPour()
    {
        pouring = false;
    }
}