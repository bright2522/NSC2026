using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pep.Minigames.Plating
{
    [RequireComponent(typeof(Collider))]
    public class PlateDropZone : MonoBehaviour
    {
        [SerializeField] private float snapGridSize = 0.5f;
        [SerializeField] private float snapRadius = 0.4f;
        [SerializeField] private float plateYOffset = 0.05f;
        [SerializeField] private bool allowDuplicateItemIds = false;
        [SerializeField] private bool showGizmos = true;

        [Header("Snap Indicator")]
        [SerializeField] private GameObject snapIndicatorPrefab;
        [SerializeField] private float indicatorScale = 0.35f;
        [SerializeField] private float indicatorHeightAbovePlate = 0.02f;

        public event Action<DraggablePlateItem> OnItemPlaced;
        public event Action<DraggablePlateItem> OnItemRemoved;
        public event Action OnPlateChanged;

        private readonly List<DraggablePlateItem> placedItems = new List<DraggablePlateItem>();
        private readonly List<Vector3> occupiedSnapPoints = new List<Vector3>();

        private GameObject snapIndicator;
        private Renderer indicatorRenderer;
        private int indicatorTweenId = -1;

        public IReadOnlyList<DraggablePlateItem> PlacedItems => placedItems;
        public int ItemCount => placedItems.Count;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;

            BuildSnapIndicator();
        }

        private void BuildSnapIndicator()
        {
            if (snapIndicatorPrefab != null)
            {
                snapIndicator = Instantiate(snapIndicatorPrefab, transform);
            }
            else
            {
                snapIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                snapIndicator.name = "SnapIndicator";
                Destroy(snapIndicator.GetComponent<Collider>());
                snapIndicator.transform.SetParent(transform);
                snapIndicator.transform.localScale = new Vector3(indicatorScale, 0.004f, indicatorScale);
            }

            indicatorRenderer = snapIndicator.GetComponent<Renderer>();
            if (indicatorRenderer != null)
                indicatorRenderer.material = BuildHoloMaterial();

            snapIndicator.SetActive(false);
        }

        private Material BuildHoloMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard");
            var mat = new Material(shader);

            bool isUrp = shader.name.Contains("Universal");

            if (isUrp)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetFloat("_AlphaClip", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;
            }
            else
            {
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            mat.color = new Color(0.02f, 0.02f, 0.04f, 0.45f);

            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0f, 0.05f, 0.12f, 1f));
            }

            return mat;
        }

        public void ShowSnapPreview(DraggablePlateItem item)
        {
            if (snapIndicator == null) return;
            snapIndicator.SetActive(true);

            if (indicatorTweenId != -1) LeanTween.cancel(indicatorTweenId);
            snapIndicator.transform.localScale = Vector3.zero;
            indicatorTweenId = LeanTween.scale(snapIndicator,
                new Vector3(indicatorScale, 0.004f, indicatorScale), 0.18f)
                .setEaseOutBack()
                .setOnComplete(StartHoloPulse).id;

            UpdateSnapPreview(item);
        }

        private void StartHoloPulse()
        {
            if (indicatorRenderer == null) return;
            LeanTween.value(snapIndicator, 0.2f, 0.6f, 0.7f)
                .setEaseInOutSine()
                .setLoopPingPong()
                .setOnUpdate((float a) =>
                {
                    if (indicatorRenderer == null) return;
                    Color c = indicatorRenderer.material.color;
                    c.a = a;
                    indicatorRenderer.material.color = c;
                });
        }

        public void UpdateSnapPreview(DraggablePlateItem item)
        {
            if (snapIndicator == null || !snapIndicator.activeSelf) return;

            Vector3 snapPos = ComputeSnapPosition(item);
            Vector3 indicatorPos = new Vector3(snapPos.x,
                transform.position.y + plateYOffset + indicatorHeightAbovePlate,
                snapPos.z);

            snapIndicator.transform.position = Vector3.Lerp(
                snapIndicator.transform.position, indicatorPos, Time.deltaTime * 18f);
        }

        public void HideSnapPreview()
        {
            if (snapIndicator == null) return;

            if (indicatorTweenId != -1) LeanTween.cancel(indicatorTweenId);
            indicatorTweenId = LeanTween.scale(snapIndicator, Vector3.zero, 0.12f)
                .setEaseInBack()
                .setOnComplete(() => snapIndicator.SetActive(false)).id;

        }

        public bool TryDrop(DraggablePlateItem item)
        {
            if (item == null || item.ItemData == null) return false;

            if (!IsInsideBounds(item.transform.position)) return false;

            if (!allowDuplicateItemIds && IsItemIdAlreadyPlaced(item.ItemData.ItemId)) return false;

            return true;
        }

        public Vector3 ComputeSnapPosition(DraggablePlateItem item)
        {
            Vector3 dropPos = item.transform.position;
            Vector3 platePos = transform.position;

            Vector3 local = dropPos - platePos;
            float snappedX = Mathf.Round(local.x / snapGridSize) * snapGridSize;
            float snappedZ = Mathf.Round(local.z / snapGridSize) * snapGridSize;

            Vector3 candidate = platePos + new Vector3(snappedX, plateYOffset, snappedZ);

            foreach (Vector3 occupied in occupiedSnapPoints)
            {
                if (Vector3.Distance(candidate, occupied) < snapRadius)
                {
                    candidate = FindNearestFreePoint(platePos, local);
                    break;
                }
            }

            return candidate;
        }

        public void RegisterItemLanded(DraggablePlateItem item, Vector3 landedPos)
        {
            Vector3 snapPoint = new Vector3(landedPos.x, plateYOffset + transform.position.y, landedPos.z);
            RegisterPlacement(item, snapPoint);
        }

        public void RemoveItem(DraggablePlateItem item)
        {
            if (!placedItems.Contains(item)) return;

            placedItems.Remove(item);
            occupiedSnapPoints.RemoveAll(p =>
                Vector3.Distance(p, item.transform.position) < snapRadius);

            OnItemRemoved?.Invoke(item);
            OnPlateChanged?.Invoke();
        }

        public void ClearPlate()
        {
            var copy = new List<DraggablePlateItem>(placedItems);
            foreach (var item in copy)
                item.RemoveFromPlate();

            placedItems.Clear();
            occupiedSnapPoints.Clear();
            OnPlateChanged?.Invoke();
        }

        public bool ContainsItemId(string itemId)
        {
            foreach (var item in placedItems)
                if (item.ItemData != null && item.ItemData.ItemId == itemId) return true;
            return false;
        }

        public List<string> GetPlacedIngredientIds()
        {
            var result = new List<string>();
            foreach (var item in placedItems)
                if (item.ItemData != null)
                    result.Add(item.ItemData.LinkedIngredientId);
            return result;
        }

        private void RegisterPlacement(DraggablePlateItem item, Vector3 snapPos)
        {
            placedItems.Add(item);
            occupiedSnapPoints.Add(snapPos);
            OnItemPlaced?.Invoke(item);
            OnPlateChanged?.Invoke();
        }

        private bool IsInsideBounds(Vector3 worldPos)
        {
            var col = GetComponent<Collider>();
            return col.bounds.Contains(worldPos);
        }

        private bool IsItemIdAlreadyPlaced(string itemId)
        {
            foreach (var item in placedItems)
                if (item.ItemData != null && item.ItemData.ItemId == itemId) return true;
            return false;
        }

        private Vector3 FindNearestFreePoint(Vector3 origin, Vector3 localHint)
        {
            float searchRadius = snapGridSize;
            int maxIterations = 16;

            for (int i = 0; i < maxIterations; i++)
            {
                float angle = i * (360f / maxIterations) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * searchRadius,
                    0f,
                    Mathf.Sin(angle) * searchRadius);

                Vector3 candidate = origin + new Vector3(
                    Mathf.Round((localHint.x + offset.x) / snapGridSize) * snapGridSize,
                    plateYOffset,
                    Mathf.Round((localHint.z + offset.z) / snapGridSize) * snapGridSize);

                bool occupied = false;
                foreach (Vector3 p in occupiedSnapPoints)
                {
                    if (Vector3.Distance(candidate, p) < snapRadius)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied) return candidate;
            }

            return origin + new Vector3(localHint.x, plateYOffset, localHint.z);
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;
            DrawZoneGizmo(false);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            DrawZoneGizmo(true);

            Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
            foreach (var pt in occupiedSnapPoints)
            {
                Gizmos.DrawCube(pt, Vector3.one * snapRadius * 0.4f);
                Gizmos.DrawWireCube(pt, Vector3.one * snapRadius * 0.4f);
            }
        }

        private void DrawZoneGizmo(bool selected)
        {
            Vector3 center = transform.position;
            Vector3 size = Vector3.one;

            var box = GetComponent<BoxCollider>();
            var sphere = GetComponent<SphereCollider>();

            if (box != null)
            {
                center = transform.TransformPoint(box.center);
                size = Vector3.Scale(box.size, transform.lossyScale);

                Gizmos.color = new Color(0f, 1f, 0.4f, selected ? 0.20f : 0.10f);
                Gizmos.DrawCube(center, size);

                Gizmos.color = new Color(0f, 1f, 0.4f, selected ? 1f : 0.55f);
                Gizmos.DrawWireCube(center, size);
            }
            else if (sphere != null)
            {
                center = transform.TransformPoint(sphere.center);
                float radius = sphere.radius * Mathf.Max(transform.lossyScale.x,
                    transform.lossyScale.y, transform.lossyScale.z);

                Gizmos.color = new Color(0f, 1f, 0.4f, selected ? 0.20f : 0.10f);
                Gizmos.DrawSphere(center, radius);

                Gizmos.color = new Color(0f, 1f, 0.4f, selected ? 1f : 0.55f);
                Gizmos.DrawWireSphere(center, radius);
            }
            else
            {
                Gizmos.color = new Color(0f, 1f, 0.4f, selected ? 1f : 0.55f);
                Gizmos.DrawWireCube(center, Vector3.one * 2f);
            }
        }
    }
}
