using UnityEngine;
using UnityEngine.UI;

namespace TravkinGames.Ads
{
    [RequireComponent(typeof(Button))]
    public class DisableIfAdsNotLoadedButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private string _adId = "default";
        
        private void Update()
        {
            if (!AdsManager.Instance || AdsManager.Instance.Manager == null)
            {
                return;
            }

            _button.interactable = !AdsManager.Instance.Manager.RewardedIsLoaded(_adId);
        }
    }
}