using System;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Pep.Minigames.Plating
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class DraggablePlateItem : MonoBehaviour
    {
        [Header("Drag")]
        [SerializeField] private float liftHeight = 0.6f;
        [SerializeField] private float liftAnimDuration = 0.18f;
        [SerializeField] private float pickupScaleMultiplier = 1.15f;
        [SerializeField] private float pickupAnimDuration = 0.14f;

        [Header("Physics Drop")]
        [SerializeField] private float landFreezeDelay = 0.15f;
        [SerializeField] private float landFreezeVelocityThreshold = 0.08f;

        [Header("Return")]
        [SerializeField] private float returnAnimDuration = 0.28f;

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
        private Vector3 originalScale;
        private Plane dragPlane;
        private bool isInteractable = true;
        private bool isLifting;
        private bool isWaitingToLand;
        private float landFreezeTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public void Initialise(PlateItemSO data, Vector3 trayPos, PlatingItemCatalogManager catalogManager)
        {
            ItemData = data;
            TrayPosition = trayPos;
            catalog = catalogManager;
            originalScale = transform.localScale;
            mainCamera = Camera.main;
            dragPlane = new Plane(Vector3.up, trayPos);
        }

        private void Update()
        {
            if (!isWaitingToLand) return;

            landFreezeTimer -= Time.deltaTime;
            if (landFreezeTimer > 0f) return;

            if (rb.velocity.magnitude < landFreezeVelocityThreshold)
                FreezeOnLand();
        }

        private void OnMouseDown()
        {
            if (!isInteractable || IsPlaced || isWaitingToLand) return;
            if (catalog != null && !catalog.CanPlaceMore(ItemData.ItemId)) return;
            BeginDrag();
        }

        private void OnMouseDrag()
        {
            if (!IsDragging || isLifting) return;

            Vector3 worldPos = GetMouseWorldPosition();
            if (worldPos != Vector3.positiveInfinity)
                transform.position = worldPos;
        }

        private void OnMouseUp()
        {
            if (!IsDragging) return;
            EndDrag();
        }

        private void BeginDrag()
        {
            IsDragging = true;
            isLifting = true;

            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            float targetY = transform.position.y + liftHeight;

            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale * pickupScaleMultiplier, pickupAnimDuration)
                .setEaseOutBack();
            LeanTween.moveY(gameObject, targetY, liftAnimDuration)
                .setEaseOutBack()
                .setOnComplete(() =>
                {
                    dragPlane = new Plane(Vector3.up, new Vector3(0f, targetY, 0f));
                    isLifting = false;
                });

            OnPickedUp?.Invoke(this);
        }

        private void EndDrag()
        {
            IsDragging = false;
            isLifting = false;
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
            LeanTween.scale(gameObject, originalScale, 0.12f).setEaseOutQuad();

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
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (pendingDropZone != null)
            {
                pendingDropZone.RegisterItemLanded(this, transform.position);
                pendingDropZone = null;
            }

            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale * 1.08f, 0.06f)
                .setEaseOutQuad()
                .setLoopPingPong(1);

            OnLanded?.Invoke(this);
        }

        public void ReturnToTray()
        {
            IsPlaced = false;
            isWaitingToLand = false;
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            LeanTween.cancel(gameObject);
            LeanTween.move(gameObject, TrayPosition, returnAnimDuration).setEaseOutElastic();
            LeanTween.scale(gameObject, originalScale, returnAnimDuration).setEaseOutElastic();

            LeanTween.rotateAroundLocal(gameObject, Vector3.forward, 10f, 0.1f)
                .setEaseShake()
                .setLoopPingPong(2);

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
            if (zone != null && IsDragging)
                currentDropZone = zone;
        }

        private void OnTriggerExit(Collider other)
        {
            var zone = other.GetComponent<PlateDropZone>();
            if (zone != null && currentDropZone == zone)
                currentDropZone = null;
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
