using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Register Character Models")]
    [Description("Register character models for network replication")]

    [Category("Fusion/Models/Register Character Models")]
    
    // [Parameter("Game Mode", "The game mode to start the session")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow, typeof(OverlayListVariable))] 
    
    [Keywords("Fusion", "Network", "Register", "Character Models")]
    [Serializable]
    public class InstructionRegisterNetworkModels : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private CollectorListVariable list = new();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Register Character Models from {list}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var source = list.Get(args);
            foreach (var model in source)
            {
                if (model is ModelConfig modelConfig)
                {
                    NetworkManager.RuntimeModels.TryAdd(modelConfig.prefab.Get(args).name, modelConfig);
                }
            }
            return DefaultResult;
        }
    }
}