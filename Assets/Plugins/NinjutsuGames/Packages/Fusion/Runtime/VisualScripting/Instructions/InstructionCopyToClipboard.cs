using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Copy To Clipboard")]
    [Description("Copies the specified text to the clipboard")]

    [Category("Fusion/UI/Copy To Clipboard")]
    [Parameter("Text", "The text to copy to the clipboard")]
    [Image(typeof(IconCopy), ColorTheme.Type.Green)]
    [Keywords("Copy", "Clipboard")]
    [Serializable]
    public class InstructionCopyToClipboard : Instruction
    {
        [SerializeField] private PropertyGetString text = GetStringUIInputField.Create;
        public override string Title => $"Copy {text} to Clipboard";

        protected override Task Run(Args args)
        {
            var txt = text.Get(args);
            if (!string.IsNullOrEmpty(txt))
            {
                GUIUtility.systemCopyBuffer = txt;
            }
            return DefaultResult;
        }
    }
}