#if UNITY_EDITOR
using UnityEditor;
#endif
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public static class FusionUtilities
    {
        public static bool IsPlayerAvatar(this GameObject target)
        {
            return target && target.Get<NetworkCharacter>() != null;
        }
        
        public static bool IsLocalPlayer(this GameObject target)
        {
            if(!target || !NetworkManager.Runner) return false;
            var avatar = target.Get<NetworkCharacter>();
            return avatar && avatar.Object.InputAuthority == NetworkManager.Runner.LocalPlayer;
        }
        
        public static bool IsProxy(this GameObject target)
        {
            if(!target || !NetworkManager.Runner) return false;
            var avatar = target.Get<NetworkCharacter>();
            return avatar && avatar.Object.IsProxy;
        }
        
        public static bool TryGetPrefabEditorInstance(string guid, out NetworkObject prefab)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(guid))
            {
                prefab = null;
                return false;
            }

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                prefab = null;
                return false;
            }

            var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!gameObject)
            {
                prefab = null;
                return false;
            }

            prefab = gameObject.GetComponent<NetworkObject>();
            return prefab;
#else
            prefab = null;
            return false;
#endif
        }
        
        public static string GetPrefabName(this NetworkPrefabRef prefabRef)
        {
            return TryGetPrefabEditorInstance(prefabRef.ToUnityGuidString(), out var prefab) ? prefab.name : "(none)";
        }
    }
}