using UnityEngine;

namespace TaigaGames.Localization
{
    [CreateAssetMenu(menuName = "Localization/Texture", fileName = "Localization Texture", order = 1000)]
    public class LocalizableTexture : LocalizationObject<Texture2D>
    {
        public override Texture2D Localize(SystemLanguage language)
        {
            var result = base.Localize(language);
            return result ? result : English;
        }
    }
}