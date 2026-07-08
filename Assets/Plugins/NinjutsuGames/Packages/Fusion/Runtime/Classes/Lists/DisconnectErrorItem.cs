using System;
using Fusion.Sockets;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class DisconnectErrorItem : TPolymorphicItem<DisconnectErrorItem>
    {
        [SerializeField] private NetDisconnectReason m_DisconnectReason;
        [SerializeField] private PropertyGetString m_Title;
        [SerializeField] private PropertyGetString m_Message;

        // PROPERTIES: ----------------------------------------------------------------------------
        public override string Title => $"{m_DisconnectReason}";
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public string GetName(Args args) => m_Title.Get(args);
        public string GetMessage(Args args) => m_Message.Get(args);
        
        public DisconnectErrorItem(NetDisconnectReason disconnectReason, string name, string message)
        {
            m_DisconnectReason = disconnectReason;
            m_Title = new PropertyGetString(name);
            m_Message = GetStringTextArea.Create(message);
        }
    }
}