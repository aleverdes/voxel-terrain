using TMPro;
using UnityEngine;

namespace TaigaGames.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMP : LocalizedObject
    {
        public LocalizableString Localization;
        public TMP_Text Label;
        public string StringFormat = "{0}";
        
        private void Reset()
        {
            Label = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (LocalizationManager.Instance)
            {
                Localize();
            }
        }

        public override void Localize()
        {
            Label.text = string.Format(StringFormat, Localization.Localize(LocalizationManager.Instance.CurrentLanguage));
        }
    }
}