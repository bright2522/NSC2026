using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Stop Tick Timer")]
    [Description("Stops a tick timer from a Network Object")]

    [Category("Fusion/Network Object/Stop Tick Timer")]
    
    [Image(typeof(IconTimer), ColorTheme.Type.Red)]
    
    [Keywords("Stop", "Tick Timer", "Timer", "Tick")]
    [Serializable]
    public class InstructionStopTickTimer : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectSelf.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Stop Tick Timer from <b>{networkObject}</b>";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            var go = networkObject.Get(args);
            var no = go.Get<NetworkObject>();

            if (!NetworkDataManager.Instance)
            {
                await Until(() => NetworkDataManager.Instance);
            }
            
            NetworkDataManager.Instance?.RemoveTimer(no.Id);
        }
    }
}