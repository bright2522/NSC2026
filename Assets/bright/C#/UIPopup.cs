using System.Collections;
using UnityEngine;

// แผง UI ที่เด้งขึ้น/ยุบลงแบบมีอนิเมชั่น
// ผูก Show/Hide/Toggle กับปุ่มได้เลย
public class UIPopup : MonoBehaviour
{
    [Header("แผงที่จะเด้ง (เว้นว่าง = ใช้ตัวเอง)")]
    public GameObject panel;

    [Header("อนิเมชั่น")]
    public float duration = 0.25f;      // ระยะเวลาเด้ง (วินาที)
    public float overshoot = 1.1f;      // เด้งเกินนิดหน่อยก่อนเข้าที่ (1 = ไม่เด้งเกิน)
    public bool startHidden = true;     // เริ่มเกมให้ซ่อนไว้

    private Coroutine anim;

    GameObject Target => panel != null ? panel : gameObject;

    void Start()
    {
        if (startHidden) Target.SetActive(false);
    }

    // เด้งขึ้น (ผูกกับปุ่มเปิด)
    public void Show()
    {
        Target.SetActive(true);
        Play(Vector3.zero, Vector3.one, false);
    }

    // ยุบลง (ผูกกับปุ่มปิด X)
    public void Hide()
    {
        if (!Target.activeSelf) return;
        Play(Target.transform.localScale, Vector3.zero, true);
    }

    // สลับเปิด/ปิด
    public void Toggle()
    {
        if (Target.activeSelf) Hide();
        else Show();
    }

    void Play(Vector3 from, Vector3 to, bool hideAtEnd)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Animate(from, to, hideAtEnd));
    }

    IEnumerator Animate(Vector3 from, Vector3 to, bool hideAtEnd)
    {
        Transform t = Target.transform;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime; // ใช้ unscaled เผื่อเกม pause อยู่
            float p = Mathf.Clamp01(time / duration);

            // ease out back = เด้งเกินนิดหน่อยตอนเปิด
            float eased = hideAtEnd ? EaseIn(p) : EaseOutBack(p, overshoot);

            t.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        t.localScale = to;
        if (hideAtEnd) Target.SetActive(false);
    }

    // เด้งเกินแล้วเข้าที่
    float EaseOutBack(float x, float over)
    {
        float c1 = 1.70158f * Mathf.Max(0.01f, (over - 1f) / 0.1f) * 0.1f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    float EaseIn(float x) => x * x;
}