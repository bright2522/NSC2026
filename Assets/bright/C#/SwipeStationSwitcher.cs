using System.Collections.Generic;
using UnityEngine;

// ปัดซ้าย/ขวา เพื่อสลับสเตชัน (หั่น -> ทอด -> ต้ม) ในซีนเดียว
public class SwipeStationSwitcher : MonoBehaviour
{
    [Header("สเตชันแต่ละอย่าง (เรียงซ้าย -> ขวา)")]
    [Tooltip("เช่น 0=หั่น, 1=ทอด, 2=ต้ม")]
    public List<GameObject> stations = new List<GameObject>();

    public int startIndex = 0;

    [Header("การปัด")]
    [Tooltip("ระยะปัดขั้นต่ำ (พิกเซล) ถึงจะนับว่าปัด")]
    public float minSwipeDistance = 120f;
    [Tooltip("ปัดวนได้ไหม (ปัดซ้ายจากอันสุดท้ายกลับมาอันแรก)")]
    public bool loop = false;
    [Tooltip("เปิด/ปิดการปัด — ปิดตอนกำลังหั่นเพื่อไม่ให้ชนกับการลากมีด")]
    public bool swipeEnabled = true;

    private int current;
    private Vector2 startPos;
    private bool tracking;

    void Start()
    {
        current = Mathf.Clamp(startIndex, 0, stations.Count - 1);
        ShowOnly(current);
    }

    void Update()
    {
        if (!swipeEnabled) return;

        // --- จอสัมผัส ---
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) { startPos = t.position; tracking = true; }
            else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && tracking)
            {
                EndSwipe(t.position); tracking = false;
            }
        }
        // --- เมาส์ (ทดสอบใน Editor) ---
        else
        {
            if (Input.GetMouseButtonDown(0)) { startPos = Input.mousePosition; tracking = true; }
            else if (Input.GetMouseButtonUp(0) && tracking) { EndSwipe(Input.mousePosition); tracking = false; }
        }
    }

    void EndSwipe(Vector2 endPos)
    {
        Vector2 delta = endPos - startPos;

        if (Mathf.Abs(delta.x) < minSwipeDistance) return;    // ปัดสั้นไป ไม่นับ
        if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y)) return;  // เป็นการปัดแนวตั้งมากกว่า ไม่นับ

        if (delta.x < 0) Next();      // ปัดซ้าย -> ถัดไป (ทอด/ต้ม)
        else Previous();              // ปัดขวา -> ย้อนกลับ (หั่น)
    }

    // ไปสเตชันถัดไป (เรียกจากปุ่มลูกศรก็ได้)
    public void Next()
    {
        if (current < stations.Count - 1) current++;
        else if (loop) current = 0;
        else return;
        ShowOnly(current);
    }

    // ย้อนกลับสเตชันก่อนหน้า
    public void Previous()
    {
        if (current > 0) current--;
        else if (loop) current = stations.Count - 1;
        else return;
        ShowOnly(current);
    }

    // ไปสเตชันตาม index ตรง ๆ (เผื่อผูกปุ่ม)
    public void GoTo(int index)
    {
        if (index < 0 || index >= stations.Count) return;
        current = index;
        ShowOnly(current);
    }

    // เปิด/ปิดการปัด (ผูกกับ event ตอนเริ่ม/จบการหั่นได้)
    public void SetSwipeEnabled(bool value) => swipeEnabled = value;

    void ShowOnly(int index)
    {
        for (int i = 0; i < stations.Count; i++)
            if (stations[i] != null) stations[i].SetActive(i == index);
    }

    public int CurrentIndex => current;
}