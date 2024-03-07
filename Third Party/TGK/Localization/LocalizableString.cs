using UnityEngine;

namespace TaigaGames.Localization
{
    [CreateAssetMenu(menuName = "Localization/Text", fileName = "Localization Text", order = 999)]
    public class LocalizableString : LocalizationObject<string>
    {
        public override string Localize(SystemLanguage language)
        {
            var result = base.Localize(language);
            return string.IsNullOrEmpty(result) ? English : result;
        }
    }
}