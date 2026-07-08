using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Player by Index")]
    [Category("Fusion/Player by Index")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Green, typeof(OverlayListVariable))]
    [Description("Reference to the Player gameObject by index.")]

    [Serializable]
    public class GetGameObjectPlayerByIndex : PropertyTypeGetGameObject
    {
        [SerializeField] private PropertyGetInteger playerIndex = new(0);
        
        public override GameObject Get(Args args) => GetObject(args);

        private GameObject GetObject(Args args)
        {
            var index = (int)playerIndex.Get(args);
            var player = PlayerManager.Instance.GetPlayerAvatarByIndex(index);
            return player ? player.gameObject : null;
        }

        public static PropertyGetGameObject Create => new(
            new GetGameObjectPlayerByIndex()
        );

        public override string String => $"Player #{playerIndex}";
    }
}