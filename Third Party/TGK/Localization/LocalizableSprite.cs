using UnityEngine;

namespace TravkinGames.Localization
{
    [CreateAssetMenu(menuName = "Localization/Sprite", fileName = "Localization Sprite", order = 1001)]
    public class LocalizableSprite : LocalizationObject<Sprite>
    {
        public override Sprite Localize(SystemLanguage language)
        {
            var result = base.Localize(language);
            return result ? result : English;
        }
    }
}