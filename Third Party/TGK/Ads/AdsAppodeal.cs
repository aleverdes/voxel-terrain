using System;
#if UNITY_ANDROID || UNITY_IOS
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
#endif
using UnityEngine;

namespace TaigaGames.Ads
{
    public class AdsAppodeal : MonoBehaviour, IAdsManager
#if UNITY_ANDROID || UNITY_IOS
        , IAppodealInitializationListener
#endif
    {
        [SerializeField] private string _androidAppKey;
        [SerializeField] private string _iOSAppKey;
        
        [Header("Ad types")]
        [SerializeField] private bool _interstitial = true;
        [SerializeField] private bool _banner = false;
        [SerializeField] private bool _rewardedVideo = true;
        
        [Header("Debug")]
        [SerializeField] private bool _debug;

        private string _lastAdPlacement;
        private bool _rewardedVideoFinished;
        
        public void Initialize()
        {
#if UNITY_ANDROID || UNITY_IOS
            var adTypes = 0;
            if (_interstitial)
            {
                adTypes |= AppodealAdType.Interstitial;
            }

            if (_banner)
            {
                adTypes |= AppodealAdType.Banner;
            }

            if (_rewardedVideo)
            {
                adTypes |= AppodealAdType.RewardedVideo;
            }
            
            Appodeal.SetTesting(_debug);
            Appodeal.Initialize(Application.platform == RuntimePlatform.Android ? _androidAppKey : _iOSAppKey, adTypes, this);
            Appodeal.MuteVideosIfCallsMuted(true);

            if (_rewardedVideo)
            {
                Appodeal.Cache(AppodealAdType.RewardedVideo);
            }

            if (_interstitial)
            {
                Appodeal.Cache(AppodealAdType.Interstitial);
            }
#endif
        }

        public bool InterstitialIsLoaded(string adId)
        {
#if UNITY_ANDROID || UNITY_IOS
            return Appodeal.IsLoaded(AppodealAdType.Interstitial) && Appodeal.CanShow(AppodealAdType.Interstitial);
#else
            return false;
#endif
        }

        public void ShowInterstitial(string adId)
        {
#if UNITY_ANDROID || UNITY_IOS
            Appodeal.Show(AppodealShowStyle.Interstitial, adId);
            Appodeal.Cache(AppodealAdType.Interstitial);
#endif
        }

        public bool RewardedIsLoaded(string adId)
        {
#if UNITY_ANDROID || UNITY_IOS
            return Appodeal.IsLoaded(AppodealAdType.RewardedVideo) && Appodeal.CanShow(AppodealAdType.RewardedVideo);
#else
            return false;
#endif
        }

        public void ShowRewarded(string adId)
        {
#if UNITY_ANDROID || UNITY_IOS
            Appodeal.Show(AppodealShowStyle.RewardedVideo, adId);
            _lastAdPlacement = adId;
            AppodealCallbacks.RewardedVideo.OnFinished += OnRewardedVideoFinished;
#endif
        }

#if UNITY_ANDROID || UNITY_IOS
        private void OnRewardedVideoFinished(object sender, RewardedVideoFinishedEventArgs e)
        {
            AppodealCallbacks.RewardedVideo.OnFinished -= OnRewardedVideoFinished;
            _rewardedVideoFinished = true;
            Appodeal.Cache(AppodealAdType.RewardedVideo);
        }
#endif

        private void Update()
        {
            if (_rewardedVideoFinished)
            {
                OnRewardedFinished?.Invoke(_lastAdPlacement);
                _lastAdPlacement = null;
                _rewardedVideoFinished = false;
            }
        }

        public event Action<string> OnRewardedFinished;

#if UNITY_ANDROID || UNITY_IOS
        public void OnInitializationFinished(List<string> errors)
        {
            Debug.Log("Appodeal initialization finished");
            
            if (errors == null)
            {
                return;
            }
            
            foreach (var error in errors)
            {
                Debug.LogError(error);
            }
        }
#endif
    }
}