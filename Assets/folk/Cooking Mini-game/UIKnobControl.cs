using UnityEngine;
using UnityEngine.EventSystems;

public class UIKnobControl : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public CookingSystem3D cookingSystem; 

    private float currentKnobAngle = 0f; // มุมสะสมของปุ่มหมุน (0 ถึง 270 องศา)
    private float previousPointerAngle;

    public void OnPointerDown(PointerEventData eventData)
    {
        // บันทึกมุมเริ่มต้นตอนที่เริ่มจิ้มนิ้วหรือคลิกเมาส์ครั้งแรก
        previousPointerAngle = GetPointerAngle(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (cookingSystem == null) return;

        // หามุมปัจจุบันที่เมาส์ลากไป
        float currentPointerAngle = GetPointerAngle(eventData);
        
        // คำนวณส่วนต่างของการลาก (Delta) ระหว่างเฟรม
        float deltaAngle = Mathf.DeltaAngle(previousPointerAngle, currentPointerAngle);
        previousPointerAngle = currentPointerAngle;

        // นำส่วนต่างมาคำนวณสะสม (กลับค่าลบเพื่อให้หมุนตามเข็มนาฬิกาแล้วค่าไฟเพิ่มขึ้น)
        currentKnobAngle -= deltaAngle;

        // ⭐ ล็อกขอบเขต: หมุนได้แค่ 0 ถึง 270 องศาเท่านั้น หมุนเกินจากนี้จะถูกบล็อกทันที
        currentKnobAngle = Mathf.Clamp(currentKnobAngle, 0f, 270f);

        // สั่งให้ตัวภาพปุ่มหมุน UI หมุนตามจริงในแกน Z
        transform.localRotation = Quaternion.Euler(0f, 0f, -currentKnobAngle);

        // แปลงมุมสะสม (0-270 องศา) ให้กลายเป็นเปอร์เซ็นต์ไฟเป้าหมาย (0-100%)
        float heatPercent = (currentKnobAngle / 270f) * 100f;
        
        // ส่งค่าไฟเป้าหมายไปให้ระบบหลักคอยคำนวณต่อ
        cookingSystem.targetHeat = heatPercent;
    }

    // ฟังก์ชันช่วยคำนวณมุมรอบจุดศูนย์กลางของปุ่มหมุน
    private float GetPointerAngle(PointerEventData eventData)
    {
        Vector2 centerPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, transform.position);
        Vector2 direction = eventData.position - centerPoint;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
}