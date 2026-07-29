#if CMPSETUP_COMPLETE
using UnityEngine;
using UnityEngine.UI;

public class ReadyButtonUI : MonoBehaviour
{
    [SerializeField] private Button readyButton;

    private MultiplayerGameManager.GamePhase lastKnownPhase = (MultiplayerGameManager.GamePhase)(-1);

    public void Configure(Button button)
    {
        readyButton = button;
        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(OnReadyButtonClicked);
            readyButton.onClick.AddListener(OnReadyButtonClicked);
        }
    }

    private void Awake()
    {
        if (readyButton == null)
            readyButton = GetComponent<Button>();

        if (readyButton != null)
            readyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    private void Update()
    {
        if (!MultiplayerGameManager.IsSpawnedReady || readyButton == null)
            return;

        MultiplayerGameManager.GamePhase currentPhase = MultiplayerGameManager.Instance.Phase;
        if (currentPhase == lastKnownPhase)
            return;

        lastKnownPhase = currentPhase;

        bool inWaitingRoom = currentPhase == MultiplayerGameManager.GamePhase.WaitingRoom;
        readyButton.gameObject.SetActive(inWaitingRoom);

        if (inWaitingRoom)
            readyButton.interactable = PlayerData.LocalPlayer == null || !PlayerData.LocalPlayer.IsReady;
    }

    private void OnReadyButtonClicked()
    {
        if (PlayerData.LocalPlayer == null)
            return;

        PlayerData.LocalPlayer.Ready();
        readyButton.interactable = false;
    }
}
#endif
