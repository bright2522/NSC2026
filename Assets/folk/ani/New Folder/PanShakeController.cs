using UnityEngine;

public class PanShakeController : MonoBehaviour
{
    private Animator animator;
    public float shakeThreshold = 2.0f; // ค่าความแรงในการเขย่า ยิ่งเยอะยิ่งต้องเขย่าแรง
    private float cooldownTimer = 0f;

    void Start()
    {
        // ดึง Component Animator ที่อยู่บนตัวกระทะมาใช้งาน
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0)
            TriggerShake();
#endif

        if (Input.acceleration.sqrMagnitude >= shakeThreshold && cooldownTimer <= 0)
            TriggerShake();
    }

    public void TriggerShake()
    {
        if (animator == null) return;

        animator.SetTrigger("Shake");
        cooldownTimer = 0.6f;
    }
}