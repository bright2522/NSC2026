using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("None")]
    [Category("None")]
    [Description("Don't save on anything")]
    
    [Image(typeof(IconNull), ColorTheme.Type.TextLight)]

    [Serializable]
    public class SetNetworkPrefabRefNone : PropertyTypeSetNetworkPrefabRef
    {
        public override void Set(NetworkPrefabRef value, Args args)
        { }
        
        public override void Set(NetworkPrefabRef value, GameObject gameObject)
        { }

        public static PropertySetNetworkPrefabRef Create => new(
            new SetNetworkPrefabRefNone()
        );

        public override string String => "(none)";
    }
}