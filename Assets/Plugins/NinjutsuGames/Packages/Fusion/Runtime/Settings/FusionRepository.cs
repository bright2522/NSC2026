using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.Linq;
using System.IO;
using UnityEditor;
#endif

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public class FusionRepository : TRepository<FusionRepository>
    {
        public const string REPOSITORY_ID = "fusion.general";

        // REPOSITORY PROPERTIES: -----------------------------------------------------------------
        
        public override string RepositoryID => REPOSITORY_ID;

#if UNITY_EDITOR
        public FusionRepository()
        {
            TryAssignDefaultProfanityCsv();
        }
#endif

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private FusionVersion version = new();
        [SerializeField] private FusionCoreSettings settings = new();
        [SerializeField] private FusionSessionCodeGenerator sessionCodeGenerator = new();
        [SerializeField] private FusionRegions regions = new();
        [SerializeField] private FusionErrorMessages errorMessages = new();
        [SerializeField] private ProfanityFilter profanityFilter = new();
        [SerializeField] private FusionFailSafe failSafe = new();
        [FormerlySerializedAs("updates")] [SerializeField] private FusionSubModules subModules = new();

        // PROPERTIES: ----------------------------------------------------------------------------

        public FusionCoreSettings Settings => settings;
        public FusionSessionCodeGenerator SessionCodeGenerator => sessionCodeGenerator;
        public FusionRegions Regions => regions;
        public FusionErrorMessages ErrorMessages => errorMessages;
        public ProfanityFilter ProfanityFilter => profanityFilter;
        public FusionFailSafe FailSafe => failSafe;
        
        // EDITOR ENTER PLAYMODE: -----------------------------------------------------------------

#if UNITY_EDITOR
        
        [InitializeOnEnterPlayMode]
        public static void InitializeOnEnterPlayMode() => Instance = null;
        
#endif
        
#if UNITY_EDITOR
        private void TryAssignDefaultProfanityCsv()
        {
            try
            {
                if (profanityFilter != null && profanityFilter.CsvAsset == null)
                {
                    const string defaultCsvPath = "Assets/Plugins/NinjutsuGames/Packages/Fusion/Runtime/Assets/ProfanityFilterList.csv";
                    var csv = AssetDatabase.LoadAssetAtPath<TextAsset>(defaultCsvPath);
                    if (csv != null)
                    {
                        profanityFilter.CsvAsset = csv;
                    }
                    else
                    {
                        Debug.LogWarning($"[FusionRepository] Could not find default ProfanityFilter CSV at '{defaultCsvPath}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FusionRepository] Error assigning default ProfanityFilter CSV: {ex.Message}");
            }
        }
        
        [InitializeOnLoadMethod]
        private static void Init()
        {
            /*NetworkProjectConfig.Global.PhysicsEngine = NetworkProjectConfig.PhysicsEngines.Physics3D;
            NetworkProjectConfig.Global.ServerPhysicsMode = NetworkProjectConfig.PhysicsModes.ClientPrediction;
            Physics.autoSimulation = false;*/

            // Debug.Log($"[Fusion Module Setup] PhysicsEngine = Physics3D, ServerPhysicsMode = ClientPrediction, Physics.autoSimulation = false. Adding {assembly} to assemblies to weave...");
            
            var assembly = typeof(FusionRepository).Assembly.GetName().Name;
            InitModule(assembly, "Fusion Module");
        }

        public static void InitModule(string assembly, string name)
        {
            if (NetworkProjectConfig.Global.AssembliesToWeave.Any(assembly.Equals)) return; 
            
            Debug.Log($"[{name}] Adding {assembly} to assemblies to weave...");
            ArrayUtility.Add(ref NetworkProjectConfig.Global.AssembliesToWeave, assembly);
            var so = Resources.LoadAll<NetworkProjectConfigAsset>($"");
            foreach (var asset in so)
            {
                asset.Config.AssembliesToWeave = NetworkProjectConfig.Global.AssembliesToWeave;
                EditorUtility.SetDirty(asset);
                var json = EditorJsonUtility.ToJson(NetworkProjectConfig.Global, true);
                File.WriteAllText(AssetDatabase.GetAssetPath(asset), json);
            }
            
            Debug.Log($"[{name}] Setup complete.");
        }
#endif
    }
}