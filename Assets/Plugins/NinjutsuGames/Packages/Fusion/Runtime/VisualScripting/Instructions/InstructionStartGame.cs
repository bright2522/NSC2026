using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Start Game")]
    [Description("Starts a new game session or joins an existing one")]

    [Category("Fusion/Session/Start Game")]
    
    [Parameter("Game Mode", "The game mode to start the session")]
    [Parameter("Session Name", "The name of the session to join or create")]
    [Parameter("Player Count", "Number of players allowed to connect to the session. Default: DefaultPlayers from the Global NetworkProjectConfig. Set to 0 to ignore this parameter for matchmaking.")]
    [Parameter("Starting Scene", "Scene that will be set as the starting Scene.")]
    
    [Parameter("Validate Session Code", "If enabled, the validation process verifies the Session Code, which is passed as the Session Name, prior to joining the Session.")]
    [Parameter("Matchmaking Mode", "Options for matchmaking rules for JoinRandom.\n\n" +
                 "<b>FillRoom</b>: Fills up rooms (oldest first) to get players together as fast as possible. Default.\n\n" +
                 "<b>SerialMatching</b>: Distributes players across available rooms sequentially but takes filter into account. \n\n" +
                 "<b>RandomMatching</b>: Joins a (fully) random room. Expected properties must match but aside from this, any available room")]
    [Parameter("Is Open", "Session should be created Open or Closed to accept joins")]
    [Parameter("Is Visible", "Session should be Visible or not in the Session Lobby list")]
    [Parameter("Enable Client Session Creation","Enables the Session creation when starting a Client with an specific Session Name")]
    [Parameter("Custom Lobby Name", "Session Custom Lobby to be published in")]
    [Parameter("Custom App Version", "Custom App Version to be used when joining a Session")]
    
    [Image(typeof(IconChip), ColorTheme.Type.Green)] 
    
    [Keywords("Start", "Game", "Start Game", "Fusion")]
    [Serializable]
    public class InstructionStartGame : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private GameMode gameMode = GameMode.Shared;
        [SerializeField] private PropertyGetString sessionName = GetStringEmpty.Create;
        [SerializeField] private PropertyGetInteger playerCount = GetDecimalInteger.Create(0);
        [Space(4)]
        [SerializeField] private SceneSelector initialScene = new();
        [SerializeField] private RegionSettings regionSettings = new();
        [SerializeField] private StartGameSettings advancedSettings = new();
        [SerializeField] private AuthenticationSettings authenticationSettings = new();
        [SerializeField, Space(4)] private bool m_WaitToFinish = true;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Start Game: {gameMode}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            var task = NetworkManager.ConnectAsync(args, sessionName.Get(args), (int)playerCount.Get(args), gameMode, initialScene, regionSettings, advancedSettings, authenticationSettings);
            if (m_WaitToFinish)
            {
                await task;
            }
        }
    }
}