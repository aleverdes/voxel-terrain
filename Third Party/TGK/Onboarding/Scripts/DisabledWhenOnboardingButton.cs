using UnityEngine;
using UnityEngine.UI;

namespace TravkinGames.Onboarding
{
    [RequireComponent(typeof(Button))]
    public class DisabledWhenOnboardingButton : MonoBehaviour
    {
        [SerializeField] private OnboardingManager _onboarding;
        [SerializeField] private Button _button;

        private void Reset()
        {
            _button = GetComponent<Button>();
            _onboarding = FindObjectOfType<OnboardingManager>();
        }

        private void OnEnable()
        {
            if (_onboarding.InProgress)
            {
                _button.interactable = false;
            }

            _onboarding.OnboardingStarted += OnboardingStarted;
            _onboarding.OnboardingFinished += OnboardingFinished;
        }

        private void OnDisable()
        {
            _onboarding.OnboardingStarted -= OnboardingStarted;
            _onboarding.OnboardingFinished -= OnboardingFinished;
        }

        private void OnboardingStarted()
        {
            _button.interactable = false;
        }

        private void OnboardingFinished()
        {
            _button.interactable = true;
        }
    }
}