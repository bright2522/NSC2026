using System;
using System.Threading.Tasks;
using Fusion;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Floating Text")]
    [Description("Shows a floating text message on a target GameObject")]

    [Category("Fusion/UI/Floating Text")]
    
    [Parameter("Target", "The target GameObject to show the floating text on")]
    [Parameter("Text", "The text to show")]
    [Parameter("Settings", "The settings for the floating text")]
    
    [Image(typeof(IconUIText), ColorTheme.Type.Green, typeof(OverlayArrowUp))]

    [Keywords("Floating", "Text", "Floating Text")]
    [Serializable]
    public class InstructionAddFloatingText : Instruction
    {
        [SerializeField] private PropertyGetGameObject target = GetGameObjectTransform.Create();
        [SerializeField] private PropertyGetString text = GetStringString.Create;
        [SerializeField] private FloatingTextSettings settings = new();

        public override string Title => $"Floating Text: {text}";

        private string id;

        protected override Task Run(Args args)
        {
            var go = target.Get(args);

            // Validate gameObject before proceeding
            if (go == null)
            {
                Debug.LogWarning("Floating Text: Target GameObject is null");
                return DefaultResult;
            }

            var targetTransform = go.transform;
            if (targetTransform == null)
            {
                Debug.LogWarning("Floating Text: Target Transform is null");
                return DefaultResult;
            }

            var character = go.Get<NetworkObject>();
            if (character) id = character.Id.ToString();
            if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString();

            FloatingTextManager.Show(id, text.Get(args), targetTransform, settings);
            return DefaultResult;
        }
    }
}