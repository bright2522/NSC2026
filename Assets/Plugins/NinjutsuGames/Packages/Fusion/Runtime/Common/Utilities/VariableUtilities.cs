using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public static class VariableUtilities
    {
        public static bool IsAllowedType(this object data)
        {
            return data.IsType<int>() || data.IsType<double>() || data.IsType<float>() || data.IsType<bool>()  || data.IsType<Color>() || data.IsType<string>() || data.IsType<Vector3>() || data.IsType<NetworkPrefabRef>();
        }
        
        public static Dictionary<string,SessionProperty> ToSessionProperties(this GlobalNameVariables variables)
        {
            var table = new Dictionary<string, SessionProperty>();
            var max = variables.Names.Length;

            for (var i = 0; i < max; i++)
            {
                var name = variables.Names[i];
                var data = variables.Get(name);

                if (SessionProperty.Support(data))
                {
                    table[name] = SessionProperty.Convert(data);
                }
            }

            return table;
        }

        public static Dictionary<string, SessionProperty> ToSessionProperties(this LocalNameVariables variables)
        {
            var table = new Dictionary<string, SessionProperty>();
            var vars = variables.GetRuntimeVariables();
            var enumerator = vars.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var data = enumerator.Current;
                if (SessionProperty.Support(data.Value))
                {
                    table[data.Name] = SessionProperty.Convert(data.Value);
                }
            }
            return table;
        }

        public static bool IsSupportedType(string typeIDString)
        {
            return typeIDString == ValueNumber.TYPE_ID.String || typeIDString == ValueBool.TYPE_ID.String || typeIDString == ValueColor.TYPE_ID.String || typeIDString == ValueString.TYPE_ID.String || typeIDString == ValueVector3.TYPE_ID.String || typeIDString == ValueNetworkPrefabRef.TYPE_ID.String;
        }
    }
}