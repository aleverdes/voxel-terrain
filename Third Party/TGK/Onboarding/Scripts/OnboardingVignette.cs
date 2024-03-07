using TaigaGames.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TaigaGames.Onboarding
{
    public class OnboardingVignette : MonoBehaviour
    {
        [SerializeField] private OnboardingManager _onboarding;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _root;
        [SerializeField] private Button _button;

        private void Awake()
        {
            Hide();
        }

        private void Reset()
        {
            _onboarding = FindObjectOfType<OnboardingManager>();
            _canvas = FindObjectOfType<Canvas>();
        }
        
        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            _onboarding.CurrentState.Complete();
        }

        public OnboardingVignette Show()
        {
            _root.gameObject.SetActive(true);
            return this;
        }

        public OnboardingVignette SetPosition(Vector2 position)
        {
            _root.anchoredPosition = position * (1f / _canvas.scaleFactor);
            return this;
        }

        public OnboardingVignette SetScale(Vector2 scale)
        {
            _root.localScale = scale.ToXY0().WithZ(1f);
            return this;
        }

        public OnboardingVignette Hide()
        {
            _root.gameObject.SetActive(false);
            return this;
        }
    }
}