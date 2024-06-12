using System;
using UnityEngine;

namespace TaigaGames.Ads
{
    public class AdsEmpty : MonoBehaviour, IAdsManager
    {
        public void Initialize()
        {
        }

        public bool InterstitialIsLoaded(string adId)
        {
            return false;
        }

        public void ShowInterstitial(string adId)
        {
        }

        public bool RewardedIsLoaded(string adId)
        {
            return false;
        }

        public void ShowRewarded(string adId)
        {
            OnRewardedFinished?.Invoke(adId);
        }

        public event Action<string> OnRewardedFinished;
    }
}