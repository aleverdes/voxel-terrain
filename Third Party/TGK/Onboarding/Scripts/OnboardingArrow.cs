using System;
using TravkinGames.Utils;
using UnityEngine;

namespace TravkinGames.Onboarding
{
    public class OnboardingArrow : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _root;
        
        [Header("Arrows")]
        [SerializeField] private RectTransform _toTop;
        [SerializeField] private RectTransform _toBottom;
        [SerializeField] private RectTransform _toLeft;
        [SerializeField] private RectTransform _toRight;
        
        [Header("Animation")]
        [SerializeField] private float _amplitude = 50f;
        [SerializeField] private float _speed = 2f;

        private void Reset()
        {
            _canvas = FindObjectOfType<Canvas>();
        }

        private void Awake()
        {
            Hide();
        }

        public OnboardingArrow Show()
        {
            _root.gameObject.SetActive(true);
            return this;
        }

        public OnboardingArrow SetDirection(Direction direction)
        {
            _toLeft.gameObject.SetActive(false);
            _toRight.gameObject.SetActive(false);
            _toBottom.gameObject.SetActive(false);
            _toTop.gameObject.SetActive(false);

            switch (direction)
            {
                case Direction.ToLeft:
                    _toLeft.gameObject.SetActive(true);
                    break;
                case Direction.ToRight:
                    _toRight.gameObject.SetActive(true);
                    break;
                case Direction.ToBottom:
                    _toBottom.gameObject.SetActive(true);
                    break;
                case Direction.ToTop:
                    _toTop.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            return this;
        }

        public OnboardingArrow SetPosition(Vector2 position)
        {
            _root.anchoredPosition = position * (1f / _canvas.scaleFactor);
            return this;
        }
        
        public OnboardingArrow Hide()
        {
            _root.gameObject.SetActive(false);
            return this;
        }

        private void Update()
        {
            var t = _speed * Time.time;
            var sin = Mathf.Sin(t);
            var a = Mathf.Abs(sin);
            
            _toTop.anchoredPosition = _toTop.anchoredPosition.WithY(-_amplitude * a);
            _toBottom.anchoredPosition = _toBottom.anchoredPosition.WithY(_amplitude * a);
            
            _toLeft.anchoredPosition = _toLeft.anchoredPosition.WithX(-_amplitude * a);
            _toRight.anchoredPosition = _toRight.anchoredPosition.WithX(_amplitude * a);
        }

        public enum Direction
        {
            ToLeft,
            ToRight,
            ToBottom,
            ToTop
        }
    }
}