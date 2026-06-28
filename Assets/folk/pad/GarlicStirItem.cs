// สคริปต์นี้แปะไว้ที่ GarlicPrefab ทุกก้อน
using UnityEngine;

public class GarlicStirItem : MonoBehaviour
{
    private bool hasBeenStirred = false; // กันผู้เล่นแช่ตะหลิวไว้เฉยๆ ต้องเขี่ยขยับถึงจะได้แต้ม

    private void OnCollisionEnter(Collision collision)
    {
        // เช็คว่าวัตถุที่มาชนมี Tag ว่า "Spatula" (ตะหลิว) หรือไม่
        // อย่าลืมไปตั้ง Tag ที่ตัวตะหลิวใน Unity ว่า Spatula ด้วยนะครับ
        if (collision.gameObject.CompareTag("Spatula") && !hasBeenStirred)
        {
            hasBeenStirred = true;

            // ส่งสัญญาณไปบอกตัวผู้จัดการระบบว่า "กระเทียมโดนผัดแล้วนะ!"
            if (StirFryManager.Instance != null)
            {
                StirFryManager.Instance.AddProgress(1); // เพิ่มทีละ 1 แต้มต่อก้อน
            }

            // เปลี่ยนสีตัวกระเทียมเล็กน้อยให้ดูสุกขึ้น (เลือกใส่หรือไม่ใส่ก็ได้ครับ)
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.9f, 0.75f, 0.5f); // เปลี่ยนเป็นสีเหลืองนวลอมทอง
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