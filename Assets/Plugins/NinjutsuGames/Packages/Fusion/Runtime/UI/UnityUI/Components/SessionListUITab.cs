using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [AddComponentMenu("Game Creator/UI/Fusion/SessionList UI Tab")]
    [Icon(RuntimePaths.GIZMOS + "GizmoSessionListUI.png")]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/user-interface#session-list-ui-tab")]
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_LAST_LATER)]
    
    public class SessionListUITab : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private SessionListUI m_SessionListUI;

        [SerializeField] private SortDirection m_SortDirection = SortDirection.Descending;
        [SerializeField] private int m_SortIndex = 0;
        [SerializeField] private GameObject m_ActiveIndex;
        [SerializeField] private GameObject m_DirectionArrow;
        
        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (!m_SessionListUI) return;
            m_SessionListUI.EventRefreshUI -= RefreshUI;
            m_SessionListUI.EventRefreshUI += RefreshUI;

            RefreshUI();
        }

        private void OnDisable()
        {
            if (!m_SessionListUI) return;
            m_SessionListUI.EventRefreshUI -= RefreshUI;
        }

        // CALLBACKS: -----------------------------------------------------------------------------
        
        public void OnPointerClick(PointerEventData data) => Filter();
        public void OnSubmit(BaseEventData data) => Filter();

        private void RefreshUI()
        {
            if (!m_SessionListUI) return;
            
            var currentFilter = m_SessionListUI.SortIndex;
            if(m_ActiveIndex) m_ActiveIndex.SetActive(m_SortIndex == currentFilter);
            if(m_DirectionArrow) m_DirectionArrow.SetActive(m_SortIndex == currentFilter);
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Filter()
        {
            if (!m_SessionListUI) return;
            if (m_SessionListUI.SortIndex == m_SortIndex)
            {
                // Toggle sort direction
                m_SortDirection = m_SortDirection == SortDirection.Ascending
                    ? SortDirection.Descending
                    : SortDirection.Ascending;
                
                // Flip the y scale of active direction based on the current direction
                if(m_DirectionArrow)
                {
                    m_DirectionArrow.transform.localScale = new Vector3(1,
                        m_SortDirection == SortDirection.Ascending ? -1 : 1, 1);
                }
            }
            
            m_SessionListUI.SetSortDirection(m_SortDirection);
            m_SessionListUI.SetSortIndex(m_SortIndex);
        }
    }
}