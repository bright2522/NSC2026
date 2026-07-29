#if CMPSETUP_COMPLETE
using Fusion;
using TMPro;
using UnityEngine;
using StarterAssets;

public class PlayerData : NetworkBehaviour
{
    [Networked] public int VoteChoice { get; set; }

    public static PlayerData LocalPlayer;

    [Networked] public bool IsReady { get; set; }

    [Networked, OnChangedRender(nameof(OnRoleIDChanged))]
    public int RoleID { get; set; } = -1;

    [Networked] public int WaitingIndex { get; set; }

    [Header("UI")]
    public TMP_Text statusText;
    public TMP_Text roleText;
    public TMP_Text timerText;
    public GameObject endGameUI;
    public GameObject playerCanvas;

    private GameObject currentRoleUI;

    [Header("Character Models")]
    public GameObject[] characterModels;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            LocalPlayer = this;
            if (playerCanvas != null)
                playerCanvas.SetActive(true);
        }
        else if (playerCanvas != null)
        {
            playerCanvas.SetActive(false);
        }

        if (Object.HasStateAuthority)
        {
            RoleID = -1;
            VoteChoice = 0;
        }
    }

    private void OnRoleIDChanged()
    {
        ApplyRoleVisual(RoleID);
    }

    public override void Render()
    {
        if (!MultiplayerGameManager.IsSpawnedReady)
            return;

        if (statusText != null)
        {
            statusText.text =
                $"Players : {MultiplayerGameManager.Instance.PlayerCount}\n" +
                $"Ready : {MultiplayerGameManager.Instance.ReadyCount}/{MultiplayerGameManager.Instance.PlayerCount}";
        }

        if (roleText != null && RoleID >= 0 && RoleID < MultiplayerGameManager.Instance.roles.Length)
            roleText.text = MultiplayerGameManager.Instance.roles[RoleID].roleName;

        if (timerText != null && MultiplayerGameManager.Instance.GameStarted)
        {
            float remain = MultiplayerGameManager.Instance.MatchTimer.RemainingTime(Runner) ?? 0;
            int min = Mathf.FloorToInt(remain / 60);
            int sec = Mathf.FloorToInt(remain % 60);
            timerText.text = $"{min:00}:{sec:00}";
        }
    }

    private void ApplyRoleVisual(int roleID)
    {
        if (MultiplayerGameManager.Instance == null || roleID < 0 || roleID >= MultiplayerGameManager.Instance.roles.Length)
            return;

        RoleData role = MultiplayerGameManager.Instance.roles[roleID];

        ThirdPersonController controller = GetComponent<ThirdPersonController>();
        if (controller != null)
        {
            controller.MoveSpeed = role.walkSpeed;
            controller.SprintSpeed = role.sprintSpeed;
        }

        if (characterModels != null && characterModels.Length > 0)
        {
            for (int i = 0; i < characterModels.Length; i++)
            {
                if (characterModels[i] != null)
                    characterModels[i].SetActive(false);
            }

            if (roleID < characterModels.Length && characterModels[roleID] != null)
            {
                GameObject activeModel = characterModels[roleID];
                activeModel.SetActive(true);
                activeModel.transform.SetSiblingIndex(0);

                Animator animator = GetComponent<Animator>();
                if (animator != null)
                    animator.Rebind();
            }
        }

        if (currentRoleUI != null)
            Destroy(currentRoleUI);

        if (role.roleUIPrefab != null && playerCanvas != null)
            currentRoleUI = Instantiate(role.roleUIPrefab, playerCanvas.transform, false);
    }

    public void Ready()
    {
        if (HasInputAuthority)
            RPC_SetReady();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetReady()
    {
        IsReady = true;
    }
}
#endif
