#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TaigaGames.Localization
{
    public class LocalizationEditorWindow : EditorWindow
    {
        private string _pathToLocalizationTable;
        private string _pathToLocalizableObjects;
        
        [MenuItem("Tools/Localization")]
        private static void ShowWindow()
        {
            var window = GetWindow<LocalizationEditorWindow>();
            window.titleContent = new GUIContent("Localization");
            window.Show();
        }

        private void OnEnable()
        {
            _pathToLocalizationTable = EditorPrefs.GetString($"{nameof(LocalizationEditorWindow)}/{nameof(_pathToLocalizationTable)}", string.Empty);
            _pathToLocalizableObjects = EditorPrefs.GetString($"{nameof(LocalizationEditorWindow)}/{nameof(_pathToLocalizableObjects)}", string.Empty);
            if (string.IsNullOrEmpty(_pathToLocalizableObjects))
            {
                _pathToLocalizableObjects = "Assets/Localization/Text";
            } 
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Path to localization table (tsv):");
            _pathToLocalizationTable = EditorGUILayout.TextField(_pathToLocalizationTable);

            if (GUILayout.Button("Select File"))
            {
                var folderPath = string.IsNullOrEmpty(_pathToLocalizationTable) ? "" : Path.GetDirectoryName(_pathToLocalizationTable);
                var path = EditorUtility.OpenFilePanel("Localization Table (TSV)", folderPath, "tsv");
                if (!string.IsNullOrEmpty(path))
                {
                    _pathToLocalizationTable = path;
                    EditorPrefs.SetString($"{nameof(LocalizationEditorWindow)}/{nameof(_pathToLocalizationTable)}", _pathToLocalizationTable);
                }
            }

            if (!string.IsNullOrEmpty(_pathToLocalizationTable))
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Folder to Localizable Text:");
                _pathToLocalizableObjects = EditorGUILayout.TextField(_pathToLocalizableObjects);
                
                EditorGUILayout.Space();
                if (GUILayout.Button("Load"))
                {
                    EditorPrefs.SetString($"{nameof(LocalizationEditorWindow)}/{nameof(_pathToLocalizableObjects)}", _pathToLocalizableObjects);

                    if (!File.Exists(_pathToLocalizationTable))
                    {
                        EditorUtility.DisplayDialog("Error", "TSV file with localization doesn't exists at selected path.", "OK");
                        return;
                    }

                    Localize();
                }
            }
        }

        private void Localize()
        {
            var localizableStrings = FindObjects<LocalizableString>();

            var lines = File.ReadAllLines(_pathToLocalizationTable);

            var keys = lines[0].Split("\t", StringSplitOptions.RemoveEmptyEntries);
            var languages = new Dictionary<int, SystemLanguage>();
            for (int i = 1; i < keys.Length; i++)
            {
                languages.Add(i, LocalizationUtils.GetSystemLanguageByCode(keys[i]));
            }
            
            for (int i = 1; i < lines.Length; ++i)
            {
                var values = lines[i].Split("\t", StringSplitOptions.RemoveEmptyEntries);

                if (values.Length == 0)
                {
                    continue;
                }
                
                var assetName = values[0];

                var assetLanguages = new Dictionary<SystemLanguage, string>();
                for (int j = 1; j < values.Length; j++)
                {
                    var languageValue = values[j];
                    assetLanguages.Add(languages[j], languageValue);
                }

                var localizationString = localizableStrings.FirstOrDefault(x => x.name == assetName);
                if (!localizationString)
                {
                    localizationString = CreateInstance<LocalizableString>();

                    if (!Directory.Exists(_pathToLocalizableObjects))
                    {
                        Directory.CreateDirectory(_pathToLocalizableObjects);
                    }
                    
                    AssetDatabase.CreateAsset(localizationString, Path.Combine(_pathToLocalizableObjects, assetName + ".asset"));
                }
                
                var serializedObject = new SerializedObject(localizationString);
                var english = serializedObject.FindProperty("English");
                var russian = serializedObject.FindProperty("Russian");
                var czech = serializedObject.FindProperty("Czech");
                var danish = serializedObject.FindProperty("Danish");
                var dutch = serializedObject.FindProperty("Dutch");
                var german = serializedObject.FindProperty("German");
                var greek = serializedObject.FindProperty("Greek");
                var finnish = serializedObject.FindProperty("Finnish");
                var french = serializedObject.FindProperty("French");
                var italian = serializedObject.FindProperty("Italian");
                var japanese = serializedObject.FindProperty("Japanese");
                var korean = serializedObject.FindProperty("Korean");
                var norwegian = serializedObject.FindProperty("Norwegian");
                var polish = serializedObject.FindProperty("Polish");
                var portuguese = serializedObject.FindProperty("Portuguese");
                var romanian = serializedObject.FindProperty("Romanian");
                var spanish = serializedObject.FindProperty("Spanish");
                var swedish = serializedObject.FindProperty("Swedish");
                var turkish = serializedObject.FindProperty("Turkish");
                var simplifiedChinese = serializedObject.FindProperty("SimplifiedChinese");
                var traditionalChinese = serializedObject.FindProperty("TraditionalChinese");
                
                english.stringValue = assetLanguages[SystemLanguage.English];
                russian.stringValue = assetLanguages[SystemLanguage.Russian];
                czech.stringValue = assetLanguages[SystemLanguage.Czech];
                danish.stringValue = assetLanguages[SystemLanguage.Danish];
                dutch.stringValue = assetLanguages[SystemLanguage.Dutch];
                german.stringValue = assetLanguages[SystemLanguage.German];
                greek.stringValue = assetLanguages[SystemLanguage.Greek];
                finnish.stringValue = assetLanguages[SystemLanguage.Finnish];
                french.stringValue = assetLanguages[SystemLanguage.French];
                italian.stringValue = assetLanguages[SystemLanguage.Italian];
                japanese.stringValue = assetLanguages[SystemLanguage.Japanese];
                korean.stringValue = assetLanguages[SystemLanguage.Korean];
                norwegian.stringValue = assetLanguages[SystemLanguage.Norwegian];
                polish.stringValue = assetLanguages[SystemLanguage.Polish];
                portuguese.stringValue = assetLanguages[SystemLanguage.Portuguese];
                romanian.stringValue = assetLanguages[SystemLanguage.Romanian];
                spanish.stringValue = assetLanguages[SystemLanguage.Spanish];
                swedish.stringValue = assetLanguages[SystemLanguage.Swedish];
                turkish.stringValue = assetLanguages[SystemLanguage.Turkish];
                simplifiedChinese.stringValue = assetLanguages[SystemLanguage.ChineseSimplified];
                traditionalChinese.stringValue = assetLanguages[SystemLanguage.ChineseTraditional];

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                
                EditorUtility.SetDirty(localizationString);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static T[] FindObjects<T>(string folder = null) where T : UnityEngine.Object
        {
            var assetGuids = string.IsNullOrEmpty(folder) 
                ? AssetDatabase.FindAssets("t:" + typeof(T).Name) 
                : AssetDatabase.FindAssets("t:" + typeof(T).Name, new []{folder});
            
            var assets = new T[assetGuids.Length];
            for (var i = 0; i < assetGuids.Length; i++)
            {
                var assetGuid = assetGuids[i];
                assets[i] = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(assetGuid));
            }
            return assets;
        }
    }
}
#endif