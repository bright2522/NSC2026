using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Is Scene Authority")]
    [Description("Returns true if this runner is the scene authority.")]

    [Category("Fusion/Session/Is Scene Authority")]

    [Keywords("Fusion", "Is Scene Authority", "Scene", "Authority")]
    
    [Image(typeof(IconUnity), ColorTheme.Type.TextLight)]
    
    [Serializable]
    public class ConditionIsSceneAuthority : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        protected override string Summary => $"Is Scene Authority";
        
        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            return NetworkManager.Runner && NetworkManager.Runner.IsSceneAuthority;
        }
    }
}