using System;

namespace TaigaGames.Ads
{
    public interface IAdsManager
    {
        void Initialize();

        bool InterstitialIsLoaded(string adId);
        void ShowInterstitial(string adId);

        bool RewardedIsLoaded(string adId);
        void ShowRewarded(string adId);
        
        event Action<string> OnRewardedFinished;
    }
}