using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public enum TimerType
    {
        FromSeconds,
        FromTicks
    }
    [Title("Start Tick Timer")]
    [Description("Starts a tick timer that ticks every x seconds")]

    [Category("Fusion/Network Object/Start Tick Timer")]
    
    [Parameter("Mode", "CreateFromSeconds:<br>Creates a new TickTimer with the target tick calculated using the amount of Seconds provided and the current simulation tick<br><br>CreateFromTicks:<br>Creates a new TickTimer with the target tick calculated using the amount of Ticks provided and the current simulation tick.")]
    [Parameter("Value", "The amount of seconds or ticks to wait before the timer expires")]
    [Parameter("Wait To Complete", "If true this Instruction waits until the timer expires")]
    
    [Image(typeof(IconTimer), ColorTheme.Type.Green)]
    
    [Keywords("Create", "New", "Tick Timer", "Timer", "Tick")]
    [Serializable]
    public class InstructionTickTimer : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject networkObject = GetGameObjectSelf.Create();
        [SerializeField] private TimerType mode = TimerType.FromSeconds;
        [SerializeField] private PropertyGetDecimal value = GetDecimalConstantOne.Create;
        [Space] 
        [SerializeField] private bool m_WaitToComplete = true;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Start Tick Timer <b>{mode}</b>: {value}{(mode == TimerType.FromSeconds ? "(s)" : "")}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            var runner = NetworkRunner.GetRunnerForGameObject(args.Self);
            var go = networkObject.Get(args);
            var no = go.Get<NetworkObject>();

            if (!NetworkDataManager.Instance)
            {
                await Until(() => NetworkDataManager.Instance);
            }
            
            var timer = NetworkDataManager.Instance.GetTimer(no, (float)value.Get(args), mode == TimerType.FromTicks);
            
            if(m_WaitToComplete)
            {
                await Until(() => timer.ExpiredOrNotRunning(runner));
                NetworkDataManager.Instance.RemoveTimer(no.Id);
            }
        }
    }
}