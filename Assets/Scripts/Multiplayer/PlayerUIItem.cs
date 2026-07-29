#if CMPSETUP_COMPLETE
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AvocadoShark;

namespace AvocadoShark
{
    public class PlayerUIItem : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image statusIconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI roleText;

        [Header("Status Sprites")]
        [SerializeField] private Sprite healthySprite;
        [SerializeField] private Sprite injuredSprite;
        [SerializeField] private Sprite downedSprite;

        private PlayerStats targetPlayer;
        private PlayerData targetPlayerData;

        public void Configure(TextMeshProUGUI name, TextMeshProUGUI role)
        {
            nameText = name;
            roleText = role;
        }

        public void Setup(PlayerStats player)
        {
            targetPlayer = player;
            nameText.text = player.PlayerName.ToString();
            targetPlayerData = player.GetComponent<PlayerData>();
            targetPlayer.OnStatusChanged += UpdateUI;
            UpdateRoleUI();
            UpdateUI(targetPlayer.CurrentStatus);
        }

        private void Update()
        {
            UpdateRoleUI();
        }

        private void UpdateRoleUI()
        {
            if (roleText == null || targetPlayerData == null || MultiplayerGameManager.Instance == null)
                return;

            if (targetPlayerData.RoleID >= 0 &&
                targetPlayerData.RoleID < MultiplayerGameManager.Instance.roles.Length)
                roleText.text = MultiplayerGameManager.Instance.roles[targetPlayerData.RoleID].roleName;
            else
                roleText.text = "-";
        }

        private void UpdateUI(PlayerStatus status)
        {
            if (statusIconImage == null)
                return;

            switch (status)
            {
                case PlayerStatus.Healthy:
                    if (healthySprite != null) statusIconImage.sprite = healthySprite;
                    statusIconImage.color = Color.white;
                    break;
                case PlayerStatus.Injured:
                    if (injuredSprite != null) statusIconImage.sprite = injuredSprite;
                    break;
                case PlayerStatus.Downed:
                    if (downedSprite != null) statusIconImage.sprite = downedSprite;
                    statusIconImage.color = Color.red;
                    break;
            }

            UpdateRoleUI();
        }

        private void OnDestroy()
        {
            if (targetPlayer != null)
                targetPlayer.OnStatusChanged -= UpdateUI;
        }
    }
}
#endif
