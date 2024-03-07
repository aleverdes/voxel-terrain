using UnityEngine;
using UnityEngine.UI;

namespace TaigaGames.Ads
{
    [RequireComponent(typeof(Button))]
    public class DisableIfAdsNotLoadedCanvasGroup : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private string _adId = "default";
        
        [Header("Canvas Group Settings")]
        [SerializeField] private bool _changeInteractable = true;
        [SerializeField] private bool _changeBlocksRaycasts = false;
        [SerializeField] private float _changedAlpha = 0.3f;
        
        private void Update()
        {
            if (!AdsManager.Instance || AdsManager.Instance.Manager == null)
            {
                return;
            }

            var isLoaded = AdsManager.Instance.Manager.RewardedIsLoaded(_adId);
            _canvasGroup.interactable = !_changeInteractable || !isLoaded; 
            _canvasGroup.blocksRaycasts = !_changeBlocksRaycasts || !isLoaded; 
            _canvasGroup.alpha = isLoaded ? 1f : _changedAlpha; 
        }
    }
}