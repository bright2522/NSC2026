using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingVolumeController : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;
    public TMP_Text volumePercentText;

    private void Start()
    {
        // ค่าเริ่มต้นเสียง = 0%
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = 0f;

        UpdateVolume(volumeSlider.value);

        // เวลาเลื่อน Slider ให้เรียกฟังก์ชัน UpdateVolume
        volumeSlider.onValueChanged.AddListener(UpdateVolume);
    }

    private void UpdateVolume(float value)
    {
        // ปรับเสียงทั้งเกม
        AudioListener.volume = value;

        // แปลง 0-1 เป็น 0-100%
        int percent = Mathf.RoundToInt(value * 100f);

        // แสดงตัวเลข
        volumePercentText.text = percent + "%";
    }
}