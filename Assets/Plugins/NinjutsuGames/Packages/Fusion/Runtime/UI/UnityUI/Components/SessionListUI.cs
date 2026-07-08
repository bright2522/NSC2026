using System;
using System.Collections.Generic;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public enum SortDirection
    {
        Ascending,
        Descending
    }
    
    [AddComponentMenu("Game Creator/UI/Fusion/Session List UI")]
    [Icon(RuntimePaths.GIZMOS + "GizmoSessionListUI.png")]
    [HelpURL("https://docs.ninjutsugames.com/game-creator-2/fusion-module/user-interface#session-list-ui")]
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_LAST_LATER)]
    
    [Serializable]
    public class SessionListUI : MonoBehaviour
    {
        [SerializeField] private RectTransform m_Content;
        [SerializeField] private GameObject m_Prefab;
        [SerializeField] private GameObject m_EmptyMessage;
        [SerializeField] private SortDirection m_SortDirection = SortDirection.Descending;
        [SerializeField] private int m_SortFieldIndex = 0;
        
        private List<SessionInfo> _sessionList;

        public int SortIndex => m_SortFieldIndex;
        public SortDirection SortDirection => m_SortDirection;
        public event Action EventRefreshUI;
        
        private void OnEnable()
        {
            NetworkManager.EventSessionListUpdated += OnSessionListUpdated;
            OnSessionListUpdated();
        }

        private void OnDisable()
        {
            NetworkManager.EventSessionListUpdated -= OnSessionListUpdated;
        }

        private void OnSessionListUpdated()
        {
            if(NetworkManager.IsConnectedInLobby) UpdateSessionList(NetworkManager.RunnerLobby, NetworkManager.SessionList);
        }

        private void UpdateSessionList(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if(sessionList == null) return;
            
            _sessionList = sessionList;
            if(m_EmptyMessage) m_EmptyMessage.SetActive(sessionList.Count == 0);
            RectTransformUtils.RebuildChildren(m_Content, m_Prefab, sessionList.Count);
            RefreshUI();
        }

        private void RefreshUI()
        {
            for (var i = 0; i < _sessionList.Count; ++i)
            {
                var child = m_Content.GetChild(i);
                var itemUI = child.Get<SessionItemUI>();
                if (itemUI) itemUI.RefreshUI(_sessionList[i]);
            }
            
            // Sort the children by the SortField value from the ScoreboardItemUI
            var children = new List<Transform>();
            for (var i = 0; i < m_Content.childCount; ++i)
            {
                children.Add(m_Content.GetChild(i));
            }
            
            children.Sort((a, b) =>
            {
                var itemA = a.Get<SessionItemUI>();
                var itemB = b.Get<SessionItemUI>();
                if (!itemA || !itemB) return 0;
                
                if (m_SortDirection == SortDirection.Descending)
                {
                    (itemA, itemB) = (itemB, itemA);
                }
                
                return itemA.GetField(m_SortFieldIndex).CompareTo(itemB.GetField(m_SortFieldIndex));
            });
            
            for (var i = 0; i < children.Count; ++i)
            {
                var child = children[i];
                child.SetSiblingIndex(i);
                
                var itemUI = child.Get<SessionItemUI>();
                if (itemUI) itemUI.SetAlternateBackground(i % 2 == 0);
            }
            
            EventRefreshUI?.Invoke();
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SetSortIndex(int index)
        {
            m_SortFieldIndex = index;
            RefreshUI();
        }   
        
        public void SetSortDirection(SortDirection direction)
        {
            m_SortDirection = direction;
            RefreshUI();
        }  
    }
}