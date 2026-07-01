using System.Collections.Generic;
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

    private static readonly List<KnifeMovement> instances = new List<KnifeMovement>();
    private static KnifeMovement activeKnife;

    private Camera mainCamera;
    private bool isDragging;
    private bool isChopping;
    private Plane dragPlane;
    private Collider knifeCollider;
    private Vector3 dragStartPosition;
    private float dragStartPointerX;

    public bool IsSliceActive { get; private set; }

    private void Awake()
    {
        knifeCollider = GetComponent<Collider>();
        if (knifeCollider == null)
            knifeCollider = GetComponentInChildren<Collider>();
    }

    private void OnEnable()
    {
        instances.Add(this);
    }

    private void Start()
    {
        mainCamera = Camera.main;
        BakeChildOffsetIntoRoot();
    }

    private void BakeChildOffsetIntoRoot()
    {
        Transform visualChild = GetPositionedVisualChild();
        if (visualChild != null)
        {
            Vector3 worldPos = visualChild.position;
            transform.position = new Vector3(worldPos.x, topYPosition, worldPos.z);
            visualChild.SetParent(transform, true);
            return;
        }

        transform.position = new Vector3(transform.position.x, topYPosition, transform.position.z);
    }

    private Transform GetPositionedVisualChild()
    {
        if (transform.childCount == 0)
            return null;

        Transform bestChild = null;
        float largestLocalSqr = 0f;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            float localSqr = child.localPosition.sqrMagnitude;
            if (localSqr <= largestLocalSqr)
                continue;

            largestLocalSqr = localSqr;
            bestChild = child;
        }

        return largestLocalSqr > 0.0001f ? bestChild : null;
    }

    private void Update()
    {
        if (isChopping) return;

        if (activeKnife != null && activeKnife != this)
            return;

        Vector2 inputScreenPosition = GetInputPosition();

        if (IsInputPressedThisFrame())
        {
            if (activeKnife == null && IsTopKnifeUnderCursor(inputScreenPosition))
            {
                isDragging = true;
                activeKnife = this;
                dragStartPosition = transform.position;
                dragPlane = new Plane(Vector3.up, new Vector3(0f, topYPosition, 0f));

                Ray ray = mainCamera.ScreenPointToRay(inputScreenPosition);
                if (dragPlane.Raycast(ray, out float enter))
                    dragStartPointerX = ray.GetPoint(enter).x;
            }
        }

        if (isDragging && IsInputHeld())
        {
            Ray ray = mainCamera.ScreenPointToRay(inputScreenPosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                float deltaX = hitPoint.x - dragStartPointerX;
                transform.position = new Vector3(
                    dragStartPosition.x + deltaX,
                    topYPosition,
                    dragStartPosition.z);
            }
        }

        if (isDragging && IsInputReleasedThisFrame())
        {
            isDragging = false;
            activeKnife = null;
            PlayChopTween();
        }
    }

    private void OnDisable()
    {
        instances.Remove(this);
        ReleaseInputOwnership();
        LeanTween.cancel(gameObject);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
        ReleaseInputOwnership();
        LeanTween.cancel(gameObject);
    }

    private void ReleaseInputOwnership()
    {
        if (activeKnife != this)
            return;

        activeKnife = null;
        isDragging = false;
    }

    private bool IsTopKnifeUnderCursor(Vector2 screenPosition)
    {
        if (mainCamera == null || knifeCollider == null)
            return false;

        return GetKnifeUnderCursor(screenPosition) == this;
    }

    private static KnifeMovement GetKnifeUnderCursor(Vector2 screenPosition)
    {
        Camera camera = Camera.main;
        if (camera == null || instances.Count == 0)
            return null;

        Ray ray = camera.ScreenPointToRay(screenPosition);
        KnifeMovement closestKnife = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < instances.Count; i++)
        {
            KnifeMovement knife = instances[i];
            Collider col = knife.knifeCollider;
            if (knife == null || col == null)
                continue;

            if (!col.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestKnife = knife;
            }
        }

        return closestKnife;
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
