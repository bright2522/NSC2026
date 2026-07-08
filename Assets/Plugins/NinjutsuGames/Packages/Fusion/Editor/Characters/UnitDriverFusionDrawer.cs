using Fusion;
using Fusion.Editor;
using GameCreator.Editor.Characters;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using InteractionMode = UnityEditor.InteractionMode;

namespace NinjutsuGames.FusionNetwork.Editor.Systems
{
    // [CustomPropertyDrawer(typeof(UnitDriverFusionController), true)]
    public class UnitDriverFusionDrawer : IUnitDriverDrawer
    {
        private static GameObject activeObject;
        private bool removingComponents;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            CheckComponents(property);
            return MakePropertyGUI(property, "Driver");
        }

        protected override void OnBuildBody(VisualElement body, SerializedProperty property)
        {
            CheckComponents(property);
        }

        protected void CheckComponents(SerializedProperty property)
        {
            if ((property.serializedObject.targetObject as Character)?.Driver.GetType() == typeof(UnitDriverFusionController))
            {
                AddComponents(property.serializedObject.targetObject as Character);
            }
            else
            {
                RemoveComponents(property.serializedObject.targetObject as Character);
            }
        }

        private void RemoveComponents(Character character)
        {
            var networkObject = character.GetComponent<NetworkObject>();
            if(!networkObject) return;

            activeObject = character.gameObject;
            removingComponents = true;

            var networkCharacter = character.GetComponent<NetworkCharacter>();
            var networkTransform = character.GetComponent<NetworkTransform>();
            
            if(networkCharacter) Object.DestroyImmediate(networkCharacter, true);
            if(networkTransform) Object.DestroyImmediate(networkTransform, true);
            
            if(networkObject)
            {
                EditorApplication.delayCall += () =>
                {
                    AssetDatabaseUtils.SetLabel(character.gameObject, NetworkProjectConfigImporter.FusionPrefabTag, false);
                    activeObject = character.gameObject;
                    EditorApplication.delayCall -= OnRemoved;
                    EditorApplication.delayCall += OnRemoved;
                };
            }
            else
            {
                removingComponents = false;
            }
        }
        
        private void OnRemoved()
        {
            Object.DestroyImmediate(activeObject.GetComponent<NetworkCharacter>(), true);
            Object.DestroyImmediate(activeObject.GetComponent<NetworkObject>(), true);
            AssetDatabaseUtils.SetLabel(activeObject, NetworkProjectConfigImporter.FusionPrefabTag, false);

            EditorApplication.delayCall -= OnRemoved;

            if(activeObject)
            {
                Selection.activeGameObject = activeObject;
            }
            
            removingComponents = false;
        }

        private void AddComponents(Character character)
        {
            if(removingComponents) return;
            var componentAdded = false;
            var networkObject = character.GetComponent<NetworkObject>();
            Selection.activeObject = character.gameObject;

            if(networkObject)
            {
                /*if(activeObject)
                {
                    EditorApplication.delayCall -= OnSelectionChanged;
                    EditorApplication.delayCall += OnSelectionChanged;
                }*/
                return;
            }
            
            var networkCharacter = character.GetComponent<NetworkCharacter>();
            if (!networkCharacter)
            {
                componentAdded = true;
                networkCharacter = character.gameObject.AddComponent<NetworkCharacter>();
            }
            
            var networkTransform = character.GetComponent<NetworkTransform>();
            if (!networkTransform) 
            {
                componentAdded = true;
                networkTransform = character.gameObject.AddComponent<NetworkTransform>();
                networkTransform.DisableSharedModeInterpolation = true;
                networkTransform.SyncScale = true;
                EditorUtility.SetDirty(networkTransform);
            }
            
            /*if (!networkObject)
            {
                componentAdded = true;
                networkObject = character.gameObject.AddComponent<NetworkObject>();
                ReflectionUtilities.SetValue(networkObject, "DestroyWhenStateAuthorityLeaves", true);
            }*/

            var target = PrefabUtility.GetOutermostPrefabInstanceRoot(character.gameObject);
            if (!target) target = character.gameObject;
            var prefabType = PrefabUtility.GetPrefabAssetType(target);
            // var handle = PrefabUtility.GetPrefabInstanceHandle(character.gameObject);
            var status = PrefabUtility.GetPrefabInstanceStatus(character.gameObject);
            
            // Debug.Log($"target: {target} prefabType: {prefabType} parent: {character.gameObject.transform.parent} otherType: {PrefabUtility.GetPrefabAssetType(character.gameObject)} handle: {handle} status: {status}");
            if (componentAdded && prefabType != PrefabAssetType.NotAPrefab && prefabType != PrefabAssetType.Variant && status == PrefabInstanceStatus.NotAPrefab)
            {
                var prefab = AssetDatabase.GetAssetPath(target);
                if(PrefabUtility.IsAddedComponentOverride(networkCharacter)) PrefabUtility.ApplyObjectOverride(networkCharacter, prefab, InteractionMode.AutomatedAction);
                // if(PrefabUtility.IsAddedComponentOverride(networkTransform)) PrefabUtility.ApplyObjectOverride(networkTransform, prefab, InteractionMode.AutomatedAction);
                // if(PrefabUtility.IsAddedComponentOverride(networkObject)) PrefabUtility.ApplyObjectOverride(networkObject, prefab, InteractionMode.AutomatedAction);
                if(prefabType != PrefabAssetType.Variant) PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
            activeObject = character.gameObject;
            EditorApplication.delayCall -= OnSelectionChanged;
            EditorApplication.delayCall += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            EditorApplication.delayCall -= OnSelectionChanged;
            
            var networkTransform = activeObject.GetComponent<NetworkTransform>();
            networkTransform.DisableSharedModeInterpolation = true;
            networkTransform.SyncScale = true;
            EditorUtility.SetDirty(networkTransform);

            if(activeObject)
            {
                Selection.activeGameObject = activeObject;
            }
        }

        protected override IIcon UnitIcon => new IconWheel(ColorTheme.Type.TextLight);
    }

    /*[CustomPropertyDrawer(typeof(UnitDriverController), true)]
    public class UnitDriverControllerDrawer : UnitDriverFusionDrawer
    {
        protected override void OnBuildBody(VisualElement body, SerializedProperty property)
        {
            CheckComponents(property);
        }
    }
    
    [CustomPropertyDrawer(typeof(UnitDriverRigidbody), true)]
    public class UnitDriverRigidbodyDrawer : UnitDriverFusionDrawer
    {
        protected override void OnBuildBody(VisualElement body, SerializedProperty property)
        {
            CheckComponents(property);
        }
    }
    
    [CustomPropertyDrawer(typeof(IUnitDriver), true)]
    public class IUnitDriverDrawerThree : UnitDriverFusionDrawer
    {
        protected override void OnBuildBody(VisualElement body, SerializedProperty property)
        {
            CheckComponents(property);
        }
    }*/
}