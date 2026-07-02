using UnityEngine;

public class BottleDrag : MonoBehaviour
{
    public Transform snapPoint;
    public float snapDistance = 1f;
    public float snapSpeed = 10f;

    private bool dragging;
    private bool moveToSnap;
    private bool returnHome;

    private float fixedX;
    private Plane dragPlane;

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        fixedX = transform.position.x;

        dragPlane = new Plane(
            Vector3.right,
            new Vector3(fixedX, 0, 0)
        );
    }

    void OnMouseDown()
    {
        if (returnHome) return;

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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float distance;

            if (dragPlane.Raycast(ray, out distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);

                transform.position = new Vector3(
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
                transform.position = snapPoint.position;

                moveToSnap = false;

                GetComponent<BottleTilt>().canTilt = true;
            }
        }

        if (returnHome)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                startPosition,
                Time.deltaTime * 5f
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                startRotation,
                Time.deltaTime * 5f
            );

            if (Vector3.Distance(transform.position, startPosition) < 0.05f)
            {
                transform.position = startPosition;
                transform.rotation = startRotation;

                returnHome = false;

                gameObject.SetActive(false);
            }
        }
    }

    public void ReturnBottle()
    {
        dragging = false;
        moveToSnap = false;
        returnHome = true;
    }
}