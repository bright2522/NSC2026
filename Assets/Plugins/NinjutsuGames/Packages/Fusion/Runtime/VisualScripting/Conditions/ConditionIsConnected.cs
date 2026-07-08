using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Connected")]
    [Description("Returns true if the client is connected to the server.")]

    [Category("Fusion/Session/Is Connected")]

    [Keywords("Fusion", "Is Connected", "Connected")]
    
    [Image(typeof(IconWeb), ColorTheme.Type.Green)]
    
    [Serializable]
    public class ConditionIsConnected : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"Connected to Fusion";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.IsConnected;
        }
    }
}