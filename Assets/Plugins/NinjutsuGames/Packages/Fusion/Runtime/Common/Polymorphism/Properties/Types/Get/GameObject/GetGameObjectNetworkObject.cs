using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Network Object")]
    [Category("Fusion/Network Object")]
    
    [Image(typeof(IconBoltOutline), ColorTheme.Type.Blue)]
    [Description("A NetworkObject component reference")]

    [Serializable] [HideLabelsInEditor]
    public class GetGameObjectNetworkObject : PropertyTypeGetGameObject
    {
        [SerializeField] protected NetworkObject m_NetworkObject;

        public override GameObject Get(Args args) => m_NetworkObject != null 
            ? m_NetworkObject.gameObject 
            : null;
        
        public override GameObject Get(GameObject gameObject) => m_NetworkObject != null 
            ? m_NetworkObject.gameObject 
            : null;

        public override T Get<T>(Args args)
        {
            if (typeof(T) == typeof(NetworkObject)) return m_NetworkObject as T;
            return base.Get<T>(args);
        }
        
        public GetGameObjectNetworkObject() : base()
        { }

        public GetGameObjectNetworkObject(GameObject gameObject) : this()
        {
            m_NetworkObject = gameObject.Get<NetworkObject>();
        }
        
        public GetGameObjectNetworkObject(NetworkObject networkObject) : this()
        {
            m_NetworkObject = networkObject;
        }

        public static PropertyGetGameObject Create()
        {
            var instance = new GetGameObjectNetworkObject();
            return new PropertyGetGameObject(instance);
        }
        
        public static PropertyGetGameObject Create(GameObject gameObject)
        {
            var instance = new GetGameObjectNetworkObject
            {
                m_NetworkObject = gameObject != null ? gameObject.Get<NetworkObject>() : null
            };
            
            return new PropertyGetGameObject(instance);
        }
        
        public static PropertyGetGameObject Create(NetworkObject trigger)
        {
            var instance = new GetGameObjectNetworkObject
            {
                m_NetworkObject = trigger
            };
            
            return new PropertyGetGameObject(instance);
        }

        public override string String => m_NetworkObject != null
            ? m_NetworkObject.gameObject.name
            : "(none)";
        
        public override GameObject EditorValue => m_NetworkObject != null 
            ? m_NetworkObject.gameObject
            : null;
    }
}