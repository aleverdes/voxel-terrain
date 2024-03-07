using UnityEngine;
using UnityEngine.UI;

namespace TaigaGames.Onboarding
{
    public class OnboardingTapToContinue : MonoBehaviour
    {
        [SerializeField] private OnboardingManager _onboarding;
        [SerializeField] private RectTransform _root;
        [SerializeField] private Button _button;

        private void Awake()
        {
            Hide();
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

        public OnboardingTapToContinue Show()
        {
            _root.gameObject.SetActive(true);
            return this;
        }

        public OnboardingTapToContinue Hide()
        {
            _root.gameObject.SetActive(false);
            return this;
        }
    }
}