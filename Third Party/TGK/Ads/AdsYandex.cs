using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_WEBGL
using GamePush;
#endif

namespace TaigaGames.Ads
{
    public class AdsYandex : MonoBehaviour, IAdsManager
    {
        private readonly HashSet<string> _invokes = new HashSet<string>();
        
        private int _rewardedKeysCounter;

        public void Initialize()
        {
        }

        public bool InterstitialIsLoaded(string adId)
        {
            return true;
        }

        public void ShowInterstitial(string adId)
        {
#if UNITY_WEBGL
            GP_Ads.ShowFullscreen();
#endif
        }

        public bool RewardedIsLoaded(string adId)
        {
            return true;
        }

        public void ShowRewarded(string adId)
        {
#if UNITY_WEBGL
            GP_Ads.ShowRewarded(adId);
            GP_Ads.OnRewardedReward += OnYandexRewardedFinished;

            _rewardedKeysCounter++;
#endif
        }

        private void OnYandexRewardedFinished(string adId)
        {
#if UNITY_WEBGL
            GP_Ads.OnRewardedReward -= OnYandexRewardedFinished;
            _invokes.Add(adId);
#endif
        }

        private void Update()
        {
            foreach (var invoke in _invokes)
            {
                OnRewardedFinished?.Invoke(invoke);
            }
            _invokes.Clear();
        }

        public event Action<string> OnRewardedFinished;
    }
}