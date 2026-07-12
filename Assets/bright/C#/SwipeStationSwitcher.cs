using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// สลับสเตชันแบบสไลด์: อันเก่าเลื่อนออก อันใหม่เลื่อนเข้ามาแทนตำแหน่งเดิม
// ทุกสเตชันวางไว้ตำแหน่งเดียวกัน (ตรงกลาง) ตอนสลับค่อยเลื่อน
public class StationSlideSwitcher : MonoBehaviour
{
    [Header("สเตชันทั้งหมด (เรียง หั่น/ทอด/ต้ม)")]
    public List<Transform> stations = new List<Transform>();

    [Header("ตำแหน่งกลาง (จุดที่สเตชันควรมาอยู่)")]
    [Tooltip("ระยะที่อันเก่าเลื่อนออกไปด้านข้าง (world units)")]
    public float slideOutDistance = 25f;
    [Tooltip("ความเร็วสไลด์")]
    public float slideSpeed = 10f;

    [Header("การปัด")]
    public float minSwipeDistance = 120f;
    public bool swipeEnabled = true;

    private int current = 0;
    private Vector3 centerPos;      // ตำแหน่งกลางที่สเตชันมาอยู่
    private bool animating;

    private Vector2 startPos;
    private bool tracking;

    void Start()
    {
        // ใช้ตำแหน่งของสเตชันแรกเป็นจุดกลาง
        centerPos = stations.Count > 0 ? stations[0].position : transform.position;

        // วางทุกอันไว้กลาง แล้วเปิดเฉพาะอันแรก ปิดที่เหลือ
        for (int i = 0; i < stations.Count; i++)
        {
            if (stations[i] == null) continue;
            stations[i].position = centerPos;
            stations[i].gameObject.SetActive(i == current);
        }
    }

    void Update()
    {
        if (!swipeEnabled || animating) return;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) { startPos = t.position; tracking = true; }
            else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && tracking)
            { EndSwipe(t.position); tracking = false; }
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) { startPos = Input.mousePosition; tracking = true; }
            else if (Input.GetMouseButtonUp(0) && tracking) { EndSwipe(Input.mousePosition); tracking = false; }
        }
    }

    void EndSwipe(Vector2 endPos)
    {
        Vector2 d = endPos - startPos;
        if (Mathf.Abs(d.x) < minSwipeDistance) return;
        if (Mathf.Abs(d.x) < Mathf.Abs(d.y)) return;

        if (d.x < 0) Next();      // ปัดซ้าย -> ถัดไป
        else Previous();          // ปัดขวา -> ย้อนกลับ
    }

    public void Next()
    {
        if (current >= stations.Count - 1) return;
        StartCoroutine(SlideTo(current + 1, fromRight: true));
    }

    public void Previous()
    {
        if (current <= 0) return;
        StartCoroutine(SlideTo(current - 1, fromRight: false));
    }

    // อันใหม่เลื่อนเข้ามาแทนอันเก่า
    IEnumerator SlideTo(int newIndex, bool fromRight)
    {
        animating = true;

        Transform oldS = stations[current];
        Transform newS = stations[newIndex];

        // อันใหม่เริ่มจากด้านข้าง (ขวาถ้าปัดซ้าย, ซ้ายถ้าปัดขวา)
        float side = fromRight ? 1f : -1f;
        Vector3 newStart = centerPos + Vector3.right * (slideOutDistance * side);
        Vector3 oldEnd   = centerPos + Vector3.right * (slideOutDistance * -side);

        newS.position = newStart;
        newS.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * slideSpeed;
            float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            if (oldS != null) oldS.position = Vector3.Lerp(centerPos, oldEnd, e);   // อันเก่าเลื่อนออก
            newS.position = Vector3.Lerp(newStart, centerPos, e);                    // อันใหม่เลื่อนเข้า
            yield return null;
        }

        // จบ: อันใหม่อยู่กลาง อันเก่าปิด+คืนตำแหน่ง
        newS.position = centerPos;
        if (oldS != null)
        {
            oldS.gameObject.SetActive(false);
            oldS.position = centerPos;
        }

        current = newIndex;
        animating = false;
    }

    public void SetSwipeEnabled(bool v) => swipeEnabled = v;
    public int CurrentIndex => current;
}