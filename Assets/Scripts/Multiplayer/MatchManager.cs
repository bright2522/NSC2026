#if CMPSETUP_COMPLETE
using UnityEngine;
using Fusion;
using AvocadoShark;

namespace AvocadoShark
{
    public class MatchManager : NetworkBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Header("Game Over UI")]
        [SerializeField] private GameObject uiLost;
        [SerializeField] private GameObject uiWin;

        public bool GameStarted { get; private set; }
        public bool IsGameOver => isGameOver;
        private bool isGameOver;

        private void Awake()
        {
            Instance = this;
        }

        public void StartMatch()
        {
            GameStarted = true;
            isGameOver = false;
        }

        private void Update()
        {
            if (Object == null || !Object.HasStateAuthority || !GameStarted || isGameOver)
                return;

            CheckAllPlayersDowned();
        }

        private void CheckAllPlayersDowned()
        {
            if (SessionPlayers.instance == null || SessionPlayers.instance.activePlayers == null)
                return;

            int checkedPlayers = 0;
            foreach (var player in SessionPlayers.instance.activePlayers)
            {
                if (player == null || player.Object == null || !player.Object.IsValid)
                    continue;

                checkedPlayers++;
                if (player.CurrentStatus != PlayerStatus.Downed)
                    return;
            }

            if (checkedPlayers > 0)
                TriggerGameOverLost();
        }

        public void TriggerGameOverLost()
        {
            if (isGameOver)
                return;

            isGameOver = true;
            RPC_ShowLostUI();
        }

        public void TriggerGameOverWin()
        {
            if (Object == null || !Object.HasStateAuthority || isGameOver)
                return;

            isGameOver = true;
            RPC_ShowWinUI();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ShowLostUI()
        {
            isGameOver = true;
            if (uiLost != null) uiLost.SetActive(true);
            if (uiWin != null) uiWin.SetActive(false);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ShowWinUI()
        {
            isGameOver = true;
            if (uiWin != null) uiWin.SetActive(true);
            if (uiLost != null) uiLost.SetActive(false);
        }
    }
}
#endif
