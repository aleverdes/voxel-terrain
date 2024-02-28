using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TravkinGames.Ads
{
    public class AdsManager : MonoBehaviour
    {
        public static AdsManager Instance { get; private set; }
        public IAdsManager Manager { get; private set; }

        [SerializeField] private AdsEmpty _empty;
        [SerializeField] private AdsAppodeal _appodeal;
        [SerializeField] private AdsYandex _yandex;

        public float TimeWithoutAds;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this);
            
#if UNITY_ANDROID || UNITY_IOS
            Manager = _appodeal;
#elif UNITY_WEBGL
            Manager = _yandex;
#else
            Manager = _empty;
#endif
            
            Manager.Initialize();
        }

        private void Reset()
        {
            Fill();
        }

        [Button("Fill")]
        private void Fill()
        {
            if (!_appodeal)
            {
                if (!gameObject.TryGetComponent(out _appodeal))
                {
                    _appodeal = gameObject.AddComponent<AdsAppodeal>();
                }
            }
            if (!_yandex)
            {
                if (!gameObject.TryGetComponent(out _yandex))
                {
                    _yandex = gameObject.AddComponent<AdsYandex>();
                }
            }
            if (!_empty)
            {
                if (!gameObject.TryGetComponent(out _empty))
                {
                    _empty = gameObject.AddComponent<AdsEmpty>();
                }
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static void ShowRewardedAd(string adId, Action callback)
        {
            Debug.Log("Call ShowRewardedAd");
            Instance.Manager.ShowRewarded(adId);
            Instance.Manager.OnRewardedFinished += OnFinished;
            Instance.TimeWithoutAds = 0;
            
            void OnFinished(string adId)
            {
                Instance.Manager.OnRewardedFinished -= OnFinished;
                Instance.TimeWithoutAds = 0;
                callback?.Invoke();
            }
        }

        public static void ShowInterstitialAd(string adId)
        {
            Debug.Log("Call ShowInterstitialAd");
            Instance.TimeWithoutAds = 0;
            Instance.Manager.ShowInterstitial(adId);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatic()
        {
            Instance = null;
        }
    }
}