using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleButton : MonoBehaviour
{
    public int RoleID { get; private set; }

    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    public void Setup(int roleId, string roleName, System.Action<int> onClickCallback)
    {
        RoleID = roleId;

        if (label != null)
            label.text = roleName;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke(RoleID));
        }
    }
}
