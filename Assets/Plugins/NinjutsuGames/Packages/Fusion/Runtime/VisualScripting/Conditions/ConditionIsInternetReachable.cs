using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Internet Reachable")]
    [Description("Returns true if the internet connection is reachable.")]

    [Category("Fusion/Is Internet Reachable")]

    [Keywords("Fusion", "Server", "Internet", "Networking", "Reachable")]
    
    [Image(typeof(IconWeb), ColorTheme.Type.Teal)]
    
    [Serializable]
    public class ConditionIsInternetReachable : Condition
    {
        protected override string Summary => $"Is Internet Reachable";

        protected override bool Run(Args args)
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}