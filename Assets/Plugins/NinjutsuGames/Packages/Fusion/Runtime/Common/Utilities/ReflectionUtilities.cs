using System.Reflection;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    public static class ReflectionUtilities
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
        public static void SetUniqueId(Character character, string id)
        {
            SetValue(character, "m_UniqueID", new UniqueID(id));
            // character.GetType().GetField("m_UniqueID", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(character, new UniqueID(id));
        }
        
        public static NameVariableRuntime GetRuntimeVariables(this LocalNameVariables variables)
        {
            return GetValue<NameVariableRuntime>(variables, "m_Runtime");
        }
        
        public static Event GetTriggerEvent(this Trigger trigger)
        {
            return GetValue<Event>(trigger, "m_TriggerEvent");
        }
        
        public static T GetValue<T>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, Flags);
            return (T)field.GetValue(instance);
        }
        
        public static void SetValue(object instance, string fieldName, object value)
        {
            instance.GetType().GetField(fieldName, Flags)?.SetValue(instance, value);
        }
        
        public static void SetVariablesUniqueId(LocalNameVariables variables, string id)
        {
            SetValue(variables, "m_SaveUniqueID", new SaveUniqueID(true, id));
            // variables.GetType().GetField("m_SaveUniqueID", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(variables, new SaveUniqueID(true, id));
        }
    }
}