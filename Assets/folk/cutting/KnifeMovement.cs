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

    private bool isDragging;
    private bool isChopping;
    private Plane dragPlane;
    private Collider knifeCollider;
    private Vector3 dragStartPosition;
    private float dragStartPointerX;
    private float topWorldY;
    private float bottomWorldY;

    // Buffer สำหรับลด GC Allocations ตอนใช้ RaycastAll
    private static readonly RaycastHit[] raycastHitsBuffer = new RaycastHit[16];

    public bool IsSliceActive { get; private set; }
    public static bool IsAnyKnifeDragging => activeKnife != null && activeKnife.isDragging;

    private void Awake()
    {
        if (!TryGetComponent<Collider>(out knifeCollider))
            knifeCollider = GetComponentInChildren<Collider>();
    }

    private void OnEnable()
    {
        instances.Add(this);
        isDragging = false;
        isChopping = false;
        if (activeKnife == this) activeKnife = null;
    }

    private void Start()
    {
        UpdateWorldYPositions();
    }

    private void UpdateWorldYPositions()
    {
        topWorldY = transform.position.y;
        bottomWorldY = topWorldY - Mathf.Abs(topYPosition - bottomYPosition);
    }

    private void Update()
    {
        if (isChopping) return;

        if (activeKnife != null && activeKnife != this)
            return;

        Camera activeCamera = Camera.main;
        if (activeCamera == null || !activeCamera.enabled)
            return;

        Vector2 inputScreenPosition = GetInputPosition();

        if (IsInputPressedThisFrame())
        {
            if (activeKnife == null && IsTopKnifeUnderCursor(inputScreenPosition, activeCamera))
            {
                UpdateWorldYPositions();

                isDragging = true;
                activeKnife = this;
                dragStartPosition = transform.position;
                dragPlane = new Plane(Vector3.up, new Vector3(0f, topWorldY, 0f));

                Ray ray = activeCamera.ScreenPointToRay(inputScreenPosition);
                if (dragPlane.Raycast(ray, out float enter))
                    dragStartPointerX = ray.GetPoint(enter).x;
            }
        }

        if (isDragging && IsInputHeld())
        {
            Ray ray = activeCamera.ScreenPointToRay(inputScreenPosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                float deltaX = hitPoint.x - dragStartPointerX;
                transform.position = new Vector3(
                    dragStartPosition.x + deltaX,
                    topWorldY,
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

    private bool IsTopKnifeUnderCursor(Vector2 screenPosition, Camera camera)
    {
        if (knifeCollider == null)
            return false;

        return GetKnifeUnderCursor(screenPosition, camera) == this;
    }

    private static KnifeMovement GetKnifeUnderCursor(Vector2 screenPosition, Camera camera)
    {
        if (camera == null || instances.Count == 0)
            return null;

        Ray ray = camera.ScreenPointToRay(screenPosition);
        
        // 💡 OPTIMIZATION KEY: ใช้ RaycastNonAlloc เพื่อไม่สร้าง Garbage บน Memory
        int hitCount = Physics.RaycastNonAlloc(
            ray,
            raycastHitsBuffer,
            Mathf.Infinity,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);

        if (hitCount == 0)
            return null;

        System.Array.Sort(raycastHitsBuffer, 0, hitCount, RaycastHitDistanceComparer.Instance);

        for (int i = 0; i < hitCount; i++)
        {
            KnifeMovement knife = raycastHitsBuffer[i].collider.GetComponentInParent<KnifeMovement>();
            if (knife != null && knife.enabled)
                return knife;
        }

        return null;
    }

    private void PlayChopTween()
    {
        isChopping = true;
        IsSliceActive = false;
        LeanTween.cancel(gameObject);

        Vector3 chopPos = new Vector3(transform.position.x, bottomWorldY, transform.position.z);
        Vector3 topPos = new Vector3(transform.position.x, topWorldY, transform.position.z);

        float chopDuration = Mathf.Max(0.01f, Mathf.Abs(transform.position.y - bottomWorldY) / chopSpeed);
        float resetDuration = Mathf.Max(0.01f, Mathf.Abs(topWorldY - bottomWorldY) / resetSpeed);

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

    private class RaycastHitDistanceComparer : IComparer<RaycastHit>
    {
        public static readonly RaycastHitDistanceComparer Instance = new RaycastHitDistanceComparer();
        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }
}