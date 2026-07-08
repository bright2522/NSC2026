using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Set Network Prefab Ref")]
    [Description("Sets a Network Prefab Ref value equal to another one")]

    [Category("Fusion/Set Network Prefab Ref")]

    [Parameter("Set", "Where the value is set")]
    [Parameter("From", "The value that is set")]

    [Keywords("Change", "Instance", "Variable", "Asset", "Network", "Prefab", "Ref")]
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Green)]
    
    [Serializable]
    public class InstructionSetNetworkPrefabRef : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField] 
        private PropertySetNetworkPrefabRef m_Set = SetNetworkPrefabRefNone.Create;
        
        [SerializeField]
        private PropertyGetNetworkPrefabRef m_From = new();

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public override string Title => $"Set {m_Set} = {m_From}";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override Task Run(Args args)
        {
            var value = m_From.Get(args);
            m_Set.Set(value, args);

            return DefaultResult;
        }
    }
}