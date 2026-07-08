using System.Collections.Generic;
using UnityEngine;

public class PanShakeController : MonoBehaviour
{
    [Header("Rig ที่เขย่า (ใส่ root ที่มี mesh + collider กระทะ)")]
    [Tooltip("วัตถุดิบควรอยู่ใต้ transform นี้ หรืออยู่ในรัศมี panCenter")]
    public Transform shakeTarget;

    [Header("จุดกลางกระทะ (หาไส้กรอก/แครอทที่หลุดออกมาในโลก)")]
    public Transform panCenter;
    public float ingredientCaptureRadius = 1.2f;
    public LayerMask ingredientLayers = ~0;
    public bool includeChildRigidbodies = true;
    public bool includeNearbyRigidbodies = true;

    [Header("การเขย่า (LeanTween)")]
    public float shakeDuration = 1.15f;
    public float maxRotationZ = 12f;
    public float maxBobY = 0.28f;
    public float maxSlideX = 0.1f;
    public float cooldown = 0.65f;

    [Header("Input อิสระ (ปิดไว้ถ้าให้ SpatulaController เป็นคนสั่ง)")]
    public bool listenForInput = false;
    public KeyCode keyboardKey = KeyCode.W;
    public float shakeThreshold = 2f;

    private class LockedIngredient
    {
        public Rigidbody rigidbody;
        public bool wasKinematic;
        public bool usedGravity;
        public Transform originalParent;
        public bool wasReparented;
    }

    private readonly List<LockedIngredient> lockedIngredients = new List<LockedIngredient>();
    private Vector3 baseLocalPos;
    private Vector3 baseLocalEuler;
    private float cooldownTimer;
    private bool isShaking;

    public float ShakeThreshold => shakeThreshold;
    public bool IsShaking => isShaking;

    void Awake()
    {
        if (shakeTarget == null) shakeTarget = transform;
        if (panCenter == null) panCenter = shakeTarget;
        CacheBasePose();
    }

    void OnDisable()
    {
        StopShakeTween();
        UnlockIngredients();
        ResetPose();
        isShaking = false;
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (!listenForInput || isShaking || cooldownTimer > 0f) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(keyboardKey))
            TriggerShake();
#endif

        if (Input.acceleration.sqrMagnitude >= shakeThreshold)
            TriggerShake();
    }

    public void CacheBasePose()
    {
        if (shakeTarget == null) return;
        baseLocalPos = shakeTarget.localPosition;
        baseLocalEuler = shakeTarget.localEulerAngles;
    }

    public bool TriggerShake()
    {
        if (isShaking || cooldownTimer > 0f || shakeTarget == null) return false;

        StopShakeTween();
        LockIngredients();

        isShaking = true;
        cooldownTimer = cooldown;

        shakeTarget.localPosition = baseLocalPos;
        shakeTarget.localEulerAngles = baseLocalEuler;

        GameObject target = shakeTarget.gameObject;
        float dur = shakeDuration;

        LeanTween.moveLocalY(target, baseLocalPos.y + maxBobY, dur * 0.38f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.moveLocalX(target, baseLocalPos.x - maxSlideX, dur * 0.38f).setEase(LeanTweenType.easeOutQuad);

        var seq = LeanTween.sequence();
        seq.append(LeanTween.rotateLocal(target, baseLocalEuler + new Vector3(0f, 0f, -maxRotationZ), dur * 0.35f).setEase(LeanTweenType.easeOutQuad));
        seq.append(LeanTween.rotateLocal(target, baseLocalEuler + new Vector3(0f, 0f, maxRotationZ * 1.15f), dur * 0.32f).setEase(LeanTweenType.easeInOutQuad));
        seq.append(LeanTween.rotateLocal(target, baseLocalEuler + new Vector3(0f, 0f, -maxRotationZ * 0.48f), dur * 0.18f).setEase(LeanTweenType.easeInOutSine));
        seq.append(LeanTween.rotateLocal(target, baseLocalEuler, dur * 0.25f).setEase(LeanTweenType.easeOutBack));
        seq.append(() =>
        {
            LeanTween.moveLocal(target, baseLocalPos, dur * 0.25f).setEase(LeanTweenType.easeOutBack);
        });
        seq.append(dur * 0.25f);
        seq.append(FinishShake);

        return true;
    }

    void LockIngredients()
    {
        lockedIngredients.Clear();
        Transform followRoot = shakeTarget;

        if (includeChildRigidbodies)
        {
            Rigidbody[] childBodies = shakeTarget.GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < childBodies.Length; i++)
                TryLockIngredient(childBodies[i], followRoot);
        }

        if (!includeNearbyRigidbodies || panCenter == null) return;

        Collider[] hits = Physics.OverlapSphere(
            panCenter.position,
            ingredientCaptureRadius,
            ingredientLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Rigidbody rb = hits[i].attachedRigidbody;
            if (rb != null)
                TryLockIngredient(rb, followRoot);
        }
    }

    void TryLockIngredient(Rigidbody rb, Transform followRoot)
    {
        if (rb == null || IsAlreadyLocked(rb)) return;
        if (rb.CompareTag("Spatula")) return;

        var entry = new LockedIngredient
        {
            rigidbody = rb,
            wasKinematic = rb.isKinematic,
            usedGravity = rb.useGravity,
            originalParent = rb.transform.parent,
            wasReparented = false
        };

        if (!rb.transform.IsChildOf(followRoot))
        {
            rb.transform.SetParent(followRoot, true);
            entry.wasReparented = true;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        lockedIngredients.Add(entry);
    }

    bool IsAlreadyLocked(Rigidbody rb)
    {
        for (int i = 0; i < lockedIngredients.Count; i++)
        {
            if (lockedIngredients[i].rigidbody == rb)
                return true;
        }

        return false;
    }

    void UnlockIngredients()
    {
        for (int i = 0; i < lockedIngredients.Count; i++)
        {
            LockedIngredient entry = lockedIngredients[i];
            Rigidbody rb = entry.rigidbody;
            if (rb == null) continue;

            if (entry.wasReparented)
                rb.transform.SetParent(entry.originalParent, true);

            rb.isKinematic = entry.wasKinematic;
            rb.useGravity = entry.usedGravity;
        }

        lockedIngredients.Clear();
    }

    void FinishShake()
    {
        ResetPose();
        UnlockIngredients();
        isShaking = false;
    }

    void StopShakeTween()
    {
        if (shakeTarget != null)
            LeanTween.cancel(shakeTarget.gameObject);
    }

    void ResetPose()
    {
        if (shakeTarget == null) return;
        shakeTarget.localPosition = baseLocalPos;
        shakeTarget.localEulerAngles = baseLocalEuler;
    }

    void OnDrawGizmosSelected()
    {
        if (panCenter == null) return;
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(panCenter.position, ingredientCaptureRadius);
    }
}
