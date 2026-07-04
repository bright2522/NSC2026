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

        // ดักจับแรงเหวี่ยงจากมือถือ
        if (Input.acceleration.sqrMagnitude >= shakeThreshold && cooldownTimer <= 0)
        {
            if (animator != null)
            {
                // สั่งให้ Trigger ที่เราตั้งชื่อไว้ว่า "Shake" ทำงานทำให้อนิเมชันในภาพเล่นทันที!
                animator.SetTrigger("Shake"); 
                cooldownTimer = 0.6f; // เล่นเสร็จแล้วรอ 0.6 วินาทีถึงจะเขย่ารอบต่อไปได้
            }
        }
    }
}