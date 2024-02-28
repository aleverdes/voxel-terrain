using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TravkinGames.Utils
{
    public static class AssetDatabaseUtils
    {
        public static bool IsCreated(ScriptableObject so)
        {
#if UNITY_EDITOR
            return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(so));
#endif
            return false;
        }

        public static T FindObjectByName<T>(string equalsName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase) where T : Object
        {
#if UNITY_EDITOR
            var assetGuids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            foreach (var assetGuid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                var assetName = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(assetName, equalsName, stringComparison))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(path);
                }
            }
            return null;
#endif
            return null;
        }

        public static ScriptableObject FindObject(Type type, string folder = null, string nameContainsPattern = null)
        {
#if UNITY_EDITOR
            var assetGuids = string.IsNullOrEmpty(folder) 
                ? AssetDatabase.FindAssets("t:" + type.Name)  
                : AssetDatabase.FindAssets("t:" + type.Name, new []{folder});
            foreach (var assetGuid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (nameContainsPattern == null)
                    return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                var assetName = Path.GetFileNameWithoutExtension(path);
                if (assetName.Contains(nameContainsPattern))
                    return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            }
            return null;
#endif
            return null;
        }

        public static List<ScriptableObject> FindObjects(Type type, string folder = null, string nameContainsPattern = null)
        {
#if UNITY_EDITOR
            var assetGuids = string.IsNullOrEmpty(folder) 
                ? AssetDatabase.FindAssets("t:" + type.Name) 
                : AssetDatabase.FindAssets("t:" + type.Name, new []{folder});
            
            var assets = new List<ScriptableObject>();
            foreach (var assetGuid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (nameContainsPattern == null)
                    assets.Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(path));
                else
                {
                    var assetName = Path.GetFileNameWithoutExtension(path);
                    if (assetName.Contains(nameContainsPattern))
                        assets.Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(path));
                }
            }
            return assets;
#endif
            return new List<ScriptableObject>();
        }

        public static T FindObject<T>(string folder = null, string nameContainsPattern = null) where T : Object
        {
#if UNITY_EDITOR
            var assetGuids = string.IsNullOrEmpty(folder) 
                ? AssetDatabase.FindAssets("t:" + typeof(T).Name)  
                : AssetDatabase.FindAssets("t:" + typeof(T).Name, new []{folder});
            foreach (var assetGuid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (nameContainsPattern == null)
                    return AssetDatabase.LoadAssetAtPath<T>(path);
                
                var assetName = Path.GetFileNameWithoutExtension(path);
                if (assetName.Contains(nameContainsPattern))
                    return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return null;
#endif
            return null;
        }

        public static List<T> FindObjects<T>(string folder = null, string nameContainsPattern = null) where T : Object
        {
#if UNITY_EDITOR
            var assetGuids = string.IsNullOrEmpty(folder) 
                ? AssetDatabase.FindAssets("t:" + typeof(T).Name) 
                : AssetDatabase.FindAssets("t:" + typeof(T).Name, new []{folder});
            
            var assets = new List<T>();
            foreach (var assetGuid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (nameContainsPattern == null)
                    assets.Add(AssetDatabase.LoadAssetAtPath<T>(path));
                else
                {
                    var assetName = Path.GetFileNameWithoutExtension(path);
                    if (assetName.Contains(nameContainsPattern))
                        assets.Add(AssetDatabase.LoadAssetAtPath<T>(path));
                }
            }
            return assets;
#endif
            return new List<T>();
        }

        public static string GetAssetFolder(ScriptableObject scriptableObject)
        {
#if UNITY_EDITOR
            return Path.GetDirectoryName(AssetDatabase.GetAssetPath(scriptableObject));
#endif
            return null;
        }
    }
}