using UnityEngine;

namespace TaigaGames.Localization
{
    public abstract class LocalizationObject<T> : ScriptableObject
    {
        public T English;
        public T Russian;
        public T Czech;
        public T Danish;
        public T Dutch;
        public T German;
        public T Greek;
        public T Finnish;
        public T French;
        public T Italian;
        public T Japanese;
        public T Korean;
        public T Norwegian;
        public T Polish;
        public T Portuguese;
        public T Romanian;
        public T Spanish;
        public T Swedish;
        public T Turkish;
        public T SimplifiedChinese;
        public T TraditionalChinese;

        public T Localize()
        {
            return Localize(LocalizationManager.Instance.CurrentLanguage);
        }
        
        public virtual T Localize(SystemLanguage language)
        {
            return language switch
            {
                SystemLanguage.Czech => Czech,
                SystemLanguage.Danish => Danish,
                SystemLanguage.Dutch => Dutch,
                SystemLanguage.English => English,
                SystemLanguage.Finnish => Finnish,
                SystemLanguage.French => French,
                SystemLanguage.German => German,
                SystemLanguage.Greek => Greek,
                SystemLanguage.Italian => Italian,
                SystemLanguage.Japanese => Japanese,
                SystemLanguage.Korean => Korean,
                SystemLanguage.Norwegian => Norwegian,
                SystemLanguage.Polish => Polish,
                SystemLanguage.Portuguese => Portuguese,
                SystemLanguage.Romanian => Romanian,
                SystemLanguage.Russian => Russian,
                SystemLanguage.Spanish => Spanish,
                SystemLanguage.Swedish => Swedish,
                SystemLanguage.Turkish => Turkish,
                SystemLanguage.Chinese => TraditionalChinese,
                SystemLanguage.ChineseSimplified => SimplifiedChinese,
                SystemLanguage.ChineseTraditional => TraditionalChinese,
                _ => English
            };
        }
    }
}