#if CMPSETUP_COMPLETE
using AvocadoShark;
using Pep.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomBootstrap : MonoBehaviour
{
    [SerializeField] private FusionConnection fusionConnection;
    [SerializeField] private PlayerDataManager playerDataManager;

    private void Awake()
    {
        if (fusionConnection == null)
            fusionConnection = FindFirstObjectByType<FusionConnection>();

        if (fusionConnection == null)
        {
            var go = GameObject.Find("FusionConnectionManager") ?? new GameObject("FusionConnectionManager");
            fusionConnection = go.GetComponent<FusionConnection>();
            if (fusionConnection == null)
                fusionConnection = go.AddComponent<FusionConnection>();
        }

        var controller = GetComponent<CreateRoomController>();
        if (controller == null)
            controller = gameObject.AddComponent<CreateRoomController>();

        WireCreateRoomPanel(controller);
    }

    private void WireCreateRoomPanel(CreateRoomController controller)
    {
        var panel = GameObject.Find("CreateRoomPanel");
        if (panel == null)
            return;

        TMP_Text playerName = panel.transform.Find("PlayerNameText")?.GetComponent<TMP_Text>();
        TMP_Text status = panel.transform.Find("StatusText_CMP")?.GetComponent<TMP_Text>();
        TMP_InputField roomName = panel.transform.Find("RoomNameInput")?.GetComponent<TMP_InputField>();
        Slider maxSlider = panel.transform.Find("MaxPlayersSlider")?.GetComponent<Slider>();
        TMP_Text maxText = panel.transform.Find("MaxPlayersText")?.GetComponent<TMP_Text>();
        Button createBtn = panel.transform.Find("CreateRoomButton")?.GetComponent<Button>();

        controller.Configure(
            playerDataManager,
            playerName,
            roomName,
            maxSlider,
            maxText,
            createBtn,
            status,
            fusionConnection);
    }
}
#endif
