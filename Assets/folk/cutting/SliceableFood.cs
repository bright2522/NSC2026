using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class SliceableFood : MonoBehaviour
{
    [Header("Slicing Optimization")]
    public float minVolumeThreshold = 0.0001f;
    [Tooltip("ชิ้นที่บางกว่านี้ (เมตร) จะถูกลบทิ้ง")]
    public float minAxisSize = 0.002f;

    [Header("Gentle Physics Settings")]
    [Tooltip("ระยะที่ชิ้นส่วนจะขยับหลบใบมีดทางซ้าย-ขวา")]
    public float spawnOffset = 0.015f;
    [Tooltip("ระยะที่ชิ้นส่วนจะยกตัวลอยเหนือเขียงนิดหน่อยในเฟรมแรกเพื่อแก้บั๊กฟิสิกส์ดีดตัว")]
    public float antiClipYOffset = 0.02f;
    [Tooltip("แรงผลักดีดออกไปทางซ้าย (เฉพาะชิ้นซ้าย)")]
    public float pushForce = 0.4f;
    [Tooltip("แรงดีดให้ลอยขึ้นฟ้า (เฉพาะชิ้นซ้าย)")]
    public float bounceUpForce = 0.5f;
    [Tooltip("แรงหมุนติ้วๆ ตอนกระเด็นให้ล้มคว่ำสมจริง")]
    public float torqueForce = 1.0f;

    private bool isSlicing;

    private void Awake()
    {
        if (!IsFoodRoot())
        {
            Destroy(this);
            return;
        }

        SetupColliderRelays();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnKnifeHit(other);
    }

    private bool IsFoodRoot()
    {
        Transform parent = transform.parent;
        while (parent != null)
        {
            if (parent.GetComponent<SliceableFood>() != null)
                return false;
            parent = parent.parent;
        }

        return true;
    }

    private void SetupColliderRelays()
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            if (col.GetComponent<SliceableFood>() != null)
                continue;

            SliceableTriggerRelay relay = col.gameObject.GetComponent<SliceableTriggerRelay>();
            if (relay == null)
                relay = col.gameObject.AddComponent<SliceableTriggerRelay>();
            relay.Initialize(this);
        }
    }

    internal void OnKnifeHit(Collider knifeCollider)
    {
        if (isSlicing || !knifeCollider.CompareTag("Knife"))
            return;

        if (!IsKnifeChopping(knifeCollider))
            return;

        isSlicing = true;
        Slice(knifeCollider.gameObject);
    }

    private static bool IsKnifeChopping(Collider knifeCollider)
    {
        KnifeMovement knife = knifeCollider.GetComponent<KnifeMovement>();
        if (knife == null)
            knife = knifeCollider.GetComponentInParent<KnifeMovement>();

        return knife != null && knife.IsSliceActive;
    }

    private void Slice(GameObject knife)
    {
        List<MeshPart> parts = CollectMeshParts(transform);
        if (parts.Count == 0)
        {
            Debug.LogWarning($"{name}: no MeshFilter/MeshRenderer found.", this);
            isSlicing = false;
            return;
        }

        Vector3 slicePosition = knife.transform.position;
        Vector3 sliceNormal = Vector3.right;
        bool anyPieceKept = false;
        HashSet<GameObject> sourcesToDestroy = new HashSet<GameObject>();

        GameObject upperGroup = CreateGroupRoot($"{name}_Upper");
        GameObject lowerGroup = CreateGroupRoot($"{name}_Lower");

        foreach (MeshPart part in parts)
        {
            Material crossSectionMat = part.Renderer.sharedMaterial;
            if (crossSectionMat == null)
                continue;

            SlicedHull hull = part.Owner.Slice(slicePosition, sliceNormal, crossSectionMat);
            if (hull == null)
                continue;

            GameObject upperHull = hull.CreateUpperHull(part.Owner, crossSectionMat);
            GameObject lowerHull = hull.CreateLowerHull(part.Owner, crossSectionMat);

            bool keptUpper = TryAddHullToGroup(upperHull, part.Owner.transform, upperGroup.transform);
            bool keptLower = TryAddHullToGroup(lowerHull, part.Owner.transform, lowerGroup.transform);

            if (keptUpper || keptLower)
            {
                anyPieceKept = true;
                sourcesToDestroy.Add(part.Owner);
            }
            else
            {
                if (upperHull != null) Destroy(upperHull);
                if (lowerHull != null) Destroy(lowerHull);
            }
        }

        if (!anyPieceKept)
        {
            Destroy(upperGroup);
            Destroy(lowerGroup);
            isSlicing = false;
            return;
        }

        FinalizeGroup(upperGroup, isLeftPiece: false);
        FinalizeGroup(lowerGroup, isLeftPiece: true);

        foreach (GameObject source in sourcesToDestroy)
        {
            if (source == null || source == gameObject)
                continue;

            Destroy(source);
        }

        if (sourcesToDestroy.Contains(gameObject) || !HasRemainingMeshes())
            Destroy(gameObject);
        else
            isSlicing = false;
    }

    private bool TryAddHullToGroup(GameObject hull, Transform sourceTransform, Transform groupParent)
    {
        if (hull == null || !IsValidSize(hull))
        {
            if (hull != null) Destroy(hull);
            return false;
        }

        AlignHullTransform(hull.transform, sourceTransform);
        hull.transform.SetParent(groupParent, true);
        return true;
    }

    private void FinalizeGroup(GameObject groupRoot, bool isLeftPiece)
    {
        if (!HasChildMeshes(groupRoot.transform))
        {
            Destroy(groupRoot);
            return;
        }

        SetupGroupPhysics(groupRoot, isLeftPiece);
    }

    private static bool HasChildMeshes(Transform group)
    {
        foreach (MeshFilter filter in group.GetComponentsInChildren<MeshFilter>())
        {
            if (filter.sharedMesh != null)
                return true;
        }

        return false;
    }

    private GameObject CreateGroupRoot(string rootName)
    {
        GameObject group = new GameObject(rootName);
        group.tag = "Sliceable";

        Transform groupTransform = group.transform;
        groupTransform.SetPositionAndRotation(transform.position, transform.rotation);
        groupTransform.SetParent(SlicedFoodManager.Container, true);

        return group;
    }

    private bool HasRemainingMeshes()
    {
        foreach (MeshFilter filter in GetComponentsInChildren<MeshFilter>())
        {
            if (filter.sharedMesh == null)
                continue;

            if (filter.GetComponent<MeshRenderer>() == null)
                continue;

            return true;
        }

        return false;
    }

    private static void AlignHullTransform(Transform hull, Transform source)
    {
        hull.SetParent(source.parent, false);
        hull.localPosition = source.localPosition;
        hull.localRotation = source.localRotation;
        hull.localScale = source.localScale;
    }

    private static List<MeshPart> CollectMeshParts(Transform root)
    {
        List<MeshPart> parts = new List<MeshPart>();

        foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
        {
            if (filter.sharedMesh == null)
                continue;

            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null)
                continue;

            parts.Add(new MeshPart(filter.gameObject, renderer));
        }

        return parts;
    }

    private bool IsValidSize(GameObject obj)
    {
        if (obj == null) return false;

        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
        if (mesh == null || mesh.vertexCount < 4)
            return false;

        Vector3 scaledSize = Vector3.Scale(mesh.bounds.size, meshFilter.transform.lossyScale);
        float volume = scaledSize.x * scaledSize.y * scaledSize.z;
        float minAxis = Mathf.Min(scaledSize.x, scaledSize.y, scaledSize.z);
        float maxAxis = Mathf.Max(scaledSize.x, scaledSize.y, scaledSize.z);

        return minAxis >= minAxisSize || volume >= minVolumeThreshold || maxAxis >= minAxisSize * 2f;
    }

    private void SetupGroupPhysics(GameObject groupRoot, bool isLeftPiece)
    {
        float directionX = isLeftPiece ? -1f : 1f;
        groupRoot.transform.position += new Vector3(spawnOffset * directionX, antiClipYOffset, 0f);

        foreach (MeshFilter filter in groupRoot.GetComponentsInChildren<MeshFilter>())
            EnsureCollider(filter.gameObject);

        Rigidbody rb = groupRoot.AddComponent<Rigidbody>();
        rb.mass = 2.0f;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.linearDamping = 4f;
        rb.angularDamping = 5f;

        if (isLeftPiece)
        {
            float forceX = pushForce * directionX;
            rb.AddForce(new Vector3(forceX, bounceUpForce, -0.15f), ForceMode.Impulse);
            rb.AddTorque(new Vector3(
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce)), ForceMode.Impulse);
        }
        else
        {
            float forceX = (pushForce * 0.2f) * directionX;
            rb.AddForce(new Vector3(forceX, 0f, 0f), ForceMode.Impulse);
            rb.AddTorque(new Vector3(0f, 0f, Random.Range(-torqueForce * 0.2f, torqueForce * 0.2f)), ForceMode.Impulse);
        }

        SliceableFood sliceScript = groupRoot.AddComponent<SliceableFood>();
        sliceScript.minVolumeThreshold = minVolumeThreshold;
        sliceScript.minAxisSize = minAxisSize;
        sliceScript.spawnOffset = spawnOffset;
        sliceScript.antiClipYOffset = antiClipYOffset;
        sliceScript.pushForce = pushForce;
        sliceScript.bounceUpForce = bounceUpForce;
        sliceScript.torqueForce = torqueForce;
        sliceScript.isSlicing = false;
        sliceScript.SetupColliderRelays();
    }

    private static void EnsureCollider(GameObject piece)
    {
        MeshFilter filter = piece.GetComponent<MeshFilter>();
        if (filter == null || filter.sharedMesh == null)
            return;

        MeshCollider meshCollider = piece.GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = piece.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = filter.mesh;
        meshCollider.convex = true;
        meshCollider.contactOffset = 0.001f;
    }

    private readonly struct MeshPart
    {
        public GameObject Owner { get; }
        public MeshRenderer Renderer { get; }

        public MeshPart(GameObject owner, MeshRenderer renderer)
        {
            Owner = owner;
            Renderer = renderer;
        }
    }

    private sealed class SliceableTriggerRelay : MonoBehaviour
    {
        private SliceableFood root;

        public void Initialize(SliceableFood foodRoot)
        {
            root = foodRoot;
        }

        private void OnTriggerEnter(Collider other)
        {
            root?.OnKnifeHit(other);
        }
    }
}
