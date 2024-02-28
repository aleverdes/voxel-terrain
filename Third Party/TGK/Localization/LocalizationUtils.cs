using UnityEngine;

namespace TravkinGames.Localization
{
    public static class LocalizationUtils
    {
        public static string Localize(LocalizableString localizableString)
        {
            return localizableString != null ? localizableString.Localize() ?? "<null>" : "<null>";
        }
        
        public static SystemLanguage GetSystemLanguageByCode(string code)
        {
            return code switch
            {
                "en" => SystemLanguage.English,
                "ru" => SystemLanguage.Russian,
                "cs" => SystemLanguage.Czech,
                "da" => SystemLanguage.Danish,
                "nl" => SystemLanguage.Dutch,
                "de" => SystemLanguage.German,
                "el" => SystemLanguage.Greek,
                "fi" => SystemLanguage.Finnish,
                "fr" => SystemLanguage.French,
                "it" => SystemLanguage.Italian,
                "ja" => SystemLanguage.Japanese,
                "ko" => SystemLanguage.Korean,
                "nb" => SystemLanguage.Norwegian,
                "nn" => SystemLanguage.Norwegian,
                "no" => SystemLanguage.Norwegian,
                "pl" => SystemLanguage.Polish,
                "pt" => SystemLanguage.Portuguese,
                "ro" => SystemLanguage.Romanian,
                "es" => SystemLanguage.Spanish,
                "sv" => SystemLanguage.Swedish,
                "tr" => SystemLanguage.Turkish,
                "zh-Hans" => SystemLanguage.ChineseSimplified,
                "zh-CN" => SystemLanguage.ChineseSimplified,
                "zh-Hant" => SystemLanguage.ChineseTraditional,
                "zh-TW" => SystemLanguage.ChineseTraditional,
                _ => SystemLanguage.English
            };
        }
    }
}