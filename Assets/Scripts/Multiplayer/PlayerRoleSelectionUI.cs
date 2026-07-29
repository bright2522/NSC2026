#if CMPSETUP_COMPLETE
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Fusion;

public class PlayerRoleSelectionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject roleSelectionPanel;
    [SerializeField] private GameObject backgroundAtStart;
    [SerializeField] private GameObject playerList;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button roleButtonPrefab;

    private readonly List<GameObject> spawnedButtons = new List<GameObject>();
    private MultiplayerGameManager.GamePhase lastKnownPhase = (MultiplayerGameManager.GamePhase)(-1);
    private bool hasAlreadyPicked;

    private void Start()
    {
        if (roleSelectionPanel != null)
            roleSelectionPanel.SetActive(false);
        if (playerList != null)
            playerList.SetActive(false);
    }

    private void Update()
    {
        if (!MultiplayerGameManager.IsSpawnedReady)
            return;

        MultiplayerGameManager.GamePhase currentPhase = MultiplayerGameManager.Instance.Phase;

        if (currentPhase != lastKnownPhase)
        {
            lastKnownPhase = currentPhase;
            HandlePhaseChanged(currentPhase);
        }

        if (currentPhase == MultiplayerGameManager.GamePhase.RoleSelection && !hasAlreadyPicked)
        {
            if (PlayerData.LocalPlayer != null && PlayerData.LocalPlayer.RoleID != -1)
            {
                hasAlreadyPicked = true;
                HidePanels();
            }
        }
    }

    private void HandlePhaseChanged(MultiplayerGameManager.GamePhase phase)
    {
        bool shouldShow = phase == MultiplayerGameManager.GamePhase.RoleSelection;

        if (shouldShow)
        {
            hasAlreadyPicked = PlayerData.LocalPlayer != null && PlayerData.LocalPlayer.RoleID != -1;
            BuildRoleButtons();
        }
        else
        {
            ClearRoleButtons();
        }

        if (roleSelectionPanel != null)
            roleSelectionPanel.SetActive(shouldShow && !hasAlreadyPicked);

        if (playerList != null)
            playerList.SetActive(shouldShow && !hasAlreadyPicked);
    }

    private void HidePanels()
    {
        if (roleSelectionPanel != null)
            roleSelectionPanel.SetActive(false);
        if (backgroundAtStart != null)
            backgroundAtStart.SetActive(false);
        if (playerList != null)
            playerList.SetActive(false);
    }

    private void BuildRoleButtons()
    {
        ClearRoleButtons();

        if (MultiplayerGameManager.Instance == null || MultiplayerGameManager.Instance.roles == null)
            return;
        if (roleButtonPrefab == null || buttonContainer == null)
            return;

        RoleData[] roles = MultiplayerGameManager.Instance.roles;

        for (int i = 0; i < roles.Length; i++)
        {
            int roleIndex = i;
            RoleData role = roles[i];
            if (role == null)
                continue;

            Button newButton = Instantiate(roleButtonPrefab, buttonContainer);
            spawnedButtons.Add(newButton.gameObject);

            RoleButton roleBtn = newButton.GetComponent<RoleButton>();
            if (roleBtn != null)
                roleBtn.Setup(roleIndex, role.roleName, OnRoleButtonClicked);
            else
            {
                TMP_Text label = newButton.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = role.roleName;
                newButton.onClick.RemoveAllListeners();
                newButton.onClick.AddListener(() => OnRoleButtonClicked(roleIndex));
            }
        }
    }

    private void ClearRoleButtons()
    {
        foreach (var buttonObj in spawnedButtons)
        {
            if (buttonObj != null)
                Destroy(buttonObj);
        }
        spawnedButtons.Clear();
    }

    private void OnRoleButtonClicked(int roleIndex)
    {
        if (MultiplayerGameManager.Instance == null || PlayerData.LocalPlayer == null)
            return;
        if (hasAlreadyPicked || PlayerData.LocalPlayer.RoleID != -1)
            return;

        PlayerRef localPlayerRef = PlayerData.LocalPlayer.Object.InputAuthority;
        MultiplayerGameManager.Instance.RPC_RequestSelectRole(localPlayerRef, roleIndex);

        hasAlreadyPicked = true;
        HidePanels();
    }
}
#endif
