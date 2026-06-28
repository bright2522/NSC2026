using System;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Pep.Minigames.Plating
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class DraggablePlateItem : MonoBehaviour
    {
        [Header("Lift")]
        [SerializeField] private float liftHeight = 0.6f;
        [SerializeField] private float liftAnimDuration = 0.22f;

        [Header("Drag Feel")]
        [SerializeField] private float dragFollowSpeed = 20f;
        [SerializeField] private float pickupScaleMultiplier = 1.18f;
        [SerializeField] private float tiltAmount = 18f;
        [SerializeField] private float tiltSpeed = 10f;
        [SerializeField] private float bobAmplitude = 0.025f;
        [SerializeField] private float bobFrequency = 2.2f;

        [Header("Physics Drop")]
        [SerializeField] private float landFreezeDelay = 0.12f;
        [SerializeField] private float landFreezeVelocityThreshold = 0.08f;

        [Header("Return")]
        [SerializeField] private float returnAnimDuration = 0.32f;

        [Header("Hold to Remove")]
        [SerializeField] private float holdToRemoveDuration = 1.0f;


        public event Action<DraggablePlateItem> OnPickedUp;
        public event Action<DraggablePlateItem> OnDropped;
        public event Action<DraggablePlateItem> OnReturnedToTray;
        public event Action<DraggablePlateItem> OnLanded;

        public PlateItemSO ItemData { get; private set; }
        public bool IsDragging { get; private set; }
        public bool IsPlaced { get; private set; }
        public Vector3 TrayPosition { get; private set; }

        private PlatingItemCatalogManager catalog;
        private PlateDropZone currentDropZone;
        private PlateDropZone pendingDropZone;
        private Camera mainCamera;
        private Rigidbody rb;
        private Renderer itemRenderer;
        private Vector3 originalScale;
        private Quaternion originalRotation;
        private Plane dragPlane;
        private bool isInteractable = true;
        private bool isLifting;
        private bool isWaitingToLand;
        private float landFreezeTimer;

        private Vector3 targetDragPos;
        private Vector3 lastPos;
        private float bobTimer;
        private bool isInDropZone;
        private bool isHoldingToRemove;
        private float holdProgress;

        private static readonly Color HoldEmission = new Color(0.8f, 0.1f, 0.05f);

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            itemRenderer = GetComponentInChildren<Renderer>();
        }

        public void Initialise(PlateItemSO data, Vector3 trayPos, PlatingItemCatalogManager catalogManager)
        {
            ItemData = data;
            TrayPosition = trayPos;
            catalog = catalogManager;
            originalScale = transform.localScale;
            originalRotation = transform.rotation;
            mainCamera = Camera.main;
            dragPlane = new Plane(Vector3.up, trayPos);
            lastPos = transform.position;
        }

        private void Update()
        {
            if (isWaitingToLand)
            {
                landFreezeTimer -= Time.deltaTime;
                if (landFreezeTimer <= 0f && rb.linearVelocity.magnitude < landFreezeVelocityThreshold)
                    FreezeOnLand();
                return;
            }

            if (isHoldingToRemove)
            {
                UpdateHoldToRemove();
                return;
            }

            if (IsDragging && !isLifting)
                UpdateDrag();
        }

        private void UpdateHoldToRemove()
        {
            holdProgress += Time.deltaTime / holdToRemoveDuration;
            holdProgress = Mathf.Clamp01(holdProgress);

            // easeInQuad — slow start, fast finish
            float eased = holdProgress * holdProgress;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, eased);

            // Emission shifts from red to bright red as progress increases
            Color holdColor = Color.Lerp(HoldEmission, HoldEmission * 2.5f, eased);
            SetEmission(holdColor);

            if (holdProgress >= 1f)
                CompleteHoldRemove();
        }

        private void UpdateDrag()
        {
            bobTimer += Time.deltaTime;
            float bob = Mathf.Sin(bobTimer * bobFrequency * Mathf.PI * 2f) * bobAmplitude;

            Vector3 smooth = Vector3.Lerp(transform.position,
                targetDragPos + Vector3.up * bob,
                Time.deltaTime * dragFollowSpeed);
            transform.position = smooth;

            currentDropZone?.UpdateSnapPreview(this);

            Vector3 delta = transform.position - lastPos;
            lastPos = transform.position;

            if (delta.sqrMagnitude > 0.000001f)
            {
                float tiltX = -delta.z * tiltAmount * 60f;
                float tiltZ = delta.x * tiltAmount * 60f;
                Quaternion targetRot = originalRotation * Quaternion.Euler(tiltX, 0f, tiltZ);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * tiltSpeed);
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, Time.deltaTime * tiltSpeed);
            }
        }

        private void OnMouseDown()
        {
            if (!isInteractable || isWaitingToLand) return;

            if (IsPlaced)
            {
                BeginHoldToRemove();
                return;
            }

            if (catalog != null && !catalog.CanPlaceMore(ItemData.ItemId)) return;
            BeginDrag();
        }

        private void OnMouseDrag()
        {
            if (!IsDragging || isLifting) return;
            Vector3 worldPos = GetMouseWorldPosition();
            if (worldPos != Vector3.positiveInfinity)
                targetDragPos = worldPos;
        }

        private void OnMouseUp()
        {
            if (isHoldingToRemove)
            {
                CancelHoldToRemove();
                return;
            }
            if (!IsDragging) return;
            EndDrag();
        }

        private void BeginHoldToRemove()
        {
            isHoldingToRemove = true;
            holdProgress = 0f;
            LeanTween.cancel(gameObject);
        }

        private void CancelHoldToRemove()
        {
            isHoldingToRemove = false;
            ClearEmission();
            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale, 0.22f).setEaseOutBack();
        }

        private void CompleteHoldRemove()
        {
            isHoldingToRemove = false;
            catalog?.UnregisterPlaced(ItemData.ItemId);
            IsPlaced = false;
            transform.localScale = Vector3.zero;
            ClearEmission();
            ReturnToTray();
        }

        private void BeginDrag()
        {
            IsDragging = true;
            isLifting = true;
            bobTimer = 0f;

            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            float targetY = transform.position.y + liftHeight;
            targetDragPos = transform.position;
            lastPos = transform.position;

            LeanTween.cancel(gameObject);

            // Squash down briefly, then shoot up
            LeanTween.scale(gameObject,
                new Vector3(originalScale.x * 1.25f, originalScale.y * 0.75f, originalScale.z * 1.25f),
                0.07f)
                .setEaseOutQuad()
                .setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, originalScale * pickupScaleMultiplier, 0.18f)
                        .setEaseOutBack();
                });

            LeanTween.moveY(gameObject, targetY, liftAnimDuration)
                .setEaseOutCubic()
                .setOnComplete(() =>
                {
                    dragPlane = new Plane(Vector3.up, new Vector3(0f, targetY, 0f));
                    targetDragPos = transform.position;
                    isLifting = false;
                });

            OnPickedUp?.Invoke(this);
        }

        private void EndDrag()
        {
            IsDragging = false;
            isLifting = false;
            currentDropZone?.HideSnapPreview();
            transform.rotation = originalRotation;
            OnDropped?.Invoke(this);

            if (currentDropZone != null && currentDropZone.TryDrop(this))
                DropWithPhysics(currentDropZone);
            else
                ReturnToTray();
        }

        private void DropWithPhysics(PlateDropZone zone)
        {
            IsPlaced = true;
            catalog?.RegisterPlaced(ItemData.ItemId);
            pendingDropZone = zone;

            Vector3 snapXZ = zone.ComputeSnapPosition(this);
            transform.position = new Vector3(snapXZ.x, transform.position.y, snapXZ.z);

            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale * 0.9f, 0.1f).setEaseInQuad();

            rb.isKinematic = false;
            isWaitingToLand = true;
            landFreezeTimer = landFreezeDelay;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isWaitingToLand || landFreezeTimer > 0f) return;
            FreezeOnLand();
        }

        private void FreezeOnLand()
        {
            if (!isWaitingToLand) return;
            isWaitingToLand = false;

            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (pendingDropZone != null)
            {
                pendingDropZone.RegisterItemLanded(this, transform.position);
                pendingDropZone = null;
            }

            LeanTween.cancel(gameObject);

            // Squash on impact → stretch back → settle
            Vector3 squash = new Vector3(originalScale.x * 1.35f, originalScale.y * 0.6f, originalScale.z * 1.35f);
            LeanTween.scale(gameObject, squash, 0.07f)
                .setEaseOutQuad()
                .setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, originalScale * 1.05f, 0.14f)
                        .setEaseOutElastic()
                        .setOnComplete(() =>
                        {
                            LeanTween.scale(gameObject, originalScale, 0.1f).setEaseOutQuad();
                        });
                });

            OnLanded?.Invoke(this);
        }

        public void ReturnToTray()
        {
            IsPlaced = false;
            isWaitingToLand = false;
            isInDropZone = false;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            LeanTween.cancel(gameObject);

            // Spin + shrink slightly, then elastic return
            LeanTween.scale(gameObject, originalScale * 0.85f, 0.1f).setEaseInBack();
            LeanTween.rotateLocal(gameObject, originalRotation.eulerAngles + new Vector3(0f, -30f, 0f), 0.1f)
                .setEaseInBack()
                .setOnComplete(() =>
                {
                    LeanTween.move(gameObject, TrayPosition, returnAnimDuration).setEaseOutElastic();
                    LeanTween.scale(gameObject, originalScale, returnAnimDuration).setEaseOutElastic();
                    LeanTween.rotate(gameObject, originalRotation.eulerAngles, returnAnimDuration).setEaseOutElastic();
                });

            ClearEmission();
            OnReturnedToTray?.Invoke(this);
        }

        public void RemoveFromPlate()
        {
            if (!IsPlaced) return;
            catalog?.UnregisterPlaced(ItemData.ItemId);
            IsPlaced = false;
            ReturnToTray();
        }

        public void SetInteractable(bool value)
        {
            isInteractable = value;
        }

        private void OnTriggerEnter(Collider other)
        {
            var zone = other.GetComponent<PlateDropZone>();
            if (zone == null || !IsDragging) return;
            currentDropZone = zone;

            if (!isInDropZone)
            {
                isInDropZone = true;
                zone.ShowSnapPreview(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var zone = other.GetComponent<PlateDropZone>();
            if (zone == null || currentDropZone != zone) return;
            zone.HideSnapPreview();
            currentDropZone = null;
            isInDropZone = false;
        }

        private void SetEmission(Color color)
        {
            if (itemRenderer == null) return;
            if (!itemRenderer.material.HasProperty("_EmissionColor")) return;
            itemRenderer.material.EnableKeyword("_EMISSION");
            itemRenderer.material.SetColor("_EmissionColor", color);
        }

        private void ClearEmission()
        {
            if (itemRenderer == null) return;
            if (!itemRenderer.material.HasProperty("_EmissionColor")) return;
            itemRenderer.material.DisableKeyword("_EMISSION");
            itemRenderer.material.SetColor("_EmissionColor", Color.black);
        }

        private Vector3 GetMouseWorldPosition()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return Vector3.positiveInfinity;

            Ray ray = mainCamera.ScreenPointToRay(UnityInput.mousePosition);
            if (dragPlane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);

            return Vector3.positiveInfinity;
        }

        private void OnDestroy()
        {
            LeanTween.cancel(gameObject);
        }
    }
}
