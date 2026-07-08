// สคริปต์นี้แปะไว้ที่ GarlicPrefab ทุกก้อน
using UnityEngine;

public class GarlicStirItem : MonoBehaviour
{
    private bool hasBeenStirred = false; // กันผู้เล่นแช่ตะหลิวไว้เฉยๆ ต้องเขี่ยขยับถึงจะได้แต้ม

    private void OnCollisionEnter(Collision collision)
    {
        // เช็คว่าวัตถุที่มาชนมี Tag ว่า "Spatula" (ตะหลิว) หรือไม่
        if (collision.gameObject.CompareTag("Spatula") && !hasBeenStirred)
        {
            hasBeenStirred = true;

            // 🍳 ส่งสัญญาณเพิ่มแต้มระบบหลังบ้านทีละ 1 แต้มต่อการเขี่ยโดนหนึ่งครั้ง
            if (StirFryManager.Instance != null)
            {
                StirFryManager.Instance.AddProgress(1f); 
            }

            GameplayScore.Instance?.AddScore(5);

            // 🎨 เปลี่ยนสีตัวกระเทียมให้ดูสุกเหลืองทองนวลอมทองแบบสมจริง
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.9f, 0.75f, 0.5f); 
            }
        }
    }

    // ทริคเสริม: ถ้าผัดจนกระจายหลุดออกจากกันแล้ว ให้พร้อมโดนผัดใหม่อีกรอบได้หลังจากผ่านไป 2 วินาที
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Spatula"))
        {
            Invoke(nameof(ResetStirStatus), 2.0f);
        }
    }

    void ResetStirStatus()
    {
        hasBeenStirred = false;
    }
}