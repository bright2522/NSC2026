using GameCreator.Editor.Installs;
using UnityEditor;

namespace NinjutsuGames.FusionNetwork.Editor
{
    public static class UninstallFusion
    {
        private const string UNINSTALL_TITLE = "Are you sure you want to uninstall {0}";
        private const string UNINSTALL_MSG = "This operation cannot be undone";
        private const string ModuleName = "Fusion";
        
        [MenuItem(
            itemName: "Game Creator/Uninstall/Fusion",
            isValidateFunction: false,
            priority: UninstallManager.PRIORITY
        )]
        
        private static void Uninstall()
        {
            UninstallManager.Uninstall(ModuleName);

            var path = $"Assets/Plugins/NinjutsuGames/Packages/{ModuleName}";
            if (!AssetDatabase.IsValidFolder(path)) return;

            var delete = EditorUtility.DisplayDialog(
                string.Format(UNINSTALL_TITLE, ModuleName),
                UNINSTALL_MSG, 
                "Yes", "Cancel"
            );
            
            if (!delete) return;
            
            AssetDatabase.MoveAssetToTrash(path+"/Runtime");
            AssetDatabase.MoveAssetToTrash(path+"/Editor");
            AssetDatabase.MoveAssetToTrash(path+"/Examples");
        }
    }
}