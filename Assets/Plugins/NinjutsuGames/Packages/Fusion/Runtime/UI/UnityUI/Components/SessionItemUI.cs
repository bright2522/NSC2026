using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.UI;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [DisallowMultipleComponent]
    
    [AddComponentMenu("Game Creator/UI/Fusion/Session Item UI")]
    [Icon(RuntimePaths.GIZMOS + "GizmoSessionUI.png")]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/user-interface#session-item-ui")]
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_LAST_LATER)]
    public class SessionItemUI : MonoBehaviour
    {
        [SerializeField] private Graphic m_AlternateBackground;
        [SerializeField] private Button joinButton;
        [SerializeField] private FieldList fieldList;
        [SerializeField] private AuthenticationSettings authenticationSettings;
        
        public SessionInfo SessionInfo { get; private set; }

        public FieldItem GetField(int index) => fieldList.Get(index);
        
        public void RefreshUI(SessionInfo sessionInfo)
        {
            SessionInfo = sessionInfo;
            for(var i = 0; i < fieldList.Length; i++)
            {
                var field = fieldList.Get(i);
                field.Refresh(gameObject, sessionInfo);
            }
            joinButton.interactable = sessionInfo.IsOpen;
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(JoinSession);
        }

        private void JoinSession()
        {
            _ = NetworkManager.Instance.JoinSession(SessionInfo.Name, NetworkManager.RunnerLobby.LobbyInfo.Region, NetworkManager.LobbyGameMode, authenticationSettings.AuthValues);
        }

        public void SetAlternateBackground(bool b)
        {
            if(m_AlternateBackground) m_AlternateBackground.enabled = b;
        }
    }
}