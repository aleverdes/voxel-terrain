using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using TaigaGames.Localization;
using TaigaGames.Utils;
using UnityEngine;

#if UNITY_WEBGL
using GamePush;
#endif

namespace TaigaGames.Saves
{
    public abstract class SaveManager<T> : MonoBehaviour where T : class, IGameData, new()
    {
        [SerializeField] protected int MinimumDataVersion;
        [SerializeField] protected float SaveDelay = 5f;
        
        public event Action BeforeDataSaved;
        public event Action AfterDataSaved;
        
        public event Loading AfterDataLoaded;
        
        public event Action<T> AfterNewDataCreated; 

        private float? _saveDelay;

        public T Data { get; private set; }

        protected virtual string DataKey => "data"; 
        
        protected IEnumerator Load()
        {
            yield return new WaitUntilWithUnscaledTime(() => LocalizationManager.Instance, 1f);

            if (!LocalizationManager.Instance)
                throw new Exception("LocalizationManager not found. SaveManager can't be initialized without it.");
            
            var loadingResult = LoadingResult.NewDataCreated;

#if UNITY_WEBGL && !UNITY_EDITOR
            var saveDataJson = GP_Player.GetString(DataKey);
#else
            var saveDataPath = Path.Combine(Application.persistentDataPath, DataKey + ".json");
            var saveDataJson = File.Exists(saveDataPath) ? File.ReadAllText(saveDataPath) : null;
#endif
            
            if (!string.IsNullOrEmpty(saveDataJson))
            {
                try
                {
                    loadingResult = LoadingResult.LoadingFailed;
                    Data = JsonConvert.DeserializeObject<T>(saveDataJson);
                    loadingResult = LoadingResult.SuccessfulLoaded;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (Data == null || Data.Version < MinimumDataVersion)
                CreateNewData();
            
            InitializeLanguage();
            Initialize();
            AfterDataLoaded?.Invoke(Data, loadingResult);
            yield return true;
        }

        protected abstract void Initialize();

        private void InitializeLanguage()
        {
            if (string.IsNullOrEmpty(Data.Language))
            {
#if UNITY_WEBGL
                var lang = GP_Language.Current() switch
                {
                    Language.English => SystemLanguage.English,
                    Language.Russian => SystemLanguage.Russian,
                    Language.Turkish => SystemLanguage.Turkish,
                    Language.French => SystemLanguage.French,
                    Language.Italian => SystemLanguage.Italian,
                    Language.German => SystemLanguage.German,
                    Language.Spanish => SystemLanguage.Spanish,
                    Language.Chineese => SystemLanguage.ChineseSimplified,
                    Language.Portuguese => SystemLanguage.Portuguese,
                    Language.Korean => SystemLanguage.Korean,
                    Language.Japanese => SystemLanguage.Japanese,
                    Language.Arab => SystemLanguage.Arabic,
                    Language.Hindi => SystemLanguage.Hindi,
                    Language.Indonesian => SystemLanguage.Indonesian,
                    _ => SystemLanguage.English
                };
#else
                var lang = SystemLanguage.English;
#endif
                LocalizationManager.Instance.ChangeLanguage(lang);

                Data.Language = lang switch
                {
                    SystemLanguage.English => "en",
                    SystemLanguage.Russian => "ru",
                    SystemLanguage.Turkish => "tr",
                    SystemLanguage.French => "fr",
                    SystemLanguage.Italian => "it",
                    SystemLanguage.German => "de",
                    SystemLanguage.Spanish => "sp",
                    SystemLanguage.ChineseSimplified => "zh-CN",
                    SystemLanguage.ChineseTraditional => "zh-CN",
                    SystemLanguage.Portuguese => "pt",
                    SystemLanguage.Korean => "ko",
                    SystemLanguage.Japanese => "jp",
                    SystemLanguage.Arabic => "ar",
                    SystemLanguage.Hindi => "hi",
                    SystemLanguage.Indonesian => "id",
                    _ => "en"
                };
            }
            else
            {
                LocalizationManager.Instance.ChangeLanguage(Data.Language);
            }
        }

        public void SaveImmediate()
        {
            BeforeDataSaved?.Invoke();
#if UNITY_WEBGL && !UNITY_EDITOR
            GP_Player.Sync(true);
#else
            var saveDataPath = Path.Combine(Application.persistentDataPath, DataKey + ".json");
            var saveDataJson = JsonConvert.SerializeObject(Data);
            File.WriteAllText(saveDataPath, saveDataJson);
#endif
            AfterDataSaved?.Invoke();
        }

        public void SaveWithDelay()
        {
            _saveDelay = SaveDelay;
        }

        public void SaveWithDelay(float saveDelay)
        {
            _saveDelay = saveDelay;
        }

        private void Update()
        {
            if (!_saveDelay.HasValue) 
                return;
            
            _saveDelay -= Time.unscaledDeltaTime;
            if (_saveDelay > 0) 
                return;
            _saveDelay = null;
            
            SaveImmediate();
        }

        [Button("Delete save")]
        public void DeleteSave()
        {
#if UNITY_EDITOR
            var saveDataPath = Path.Combine(Application.persistentDataPath, DataKey + ".json");
            if (File.Exists(saveDataPath))
                File.Delete(saveDataPath);
#endif
        }

        private void CreateNewData()
        {
            Data = new T();
            Data.Initialize();
            Data.Version = MinimumDataVersion;
            AfterNewDataCreated?.Invoke(Data);
        }

        public delegate void Loading(T loadedData, LoadingResult loadingResult);
    }

    public enum LoadingResult
    {
        LoadingFailed,
        SuccessfulLoaded,
        NewDataCreated
    }
}