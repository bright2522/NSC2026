#if CMPSETUP_COMPLETE
using AvocadoShark;
using Pep.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomController : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private TMP_Text playerNameDisplay;
    [SerializeField] private PlayerDataManager playerDataManager;

    [Header("Room Creation")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Toggle passwordToggle;
    [SerializeField] private Slider maxPlayersSlider;
    [SerializeField] private TMP_Text maxPlayersText;

    [Header("Buttons")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button refreshButton;

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;

    [Header("Panels")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomCreationPanel;

    private bool _connected;

    public void Configure(
        PlayerDataManager dataManager,
        TMP_Text playerNameDisplay,
        TMP_InputField roomNameInput,
        Slider maxPlayersSlider,
        TMP_Text maxPlayersText,
        Button createRoomButton,
        TMP_Text statusText,
        FusionConnection fusionConnectionOverride = null)
    {
        playerDataManager = dataManager;
        this.playerNameDisplay = playerNameDisplay;
        this.roomNameInput = roomNameInput;
        this.maxPlayersSlider = maxPlayersSlider;
        this.maxPlayersText = maxPlayersText;
        this.createRoomButton = createRoomButton;
        this.statusText = statusText;
    }

    private void Start()
    {
        SetupPlayerName();
        SetupUI();
        ConnectToPhoton();
    }

    private void SetupPlayerName()
    {
        string displayName = "Player";

        if (playerDataManager != null && playerDataManager.CurrentData != null)
        {
            string saved = playerDataManager.CurrentData.playerName;
            if (!string.IsNullOrWhiteSpace(saved))
                displayName = saved;
        }

        if (FusionConnection.Instance != null)
            FusionConnection.Instance._playerName = displayName;

        PlayerPrefs.SetString("MP_PLAYER_NAME", displayName);
        PlayerPrefs.Save();

        if (playerNameDisplay != null)
            playerNameDisplay.text = displayName;
    }

    private void SetupUI()
    {
        if (passwordToggle != null && passwordInput != null)
        {
            passwordInput.interactable = false;
            passwordToggle.isOn = false;
            passwordToggle.onValueChanged.AddListener(on => passwordInput.interactable = on);
        }

        if (maxPlayersSlider != null)
        {
            maxPlayersSlider.onValueChanged.AddListener(v =>
            {
                if (maxPlayersText != null)
                    maxPlayersText.text = Mathf.RoundToInt(v).ToString();
            });
            if (maxPlayersText != null)
                maxPlayersText.text = Mathf.RoundToInt(maxPlayersSlider.value).ToString();
        }

        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(OnCreateRoom);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefresh);
    }

    private void ConnectToPhoton()
    {
        if (FusionConnection.Instance == null)
        {
            SetStatus("ไม่พบ FusionConnection");
            return;
        }

        SetStatus("กำลังเชื่อมต่อ...");

        var fc = FusionConnection.Instance;
        fc._playerName = playerNameDisplay != null ? playerNameDisplay.text : "Player";

        if (fc.mainObject != null)
            fc.mainObject.SetActive(false);
        if (fc.characterselectionobject != null)
            fc.characterselectionobject.SetActive(false);

        fc.ConnectToRunner();
        _connected = true;
        SetStatus("เชื่อมต่อแล้ว รอรายการห้อง...");
    }

    public void OnCreateRoom()
    {
        if (FusionConnection.Instance == null || !_connected)
        {
            SetStatus("ยังไม่ได้เชื่อมต่อ");
            return;
        }

        string roomName = roomNameInput != null ? roomNameInput.text : "";
        if (string.IsNullOrWhiteSpace(roomName))
            roomName = "Room-" + Random.Range(1000, 9999);

        int maxPlayers = maxPlayersSlider != null ? Mathf.RoundToInt(maxPlayersSlider.value) : 4;

        string password = string.Empty;
        if (passwordToggle != null && passwordToggle.isOn && passwordInput != null)
            password = passwordInput.text;

        PlayerPrefs.SetInt("has_pass", string.IsNullOrEmpty(password) ? 0 : 1);

        SetStatus("กำลังสร้างห้อง...");

        if (FusionConnection.Instance.loadingScreenScript != null)
        {
            FusionConnection.Instance.loadingScreenScript.gameObject.SetActive(true);
        }

        FusionConnection.Instance.JoinRoom(roomName, maxPlayers, password);
    }

    public void OnRefresh()
    {
        if (FusionConnection.Instance != null)
            FusionConnection.Instance.RefreshRoomList();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
#endif
