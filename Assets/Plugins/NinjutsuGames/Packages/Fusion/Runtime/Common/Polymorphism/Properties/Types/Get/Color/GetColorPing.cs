using System;
using Fusion;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Title("Ping Color")]
    [Category("Fusion/Ping Color")]

    [Image(typeof(IconColor), ColorTheme.Type.Green, typeof(OverlayArrowLeft))]
    [Description("Returns the color of the player's ping value based on range and gradient.")]

    [Serializable]
    public class GetColorPing : PropertyTypeGetColor
    {
        [SerializeField] protected PropertyGetGameObject m_Target = GetGameObjectLocalPlayer.Create();
        [SerializeField] protected Gradient m_ColorRange = new()
        {
            colorKeys = new GradientColorKey[]
            {
                new(Color.green, 0f),
                new(Color.yellow, 0.5f),
                new(Color.red, 1f),
            }
        };
        [SerializeField] private Vector2 m_Range = new(0f, 1000f);

        public override Color Get(Args args)
        {
            var p = m_Target.Get(args);
            if(!p || !PlayerManager.Instance) return Color.white; 
            var no = p.Get<NetworkObject>();
            var networkPlayer = PlayerManager.Instance.GetPlayer(no ? no.InputAuthority : NetworkManager.Runner.LocalPlayer);
            if (!networkPlayer) return Color.white;

            var ping = (float)networkPlayer.Ping;
            var t = Mathf.InverseLerp(m_Range.x, m_Range.y, ping);
            return m_ColorRange.Evaluate(t);
        }

        public static PropertyGetColor Create(Color value) => new(
            new GetColorValue(value)
        );

        public override string String => $"{m_Target} Ping Color";
    }
}