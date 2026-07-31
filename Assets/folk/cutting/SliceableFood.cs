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
    [Tooltip("ให้ชิ้นที่หั่นกระเด็นไหม — ปิด = ตกนิ่ง ๆ ไม่ดีด")]
    public bool launchPieces = false;
    [Tooltip("ระยะที่ชิ้นส่วนจะขยับหลบใบมีดทางซ้าย-ขวา")]
    public float spawnOffset = 0.005f;
    [Tooltip("ระยะที่ชิ้นส่วนจะยกตัวลอยเหนือเขียงนิดหน่อยในเฟรมแรกเพื่อแก้บั๊กฟิสิกส์ดีดตัว")]
    public float antiClipYOffset = 0.005f;
    [Tooltip("แรงผลักดีดออกไปทางซ้าย (เฉพาะชิ้นซ้าย)")]
    public float pushForce = 0.2f;
    [Tooltip("แรงดีดให้ลอยขึ้นฟ้า (เฉพาะชิ้นซ้าย)")]
    public float bounceUpForce = 0.1f;
    [Tooltip("แรงหมุนตอนกระเด็น")]
    public float torqueForce = 0.0f;
    [Tooltip("รอหั่นซ้ำได้หลังหั่นล่าสุด (วินาที) — กันชิ้นดีด/หั่นซ้ำทันที")]
    public float knifeSliceCooldown = 0.4f;

    private bool isSlicing;
    private float knifeSliceReadyTime;

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
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col.GetComponent<SliceableFood>() != null)
                continue;

            if (!col.TryGetComponent<SliceableTriggerRelay>(out var relay))
                relay = col.gameObject.AddComponent<SliceableTriggerRelay>();

            relay.Initialize(this);
        }
    }

    internal void OnKnifeHit(Collider knifeCollider)
    {
        if (isSlicing || Time.time < knifeSliceReadyTime || !knifeCollider.CompareTag("Knife"))
            return;

        if (!IsKnifeChopping(knifeCollider))
            return;

        isSlicing = true;
        Slice(knifeCollider.gameObject);
    }

    private static bool IsKnifeChopping(Collider knifeCollider)
    {
        if (!knifeCollider.TryGetComponent<KnifeMovement>(out var knife))
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

        for (int i = 0; i < parts.Count; i++)
        {
            MeshPart part = parts[i];
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

        // กันชิ้นบน-ล่างดันกันเอง
        IgnoreCollisions(upperGroup, lowerGroup);
        
        if (SliceProgress.Instance != null) SliceProgress.Instance.AddSlice();
        GameplayScore.Instance?.AddScore(10);

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
        MeshFilter[] filters = group.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i].sharedMesh != null)
                return true;
        }

        return false;
    }

    private GameObject CreateGroupRoot(string rootName)
    {
        GameObject group = new GameObject(rootName)
        {
            tag = "Sliceable"
        };

        Transform groupTransform = group.transform;
        groupTransform.SetPositionAndRotation(transform.position, transform.rotation);
        groupTransform.SetParent(transform.parent, true);
        SlicedFoodManager.RegisterPiece(group);

        return group;
    }

    private void OnDestroy()
    {
        SlicedFoodManager.UnregisterPiece(gameObject);
    }

    private bool HasRemainingMeshes()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < filters.Length; i++)
        {
            MeshFilter filter = filters[i];
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
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();

        for (int i = 0; i < filters.Length; i++)
        {
            MeshFilter filter = filters[i];
            if (filter.sharedMesh == null)
                continue;

            if (filter.TryGetComponent<MeshRenderer>(out var renderer))
            {
                parts.Add(new MeshPart(filter.gameObject, renderer));
            }
        }

        return parts;
    }

    private bool IsValidSize(GameObject obj)
    {
        if (obj == null) return false;

        if (!obj.TryGetComponent<MeshFilter>(out var meshFilter))
            return false;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null || mesh.vertexCount < 4)
            return false;

        Vector3 scaledSize = Vector3.Scale(mesh.bounds.size, meshFilter.transform.lossyScale);
        float volume = scaledSize.x * scaledSize.y * scaledSize.z;
        float minAxis = Mathf.Min(scaledSize.x, Mathf.Min(scaledSize.y, scaledSize.z));
        float maxAxis = Mathf.Max(scaledSize.x, Mathf.Max(scaledSize.y, scaledSize.z));

        return minAxis >= minAxisSize || volume >= minVolumeThreshold || maxAxis >= minAxisSize * 2f;
    }

    private void SetupGroupPhysics(GameObject groupRoot, bool isLeftPiece)
    {
        float directionX = isLeftPiece ? -1f : 1f;
        groupRoot.transform.position += new Vector3(spawnOffset * directionX, antiClipYOffset, 0f);

        MeshFilter[] filters = groupRoot.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < filters.Length; i++)
        {
            EnsureCollider(filters[i].gameObject);
        }

        Rigidbody rb = groupRoot.AddComponent<Rigidbody>();
        rb.mass = 5.0f;
        rb.isKinematic = false;
        
        // 🔒 LOCK SETTINGS: ล็อกการหมุนทั้งหมด ไม่ให้เอียงล้ม + ล็อกแกน Z ไม่ให้ไถลตกหน้า/หลังเขียง
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        
        // 🛑 เพิ่มแรงต้านให้หยุดนิ่งรวดเร็ว
        rb.linearDamping = 8f;
        rb.angularDamping = 10f;

        if (launchPieces)
        {
            if (isLeftPiece)
            {
                float forceX = pushForce * directionX;
                rb.AddForce(new Vector3(forceX, bounceUpForce, 0f), ForceMode.Impulse);
                if (torqueForce > 0f)
                {
                    rb.AddTorque(new Vector3(
                        Random.Range(-torqueForce, torqueForce),
                        Random.Range(-torqueForce, torqueForce),
                        Random.Range(-torqueForce, torqueForce)), ForceMode.Impulse);
                }
            }
            else
            {
                float forceX = (pushForce * 0.2f) * directionX;
                rb.AddForce(new Vector3(forceX, 0f, 0f), ForceMode.Impulse);
            }
        }

        SliceableFood sliceScript = groupRoot.AddComponent<SliceableFood>();
        sliceScript.minVolumeThreshold = minVolumeThreshold;
        sliceScript.minAxisSize = minAxisSize;
        sliceScript.launchPieces = launchPieces;
        sliceScript.spawnOffset = spawnOffset;
        sliceScript.antiClipYOffset = antiClipYOffset;
        sliceScript.pushForce = pushForce;
        sliceScript.bounceUpForce = bounceUpForce;
        sliceScript.torqueForce = torqueForce;
        sliceScript.knifeSliceCooldown = knifeSliceCooldown;
        sliceScript.knifeSliceReadyTime = Time.time + knifeSliceCooldown;
        sliceScript.isSlicing = false;
        sliceScript.SetupColliderRelays();
    }

    private static void IgnoreCollisions(GameObject a, GameObject b)
    {
        if (a == null || b == null) return;

        Collider[] colA = a.GetComponentsInChildren<Collider>();
        Collider[] colB = b.GetComponentsInChildren<Collider>();

        for (int i = 0; i < colA.Length; i++)
        {
            Collider ca = colA[i];
            if (ca == null) continue;

            for (int j = 0; j < colB.Length; j++)
            {
                Collider cb = colB[j];
                if (cb != null)
                    Physics.IgnoreCollision(ca, cb, true);
            }
        }
    }

    private static void EnsureCollider(GameObject piece)
    {
        if (!piece.TryGetComponent<MeshFilter>(out var filter) || filter.sharedMesh == null)
            return;

        if (!piece.TryGetComponent<MeshCollider>(out var meshCollider))
            meshCollider = piece.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = filter.sharedMesh;
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