using System;
using Fusion;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    [Serializable]
    public struct VariableData : INetworkStruct, IEquatable<VariableData>
    {
        public enum Type
        {
            Number,
            Vector3,
            Vector2,
            Color,
            Bool,
            String,
            PrefabRef
        }
        public Type type;
        public double number;
        public Vector3 vector3;
        public Vector2 vector2;
        public Color color;
        public NetworkBool boolValue;
        public NetworkString<_64> valueString;
        public NetworkPrefabRef prefabRef;

        public static VariableData ConvertFromObject(object obj)
        {
            if (obj == null) return default;

            var data = new VariableData();

            switch (obj)
            {
                case double number:
                    data.number = number;
                    data.type = Type.Number;
                    break;
                case float number1:
                    data.number = number1;
                    data.type = Type.Number;
                    break;
                case int number2:
                    data.number = number2;
                    data.type = Type.Number;
                    break;
                case Vector3 vec3:
                    data.vector3 = vec3;
                    data.type = Type.Vector3;
                    break;
                case Vector2 vec2:
                    data.vector2 = vec2;
                    data.type = Type.Vector2;
                    break;
                case Color color:
                    data.color = color;
                    data.type = Type.Color;
                    break;
                case bool boolVal:
                    data.boolValue = boolVal;
                    data.type = Type.Bool;
                    break;
                case NetworkBool boolVal:
                    data.boolValue = boolVal;
                    data.type = Type.Bool;
                    break;
                case string str:
                    data.valueString = str;
                    data.type = Type.String;
                    break;
                case NetworkString<_64> str:
                    data.valueString = str;
                    data.type = Type.String;
                    break;
                case NetworkPrefabRef prefabRef:
                    data.prefabRef = prefabRef;
                    data.type = Type.PrefabRef;
                    break;
            }

            return data;
        }
        
        public object GetValue()
        {
            return type switch
            {
                Type.Number => number,
                Type.Vector3 => vector3,
                Type.Vector2 => vector2,
                Type.Color => color,
                Type.Bool => (bool)boolValue,
                Type.String => valueString,
                Type.PrefabRef => prefabRef,
                _ => null
            };
        }
        
        public override string ToString()
        {
            return GetValue()?.ToString() ?? "null";
        }

        public bool Equals(VariableData other)
        {
            return type == other.type && 
                   number.Equals(other.number) && 
                   vector3.Equals(other.vector3) && 
                   vector2.Equals(other.vector2) && 
                   color.Equals(other.color) && 
                   boolValue.Equals(other.boolValue) && 
                   valueString.Equals(other.valueString) &&
                   prefabRef.Equals(other.prefabRef);
        }

        public override bool Equals(object obj)
        {
            return obj is VariableData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(type, number, vector3, vector2, color, boolValue, valueString, prefabRef);
        }
    }
}