using UnityEngine;

namespace TravkinGames.Onboarding
{
    public class OnboardingFinger : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Canvas _canvas;

        private void Reset()
        {
            _canvas = FindObjectOfType<Canvas>();
        }

        private void Awake()
        {
            Hide();
        }

        public OnboardingFinger Show()
        {
            _root.gameObject.SetActive(true);
            return this;
        }

        public OnboardingFinger Hide()
        {
            _root.gameObject.SetActive(false);
            return this;
        }

        public OnboardingFinger SetPosition(Vector2 position)
        {
            _root.anchoredPosition = position * (1f / _canvas.scaleFactor);
            return this;
        }
    }
}