using UnityEngine;

public class StirFryScrambleController : MonoBehaviour
{
    [Header("ชื่อของ Object ไข่ข้นในหน้า Hierarchy (พิมพ์ให้ตรงตัวพิมพ์เล็ก-ใหญ่)")]
    public string scrambledEggObjectName = "New_Scrambled_Egg"; // ใส่ชื่อ Object ไข่ข้นที่อยู่ในฉากหลักตรงนี้

    [Header("ตั้งค่าระบบการคนกระทะ")]
    public float totalStirProgressNeeded = 100f; // คะแนนสะสมการคนจนกว่าจะสุกสำเร็จ
    public float stirSensitivity = 5f;          // ความไวต่อการลากตะหลิวมาโดน
    public string spatulaTag = "Spatula";        // แท็กของโมเดลตะหลิวเพื่อใช้ตรวจจับ

    private GameObject scrambledEggModel;
    private float currentProgress = 0f;
    private bool isFinished = false;

    private Material friedEggMat;
    private Material scrambledEggMat;

    private Vector3 lastSpatulaPos;
    private bool hasHitPan = false;

    void Start()
    {
        // 1. ดึง Material ของตัวไข่ดาวเองมาควบคุมค่า Alpha (ความโปร่งแสง)
        Renderer friedRenderer = GetComponentInChildren<Renderer>();
        if (friedRenderer != null)
        {
            friedEggMat = friedRenderer.material;
        }

        // 2. ค้นหาโมเดลไข่ข้นที่วางรออยู่ในฉากตามชื่อที่ตั้งไว้
        scrambledEggModel = GameObject.Find(scrambledEggObjectName);

        // เซฟตี้เผื่อค้นหาชื่อแรกไม่เจอ ลองหาชื่อสั้น ๆ ดูอีกที
        if (scrambledEggModel == null) 
        {
            scrambledEggModel = GameObject.Find("ScrambledEgg");
        }

        if (scrambledEggModel != null)
        {
            Renderer scrambledRenderer = scrambledEggModel.GetComponentInChildren<Renderer>();
            if (scrambledRenderer != null)
            {
                scrambledEggMat = scrambledRenderer.material;
            }
            
            Debug.Log($"🍳 StirFryScramble: ผูกวัตถุ '{scrambledEggModel.name}' สำเร็จ!");
        }
        else
        {
            Debug.LogError($"❌ หาวัตถุไข่ข้นชื่อ '{scrambledEggObjectName}' ในฉากไม่เจอ! รบกวนตรวจสอบการพิมพ์ชื่อในช่อง Inspector ของ Prefab อีกทีนะครับ");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // เมื่อไข่ดาวตกกระทบพื้นกระทะ ให้หยุดแรงฟิสิกส์เพื่อไม่ให้ขยับหนีเวลาโดนคน
        if (!hasHitPan && (collision.gameObject.name.Contains("Pan") || collision.gameObject.name.Contains("pan")))
        {
            hasHitPan = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        // ตรวจจับเมื่อตะหลิว (ที่มี Collider แบบ Is Trigger) ลากเข้ามาบดคนเนื้อไข่
        if (other.CompareTag(spatulaTag) || other.name.Contains("Spatula") || other.name.Contains("spatula"))
        {
            if (isFinished || scrambledEggMat == null) return;

            // คำนวณระยะการขยับของตะหลิว ยิ่งผู้เล่นคนเมาส์ส่ายไปมามาก คะแนนยิ่งขึ้นไว
            Vector3 currentSpatulaPos = other.transform.position;
            float stirDistance = Vector3.Distance(currentSpatulaPos, lastSpatulaPos);

            // เช็คว่าตะหลิวมีการขยับเขยื้อนจริง ๆ ไม่ได้วางแช่ไว้เฉย ๆ
            if (stirDistance > 0.01f && stirDistance < 2f) 
            {
                currentProgress += stirDistance * stirSensitivity;
                currentProgress = Mathf.Clamp(currentProgress, 0f, totalStirProgressNeeded);

                // คำนวณอัตราส่วนการสุก (0.0 ถึง 1.0)
                float ratio = currentProgress / totalStirProgressNeeded;

                // ไข่ดาวปกติ: ค่อยๆ จางหายไป (Alpha ลดลงจาก 1 ไป 0)
                if (friedEggMat != null) 
                {
                    SetMaterialAlpha(friedEggMat, 1f - ratio);
                }

                // ไข่ข้นในฉาก: ค่อยๆ เลือนชัดขึ้นมา (Alpha เพิ่มขึ้นจาก 0 ไป 1)
                SetMaterialAlpha(scrambledEggMat, ratio);

                Debug.Log($"🍳 กำลังผัดไข่ขยี้... Progress: {ratio * 100f:F1}%");

                // เมื่อผัดจนได้คะแนนเต็มและไข่ข้นชัดเจน 100%
                if (currentProgress >= totalStirProgressNeeded)
                {
                    FinishScrambling();
                }
            }

            lastSpatulaPos = currentSpatulaPos;
        }
    }

    void FinishScrambling()
    {
        isFinished = true;
        Debug.Log("🎉 ผัดไข่ข้นเสร็จเรียบร้อยแล้ว!");

        // บังคับล็อกความชัดเจนของเนื้อไข่ข้นให้ทึบแสง 100% สมบูรณ์แบบ
        if (scrambledEggMat != null) 
        {
            SetMaterialAlpha(scrambledEggMat, 1f);
        }
        
        // ทำลายโครงซากของไข่ดาวก้อนเดิมทิ้ง เพื่อเคลียร์หน่วยความจำเครื่อง
        Destroy(gameObject);
    }

    // ฟังก์ชันส่วนตัวช่วยปรับค่าความจาง-ชัด (Alpha) ของ Material รองรับทั้ง Standard และ URP Shader
    void SetMaterialAlpha(Material mat, float alphaValue)
    {
        if (mat == null) return;

        if (mat.HasProperty("_Color"))
        {
            Color c = mat.color;
            c.a = alphaValue;
            mat.color = c;
        }
        else if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");
            c.a = alphaValue;
            mat.SetColor("_BaseColor", c);
        }
    }
}