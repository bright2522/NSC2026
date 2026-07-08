using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Has State Authority")]
    [Category("Fusion/Network Object/Has State Authority")]

    [Image(typeof(IconCharacterState), ColorTheme.Type.Green, typeof(OverlayBolt))]
    [Description("Returns true if local player has state authority over the specified object.")]
    
    [Serializable] [HideLabelsInEditor]
    public class GetBoolHasStateAuthority : PropertyTypeGetBool
    {
        [SerializeField] private PropertyGetGameObject target = GetGameObjectTarget.Create();
        public override bool Get(Args args)
        {
            var go = target.Get(args);
            var networkObject = go.Get<NetworkObject>();
            return networkObject && networkObject.HasStateAuthority;
        }
        public override string String => $"Has State Authority of {target}";
    }
}