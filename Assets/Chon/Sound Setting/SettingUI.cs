using UnityEngine;

public class SettingUI : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject settingButton;

    // เปิดหน้าตั้งค่า
    public void OpenSetting()
    {
        settingPanel.SetActive(true);
        settingButton.SetActive(false);
    }

    // ปิดหน้าตั้งค่า
    public void CloseSetting()
    {
        settingPanel.SetActive(false);
        settingButton.SetActive(true);
    }
}