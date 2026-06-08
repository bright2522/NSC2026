using UnityEngine;
using EzySlice;

public class SliceableFood : MonoBehaviour
{
    [Header("Material Settings")]
    public Material insideMaterial; 

    [Header("Slicing Optimization")]
    public float minVolumeThreshold = 0.005f; 

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

    private bool isSliced = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (isSliced) return;

        if (other.CompareTag("Knife"))
        {
            isSliced = true; 
            Slice(other.gameObject);
        }
    }

    private void Slice(GameObject knife)
    {
        Vector3 slicePosition = knife.transform.position;
        Vector3 sliceNormal = Vector3.right; 

        Material originalMaterial = GetComponent<MeshRenderer>()?.material;
        SlicedHull hull = gameObject.Slice(slicePosition, sliceNormal, insideMaterial);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(gameObject, originalMaterial);
            GameObject lowerHull = hull.CreateLowerHull(gameObject, originalMaterial);

            if (IsValidSize(upperHull) && IsValidSize(lowerHull))
            {
                // ชิ้นขวา (UpperHull): ปล่อยร่วงตามแรงโน้มถ่วงเฉยๆ ห้ามกระโดด
                SetupFreeBounceComponent(upperHull, isLeftPiece: false);

                // ชิ้นซ้าย (LowerHull): เปิดระบบกระโดดเด้งดึ๋ง
                SetupFreeBounceComponent(lowerHull, isLeftPiece: true);

                Destroy(gameObject);
            }
            else
            {
                if (upperHull != null) Destroy(upperHull);
                if (lowerHull != null) Destroy(lowerHull);
                isSliced = false;
            }
        }
        else
        {
            isSliced = false; 
        }
    }

    private bool IsValidSize(GameObject obj)
    {
        if (obj == null) return false;
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Vector3 size = meshFilter.sharedMesh.bounds.size;
            float volume = size.x * size.y * size.z;
            return volume >= minVolumeThreshold;
        }
        return false;
    }

    private void SetupFreeBounceComponent(GameObject obj, bool isLeftPiece)
    {
        // 🌟 [จุดแก้บั๊ก] ขยับจุดเกิดหลบตัวใบมีด (ซ้าย/ขวา) 
        // และบวกแกน Y ขึ้นมานิดหนึ่ง (antiClipYOffset) เพื่อหนีจากการจมเนื้อเขียงในเฟรมแรกสุด!
        float directionX = isLeftPiece ? -1f : 1f;
        obj.transform.position += new Vector3(spawnOffset * directionX, antiClipYOffset, 0f);

        MeshCollider collider = obj.AddComponent<MeshCollider>();
        collider.convex = true;
        
        // เพิ่มระยะขอบชนให้บางลงเล็กน้อยเพื่อไม่ให้เบียดกับสิ่งแวดล้อมง่ายเกินไป
        collider.contactOffset = 0.001f; 

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.mass = 2.0f; 
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.None; 

        // แรงหน่วงต้านอากาศเพื่อให้ผักตกลงพื้นแล้วนิ่งไว ไม่ไถลตกเขียง
        rb.linearDamping = 4f;     
        rb.angularDamping = 5f;    

        if (isLeftPiece)
        {
            // ชิ้นฝั่งซ้าย: ใส่แรงผลักไปทางซ้าย + แรงดีดขึ้นฟ้าเต็มระบบเพื่อให้มัน "กระโดด"
            float forceX = pushForce * directionX;
            float forceY = bounceUpForce;
            float forceZ = -0.15f; 
            
            rb.AddForce(new Vector3(forceX, forceY, forceZ), ForceMode.Impulse);

            float torqueX = Random.Range(-torqueForce, torqueForce);
            float torqueY = Random.Range(-torqueForce, torqueForce);
            float torqueZ = Random.Range(-torqueForce, torqueForce);
            rb.AddTorque(new Vector3(torqueX, torqueY, torqueZ), ForceMode.Impulse);
        }
        else
        {
            // ชิ้นฝั่งขวา: ห้ามกระโดด! ใส่แรงเคลียร์ออกจากใบมีดราบไปกับแกน X เบาๆ
            // การที่มันลอยขึ้นมาจากคำสั่งบรรทัดบนนิดเดียว มันจะหล่นลงมากระแทกเขียงดัง "แปะ" พอนิ่งๆ ครับ
            float forceX = (pushForce * 0.2f) * directionX; 
            rb.AddForce(new Vector3(forceX, 0f, 0f), ForceMode.Impulse);

            // ใส่แรงหมุนให้ชิ้นขวาเอียงล้มนิดหน่อยพอสวยงาม ไม่ให้หมุนคว้าง
            rb.AddTorque(new Vector3(0f, 0f, Random.Range(-torqueForce * 0.2f, torqueForce * 0.2f)), ForceMode.Impulse);
        }

        SliceableFood sliceScript = obj.AddComponent<SliceableFood>();
        sliceScript.insideMaterial = insideMaterial;
        sliceScript.minVolumeThreshold = minVolumeThreshold;
        sliceScript.spawnOffset = spawnOffset;
        sliceScript.antiClipYOffset = antiClipYOffset; // ส่งต่อค่าไปชิ้นถัดไป
        sliceScript.pushForce = pushForce;
        sliceScript.bounceUpForce = bounceUpForce;
        sliceScript.torqueForce = torqueForce;
        sliceScript.isSliced = false;

        obj.tag = "Sliceable"; 
    }
}