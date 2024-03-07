using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaigaGames.Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }
        public SystemLanguage CurrentLanguage { get; private set; }

        [SerializeField] private bool _overrideLanguageInEditor;
        [SerializeField] private SystemLanguage _editorLanguage = SystemLanguage.English;

        private readonly HashSet<LocalizedObject> _localizedObjects = new HashSet<LocalizedObject>();

        public event LanguageChanged LanguageChanged;

        private SystemLanguage _prevEditorLanguage;

        protected void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this);

            if (_overrideLanguageInEditor)
            {
                ChangeLanguage(_editorLanguage);
                return;
            }
            
            if (PlayerPrefs.HasKey("Language"))
            {
                var loadedLanguage = (SystemLanguage) PlayerPrefs.GetInt("Language", (int)SystemLanguage.English);
                ChangeLanguage(loadedLanguage);
            }
            else
            {
                ChangeLanguage(Application.systemLanguage);
            }
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }
            
            Instance = null;
        }

        public void ChangeLanguage(string code)
        {
            ChangeLanguage(LocalizationUtils.GetSystemLanguageByCode(code));
        }

        public void ChangeLanguage(SystemLanguage systemLanguage)
        {
            CurrentLanguage = systemLanguage;
            if (!_overrideLanguageInEditor)
            {
                PlayerPrefs.SetInt("Language", (int) CurrentLanguage);
                PlayerPrefs.Save();
            }

            foreach (var localizedObject in _localizedObjects)
            {
                localizedObject.Localize();
            }
            
            LanguageChanged?.Invoke(CurrentLanguage);
        }

        public void AddLocalizedObject(LocalizedObject localizedObject)
        {
            _localizedObjects.Add(localizedObject);
        }

        public void RemoveLocalizedObject(LocalizedObject localizedObject)
        {
            _localizedObjects.Remove(localizedObject);
        }

#if UNITY_EDITOR
        [Button("Change language")]
        public void InvokeChangeLanguage()
        {
            ChangeLanguage(_editorLanguage);
        }
#endif
    }

    public delegate void LanguageChanged(SystemLanguage newLanguage);
}