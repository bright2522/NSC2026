using UnityEngine;

public class BottleDrag : MonoBehaviour
{
    public Transform snapPoint;
    public float snapDistance = 1f;
    public float snapSpeed = 10f;

    private bool dragging;
    private bool moveToSnap;

    private float fixedX;
    private Plane dragPlane;

    void Start()
    {
        fixedX = transform.position.x;

        dragPlane = new Plane(
            Vector3.right,
            new Vector3(fixedX, 0, 0)
        );
    }

    void OnMouseDown()
    {
        dragging = true;
        moveToSnap = false;
    }

    void OnMouseUp()
    {
        dragging = false;

        float d = Vector3.Distance(
            transform.position,
            snapPoint.position
        );

        if (d <= snapDistance)
        {
            moveToSnap = true;
        }
    }

    void Update()
    {
        if (dragging)
        {
            Ray ray =
                Camera.main.ScreenPointToRay(
                    Input.mousePosition
                );

            float distance;

            if (dragPlane.Raycast(ray, out distance))
            {
                Vector3 hitPoint =
                    ray.GetPoint(distance);

                transform.position =
                    new Vector3(
                        fixedX,
                        hitPoint.y,
                        hitPoint.z
                    );
            }
        }

        if (moveToSnap)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                snapPoint.position,
                Time.deltaTime * snapSpeed
            );

            if (Vector3.Distance(
                transform.position,
                snapPoint.position
            ) < 0.05f)
            {
                transform.position =
                    snapPoint.position;

                moveToSnap = false;
            }
        }
    }
}