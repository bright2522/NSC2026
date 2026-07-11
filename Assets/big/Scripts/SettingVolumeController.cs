using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingVolumeController : MonoBehaviour
{
    [Header("UI")]
    public Slider volumeSlider;
    public TMP_Text volumePercentText;

    private const string VolumeKey = "MasterVolume";

    private void Start()
    {
        // ตั้งช่วงค่าเสียง
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;

        // โหลดค่าเสียงที่เคยบันทึกไว้
        // ถ้าไม่เคยมีค่า ให้เริ่มที่ 100
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        volumeSlider.value = savedVolume;
        ApplyVolume(savedVolume);

        // เชื่อม Slider กับฟังก์ชัน
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);

        // บันทึกค่าเสียง
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float value)
    {
        // ปรับเสียงทั้งเกม
        AudioListener.volume = value;

        // แสดงเป็นเปอร์เซ็นต์
        int percent = Mathf.RoundToInt(value * 100f);
        volumePercentText.text = percent + "%";
    }
}