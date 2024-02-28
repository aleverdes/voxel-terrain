using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TravkinGames.Utils
{
    public static class EditorUtils
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        /// <summary>
        /// Сохранить значения полей EditorField<> в редакторские настройки
        /// </summary>
        /// <param name="target">Объект в котором необходимо выполнить сохранение</param>
        public static void SaveEditorFields(Object target)
        {
#if UNITY_EDITOR
            var targetType = target.GetType();
            
            foreach (var field in targetType.GetFields(Flags))
            {
                if (!IsEditorField(field))
                    continue;

                var key = $"{UnityProject.Name}.{targetType.Name}.{field.Name}";
                var type = field.FieldType.GenericTypeArguments[0]; 
                
                if (type == typeof(string))
                    EditorPrefs.SetString(key, (field.GetValue(target) as EditorData<string>)?.Value);
                else if (type == typeof(int))
                    EditorPrefs.SetInt(key, (field.GetValue(target) as EditorData<int>)?.Value ?? default);
                else if (type == typeof(float))
                    EditorPrefs.SetFloat(key, (field.GetValue(target) as EditorData<float>)?.Value ?? default);
                else if (type == typeof(bool))
                    EditorPrefs.SetBool(key, (field.GetValue(target) as EditorData<bool>)?.Value ?? default);
            }
#endif
        }
        
        /// <summary>
        /// Загрузить значения полей EditorField<> из редакторских настроек
        /// ВАЖНО: Все поля будут перезаписаны новыми экземплярами EditorField<>
        /// </summary>
        /// <param name="target">Объект в котором необходимо выполнить загрузку</param>
        public static void LoadEditorFields(Object target)
        {
#if UNITY_EDITOR
            var targetType = target.GetType();
            
            foreach (var field in targetType.GetFields(Flags))
            {
                if (!IsEditorField(field))
                    continue;

                var key = $"{UnityProject.Name}.{targetType.Name}.{field.Name}";
                var type = field.FieldType.GenericTypeArguments[0]; 
                
                if (type == typeof(string))
                    field.SetValue(target, new EditorData<string>(EditorPrefs.GetString(key)));
                else if (type == typeof(int))
                    field.SetValue(target, new EditorData<int>(EditorPrefs.GetInt(key)));
                else if (type == typeof(float))
                    field.SetValue(target, new EditorData<float>(EditorPrefs.GetFloat(key)));
                else if (type == typeof(bool))
                    field.SetValue(target, new EditorData<bool>(EditorPrefs.GetBool(key)));
            }
#endif
        }
        
        private static bool IsEditorField(FieldInfo field)
        {
            return field.FieldType.IsGenericType
                   && field.FieldType.GetGenericTypeDefinition() == typeof(EditorData<>);
        }
        
        private static bool IsEditorField(PropertyInfo property)
        {
            return property.PropertyType.IsGenericType
                   && property.PropertyType.GetGenericTypeDefinition() == typeof(EditorData<>);
        }
    }
}