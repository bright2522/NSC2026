using UnityEngine;

public class StirFryScrambleController : MonoBehaviour
{
    [Header("ชื่อของ Object ไข่ข้นในหน้า Hierarchy")]
    public string scrambledEggObjectName = "New_Scrambled_Egg";

    [Header("ตั้งค่าระบบการคนกระทะ")]
    public float totalStirProgressNeeded = 100f; 
    public float stirSensitivity = 5f;          
    public string spatulaTag = "Spatula";        

    private GameObject scrambledEggModel;
    private float currentProgress = 0f;
    private bool isFinished = false;

    private Material friedEggMat;
    private Material scrambledEggMat;

    private Vector3 lastSpatulaPos;
    private bool hasHitPan = false;

    // อาร์เรย์เก็บค่า Material ของวัตถุดิบอื่น ๆ ในกระทะเพื่อนำมาทำ Fade Out
    private GarlicStirItem[] sceneGarlics;
    private SausageItemController[] sceneSausages;
    private GameObject[] sceneCarrots;

    void Start()
    {
        // 1. ดึง Material ของตัวไข่ดาวเอง
        Renderer friedRenderer = GetComponentInChildren<Renderer>();
        if (friedRenderer != null)
        {
            friedEggMat = friedRenderer.material;
        }

        // 2. ค้นหาโมเดลไข่ข้น
        scrambledEggModel = GameObject.Find(scrambledEggObjectName);
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
        }

        // 🔍 ค้นหาและบันทึกวัตถุดิบอื่น ๆ ที่อยู่ในกระทะ ณ ตอนเริ่มผัดไข่ทันที
        FetchAllIngredientsInScene();
    }

    // ฟังก์ชันรวบรวมวัตถุดิบทั้งหมดในฉาก ณ ตอนเริ่มผัด
    void FetchAllIngredientsInScene()
    {
        sceneGarlics = FindObjectsByType<GarlicStirItem>(FindObjectsSortMode.None);
        sceneSausages = FindObjectsByType<SausageItemController>(FindObjectsSortMode.None);
        
        // หาแครอทจากชื่อวัตถุ
        System.Collections.Generic.List<GameObject> carrotList = new System.Collections.Generic.List<GameObject>();
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.name.ToLower().Contains("carrot"))
            {
                carrotList.Add(obj);
            }
        }
        sceneCarrots = carrotList.ToArray();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasHitPan && (collision.gameObject.name.Contains("Pan") || collision.gameObject.name.Contains("pan")))
        {
            hasHitPan = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(spatulaTag) || other.name.Contains("Spatula") || other.name.Contains("spatula"))
        {
            if (isFinished || scrambledEggMat == null) return;

            Vector3 currentSpatulaPos = other.transform.position;
            float stirDistance = Vector3.Distance(currentSpatulaPos, lastSpatulaPos);

            if (stirDistance > 0.01f && stirDistance < 2f) 
            {
                currentProgress += stirDistance * stirSensitivity;
                currentProgress = Mathf.Clamp(currentProgress, 0f, totalStirProgressNeeded);

                float ratio = currentProgress / totalStirProgressNeeded;

                // 🍳 1. จัดการระบบไข่
                if (friedEggMat != null) SetMaterialAlpha(friedEggMat, 1f - ratio); // ไข่ดาวค่อยๆ จางหาย
                SetMaterialAlpha(scrambledEggMat, ratio);                           // ไข่ข้นค่อยๆ ชัดขึ้น

                // 🧄 🌭 🥕 2. จัดการให้วัตถุดิบอื่น ๆ ค่อย ๆ เลือนหายไปตามการคน (Alpha ลดลงจาก 1 ไป 0)
                FadeOtherIngredients(1f - ratio);

                Debug.Log($"🍳 กำลังผัดไข่และหลอมรวมวัตถุดิบ... Progress: {ratio * 100f:F1}%");

                if (currentProgress >= totalStirProgressNeeded)
                {
                    FinishScrambling();
                }
            }

            lastSpatulaPos = currentSpatulaPos;
        }
    }

    // ฟังก์ชันสั่งปรับค่า Alpha ให้กับวัตถุดิบอื่น ๆ ทั้งหมดที่บันทึกไว้
    void FadeOtherIngredients(float alphaTarget)
    {
        // เลือนหายกระเทียม
        foreach (var garlic in sceneGarlics)
        {
            if (garlic != null)
            {
                Renderer r = garlic.GetComponentInChildren<Renderer>();
                if (r != null) SetMaterialAlpha(r.material, alphaTarget);
            }
        }

        // เลือนหายไส้กรอก
        foreach (var sausage in sceneSausages)
        {
            if (sausage != null)
            {
                Renderer r = sausage.GetComponentInChildren<Renderer>();
                if (r != null) SetMaterialAlpha(r.material, alphaTarget);
            }
        }

        // เลือนหายแครอท
        foreach (var carrot in sceneCarrots)
        {
            if (carrot != null)
            {
                Renderer r = carrot.GetComponentInChildren<Renderer>();
                if (r != null) SetMaterialAlpha(r.material, alphaTarget);
            }
        }
    }

    void FinishScrambling()
    {
        isFinished = true;
        Debug.Log("🎉 ผัดไข่ข้นเสร็จเรียบร้อยแล้ว วัตถุดิบอื่นเลือนหายสมบูรณ์!");

        if (scrambledEggMat != null) 
        {
            SetMaterialAlpha(scrambledEggMat, 1f);
        }

        // เมื่อจางจนมองไม่เห็นแล้ว สั่งทำลาย Object ทิ้งหลังบ้านเพื่อเคลียร์ขยะฟิสิกส์
        DestroyAllLoggedIngredients();
        
        Destroy(gameObject);
    }

    // ฟังก์ชันเคลียร์ขยะ Object ทิ้งอย่างถาวร
    void DestroyAllLoggedIngredients()
    {
        foreach (var g in sceneGarlics) { if (g != null) Destroy(g.gameObject); }
        foreach (var s in sceneSausages) { if (s != null) Destroy(s.gameObject); }
        foreach (var c in sceneCarrots) { if (c != null) Destroy(c); }
    }

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