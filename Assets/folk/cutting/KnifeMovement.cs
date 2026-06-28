using UnityEngine;
using UnityEngine.InputSystem;

public class KnifeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float topYPosition = 1.5f;
    public float bottomYPosition = 0.15f;
    public float chopSpeed = 40f;
    public float resetSpeed = 10f;
    public float chopHoldDuration = 0.04f;

    [Header("LeanTween")]
    public LeanTweenType chopEase = LeanTweenType.easeInQuad;
    public LeanTweenType resetEase = LeanTweenType.easeOutQuad;

    private Camera mainCamera;
    private bool isDragging;
    private bool isChopping;
    private Plane dragPlane;
    private Collider knifeCollider;

    public bool IsSliceActive { get; private set; }

    private void Start()
    {
        mainCamera = Camera.main;
        knifeCollider = GetComponent<Collider>();
        transform.position = new Vector3(transform.position.x, topYPosition, transform.position.z);
    }

    private void Update()
    {
        if (isChopping) return;

        Vector2 inputScreenPosition = GetInputPosition();

        if (IsInputPressedThisFrame())
        {
            Ray ray = mainCamera.ScreenPointToRay(inputScreenPosition);
            if (knifeCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                isDragging = true;
                dragPlane = new Plane(Vector3.up, new Vector3(0f, topYPosition, 0f));
            }
        }

        if (isDragging && IsInputHeld())
        {
            Ray ray = mainCamera.ScreenPointToRay(inputScreenPosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                transform.position = new Vector3(hitPoint.x, topYPosition, transform.position.z);
            }
        }

        if (isDragging && IsInputReleasedThisFrame())
        {
            isDragging = false;
            PlayChopTween();
        }
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
    }

    private void PlayChopTween()
    {
        isChopping = true;
        IsSliceActive = false;
        LeanTween.cancel(gameObject);

        Vector3 chopPos = new Vector3(transform.position.x, bottomYPosition, transform.position.z);
        Vector3 topPos = new Vector3(transform.position.x, topYPosition, transform.position.z);

        float chopDuration = Mathf.Max(0.01f, Mathf.Abs(transform.position.y - bottomYPosition) / chopSpeed);
        float resetDuration = Mathf.Max(0.01f, Mathf.Abs(topYPosition - bottomYPosition) / resetSpeed);

        LeanTween.move(gameObject, chopPos, chopDuration)
            .setEase(chopEase)
            .setOnStart(() => IsSliceActive = true)
            .setOnComplete(() =>
            {
                transform.position = chopPos;
                IsSliceActive = false;
                LeanTween.delayedCall(gameObject, chopHoldDuration, () =>
                {
                    LeanTween.move(gameObject, topPos, resetDuration)
                        .setEase(resetEase)
                        .setOnComplete(() =>
                        {
                            transform.position = topPos;
                            isChopping = false;
                            IsSliceActive = false;
                        });
                });
            });
    }

    private Vector2 GetInputPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            return Touchscreen.current.touches[0].position.ReadValue();

        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return Vector2.zero;
    }

    private bool IsInputPressedThisFrame()
    {
        bool mobileTouch = Touchscreen.current != null
            && Touchscreen.current.touches.Count > 0
            && Touchscreen.current.touches[0].press.wasPressedThisFrame;
        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return mobileTouch || mouseClick;
    }

    private bool IsInputHeld()
    {
        bool mobileTouch = Touchscreen.current != null
            && Touchscreen.current.touches.Count > 0
            && Touchscreen.current.touches[0].press.isPressed;
        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.isPressed;
        return mobileTouch || mouseClick;
    }

    private bool IsInputReleasedThisFrame()
    {
        bool mobileTouch = Touchscreen.current != null
            && Touchscreen.current.touches.Count > 0
            && Touchscreen.current.touches[0].press.wasReleasedThisFrame;
        bool mouseClick = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        return mobileTouch || mouseClick;
    }
}
