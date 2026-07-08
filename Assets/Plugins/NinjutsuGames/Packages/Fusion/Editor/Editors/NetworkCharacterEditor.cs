using Fusion;
using Fusion.Editor;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using InteractionMode = UnityEditor.InteractionMode;

namespace NinjutsuGames.FusionNetwork.Editor
{
    [CustomEditor(typeof(NetworkCharacter))]
    public class NetworkCharacterEditor : UnityEditor.Editor
    {
        private const string Description = "This component is used to synchronize a character across the network.";
        private NetworkCharacter _networkCharacter;
        private NetworkObject _networkObject;
        private NetworkTransform _networkTransform;
        
        private readonly Vector2 _range = new(0f, 1000f);
        private readonly Gradient _colors = new()
        {
            colorKeys = new GradientColorKey[]
            {
                new(Color.green, 0f),
                new(Color.yellow, 0.5f),
                new(Color.red, 1f),
            }
        };
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            _networkCharacter = serializedObject.targetObject as NetworkCharacter;
            AddComponents(serializedObject.targetObject as NetworkCharacter);
            
            var playMode = EditorApplication.isPlayingOrWillChangePlaymode && !PrefabUtility.IsPartOfPrefabAsset(target) && _networkCharacter.gameObject.activeInHierarchy;
            switch (playMode)
            {
                case true: 
                    PaintRuntimeView(root);
                    break;
                case false: 
                    root.Add(new InfoMessage(Description));
                    break;
            }
            return root;
        }

        private void PaintRuntimeView(VisualElement root)
        {
            root.Clear();
            var message = $"Username: Unknown\nPing: <b><color=#ffffff>0</color></b> ms";
            var isNPC = _networkCharacter && _networkCharacter.Object && _networkCharacter.Object.InputAuthority.IsNone;
            var combat = _networkCharacter.Get<Character>().Combat;
            var primary = combat.Targets.Primary;
            var primaryText = primary ? primary.name : "(none)";
            if(isNPC)
            {
                // Redraw the view every 1 second
                root.schedule.Execute(() =>
                {
                    if (PlayerManager.Instance == null) return;
                    if (_networkCharacter.Object == null) return;
                
                    root.Clear();

                    
                    message = $"Targets: <b>{combat.Targets.List.Count}</b> <b>({primaryText})</b>\nMovement Type: <b>{_networkCharacter.Get<Character>().Motion.MovementType}</b>";
                    root.Add(new InfoMessage(message));
                }).Every(1000);
                return;
            }
            // Redraw the view every 1 second
            root.schedule.Execute(() =>
            {
                if (PlayerManager.Instance == null) return;
                if (_networkCharacter.Object == null) return;
                
                root.Clear();

                var player = PlayerManager.Instance.GetPlayer(_networkCharacter.Object.InputAuthority);
                var color = player == null
                    ? "ffffff"
                    : ColorUtility.ToHtmlStringRGB(GetColorPing(player.Ping));
                message = player == null ? Description : $"Username: <color=#FFF>{player?.Username}</color>\nPing: <b><color=#{color}>{player.Ping}</color></b> ms\nTargets: <b>{combat.Targets.List.Count}</b> <b>({primaryText})</b>";
                root.Add(new InfoMessage(message));
            }).Every(1000);
        }
        
        private Color GetColorPing(float ping)
        {
            var t = Mathf.InverseLerp(_range.x, _range.y, ping);
            return _colors.Evaluate(t);
        }

        private void OnDestroy()
        {
            RemoveComponents(_networkObject, _networkTransform, _networkCharacter);
        }

        private static void RemoveComponents(NetworkObject networkObject, NetworkTransform networkTransform, NetworkCharacter networkCharacter)
        {
            if (networkCharacter)  return;
            if(!networkObject) return;
            if(networkTransform) DestroyImmediate(networkTransform, true);
            
            if(networkObject)
            {
                EditorApplication.delayCall += () =>
                {
                    AssetDatabaseUtils.SetLabel(networkObject.gameObject, NetworkProjectConfigImporter.FusionPrefabTag, false);
                    DestroyImmediate(networkObject, true);
                };
            }
        }

        private void AddComponents(NetworkCharacter character)
        {
            var componentAdded = false;
            _networkObject = character.GetComponent<NetworkObject>();
            Selection.activeObject = character.gameObject;

            _networkTransform = character.GetComponent<NetworkTransform>();
            serializedObject.Update();
            if (!_networkTransform)
            {
                componentAdded = true;
                _networkTransform = character.gameObject.AddComponent<NetworkTransform>();
            }
            _networkTransform.DisableSharedModeInterpolation = true;
            _networkTransform.SyncScale = true;
            serializedObject.ApplyModifiedProperties();
            
            var target = PrefabUtility.GetOutermostPrefabInstanceRoot(character.gameObject);
            if (!target) target = character.gameObject;
            var prefabType = PrefabUtility.GetPrefabAssetType(target);
            // var handle = PrefabUtility.GetPrefabInstanceHandle(character.gameObject);
            var status = PrefabUtility.GetPrefabInstanceStatus(character.gameObject);
            
            // Debug.Log($"target: {target} prefabType: {prefabType} parent: {character.gameObject.transform.parent} otherType: {PrefabUtility.GetPrefabAssetType(character.gameObject)} handle: {handle} status: {status}");
            if (componentAdded && prefabType != PrefabAssetType.NotAPrefab && prefabType != PrefabAssetType.Variant && status == PrefabInstanceStatus.NotAPrefab)
            {
                var prefab = AssetDatabase.GetAssetPath(target);
                if(PrefabUtility.IsAddedComponentOverride(_networkTransform)) PrefabUtility.ApplyObjectOverride(_networkTransform, prefab, InteractionMode.AutomatedAction);
                if(PrefabUtility.IsAddedComponentOverride(_networkObject)) PrefabUtility.ApplyObjectOverride(_networkObject, prefab, InteractionMode.AutomatedAction);
                if(prefabType != PrefabAssetType.Variant) PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
        }
    }
}