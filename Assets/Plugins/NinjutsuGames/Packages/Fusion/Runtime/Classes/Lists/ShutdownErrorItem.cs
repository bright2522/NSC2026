using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class ShutdownErrorItem : TPolymorphicItem<ShutdownErrorItem>
    {
        [SerializeField] private ShutdownReason m_ShutdownReason;
        [SerializeField] private PropertyGetString m_Title;
        [SerializeField] private PropertyGetString m_Message;

        // PROPERTIES: ----------------------------------------------------------------------------
        public override string Title => $"{m_ShutdownReason}";
        public ShutdownReason Reason => m_ShutdownReason;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public string GetName(Args args) => m_Title.Get(args);
        public string GetMessage(Args args) => m_Message.Get(args);
        
        public ShutdownErrorItem(ShutdownReason shutdownReason, string name, string message)
        {
            m_ShutdownReason = shutdownReason;
            m_Title = new PropertyGetString(name);
            m_Message = GetStringTextArea.Create(message);
        }

        public override string ToString()
        {
            return $"{Reason} - {GetName(Args.EMPTY)} = {GetMessage(Args.EMPTY)}";
        }
    }
}