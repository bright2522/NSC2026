using System.Collections.Generic;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using NinjutsuGames.FusionNetwork.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace NinjutsuGames.FusionNetwork.Editor
{
    public class FieldListTool : TPolymorphicListTool
    {
        private const string NAME_BUTTON_ADD = "GC-Handles-Foot-Add";
        
        private static readonly IIcon ICON_ADD = new IconString(ColorTheme.Type.TextLight);

        // MEMBERS: -------------------------------------------------------------------------------

        private Button m_ButtonAdd;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string ElementNameHead => "GC-Handles-Head";
        protected override string ElementNameBody => "GC-Handles-Body";
        protected override string ElementNameFoot => "GC-Handles-Foot";
        
        protected override List<string> CustomStyleSheetPaths => new List<string>
        {
            EditorPaths.CHARACTERS + "StyleSheets/Handles"
        };

        public override bool AllowReordering => true;
        public override bool AllowDuplicating => true;
        public override bool AllowDeleting  => true;
        public override bool AllowContextMenu => false;
        public override bool AllowInsertion => true;
        public override bool AllowCopyPaste => true;
        public override bool AllowBreakpoint => false;
        public override bool AllowDisable => false;
        public override bool AllowDocumentation => false;
        
        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public FieldListTool(SerializedProperty property)
            : base(property, "m_Fields")
        {
            SerializedObject.Update();
            
            EditorApplication.playModeStateChanged += OnChangePlayMode;

            OnChangePlayMode(EditorApplication.isPlaying
                ? PlayModeStateChange.EnteredPlayMode
                : PlayModeStateChange.ExitingPlayMode
            );
        }
        
        ~FieldListTool()
        {
            EditorApplication.playModeStateChanged -= OnChangePlayMode;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void OnChangePlayMode(PlayModeStateChange state)
        { }
        
        // override 
        
        protected override VisualElement MakeItemTool(int index)
        {
            return new FieldItemTool(this, index);
        }

        protected override void SetupHead()
        { }

        protected override void SetupFoot()
        {
            base.SetupFoot();

            m_ButtonAdd = new Button(() =>
            {
                var insertIndex = PropertyList.arraySize;
                InsertItem(insertIndex, new FieldItem());
            })
            {
                name = NAME_BUTTON_ADD
            };

            m_ButtonAdd.Add(new Image { image = ICON_ADD.Texture });
            m_ButtonAdd.Add(new Label { text = "Add Field..." });

            m_Foot.Add(m_ButtonAdd);
        }
    }
}