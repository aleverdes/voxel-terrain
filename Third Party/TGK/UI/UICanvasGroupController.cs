using System;
using TravkinGames.Utils;
using UnityEngine;

namespace TravkinGames
{
    /// <summary>
    /// Контроллер для управления CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UICanvasGroupController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Header("Content")]
        [SerializeField] private bool _disableContentIfHidden = true;
        [SerializeField] private GameObject _content;

        [Header("Showing Animation")] 
        [SerializeField] private UIAnimationPreset _animationPreset;

        [Header("Showing Settings")] 
        [SerializeField] private bool _useUnscaledDeltaTime;
        [SerializeField] private bool _isShownAtStart;
        
        private float _previousShowingProgress;
        private float _currentShowingProgress;
        private bool _isShown;

        /// <summary>
        /// Показывается ли в данный момент данный CanvasGroup-контроллер.
        /// </summary>
        public bool IsShown => _isShown;
        
        /// <summary>
        /// Виден ли в данный момент содержимое данного CanvasGroup-контроллера.
        /// </summary>
        public bool IsVisible => _currentShowingProgress > FloatEpsilon.Value;
        
        /// <summary>
        /// Прогресс показа/скрытия данного CanvasGroup-контроллера.
        /// </summary>
        public float ShowingProgress => _currentShowingProgress;

        /// <summary>
        /// Происходит ли в данный момент скрытие данного CanvasGroup-контроллера.
        /// </summary>
        public bool IsHidingInProgress => _currentShowingProgress < _previousShowingProgress;
        
        /// <summary>
        /// Происходит ли в данный момент показ данного CanvasGroup-контроллера.
        /// </summary>
        public bool IsShowingInProgress => _currentShowingProgress > _previousShowingProgress;
        
        /// <summary>
        /// Событие, происходящее в момент начала показа данного CanvasGroup-контроллера.
        /// </summary>
        public event Action ShowingStarted;
        
        /// <summary>
        /// Событие, происходящее в момент начала скрытия данного CanvasGroup-контроллера.
        /// </summary>
        public event Action HidingStarted;
        
        /// <summary>
        /// Событие, происходящее в момент завершения показа данного CanvasGroup-контроллера.
        /// </summary>
        public event Action ShowingCompleted;
        
        /// <summary>
        /// Событие, происходящее в момент завершения скрытия данного CanvasGroup-контроллера.
        /// </summary>
        public event Action HidingCompleted;
        
        private void Reset()
        {
            _canvasGroup = GetComponentInChildren<CanvasGroup>();

            switch (transform.childCount)
            {
                case 1:
                    _content = transform.GetChild(0).gameObject;
                    break;
                case 0:
                    _content = new GameObject("Content");
                    var rt = _content.AddComponent<RectTransform>();
                    rt.SetParent(transform);
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;
                    break;
                default:
                    Debug.LogError("UICanvasGroupController can have only one child and its should be a Content", this);
                    break;
            }
        }

        private void Awake()
        {
            if (transform.childCount == 0)
            {
                Debug.LogError("UICanvasGroupController should have a child", this);
                enabled = false;
                return;
            }
            
            if (transform.childCount > 1)
            {
                Debug.LogError("UICanvasGroupController can have only one child", this);
                enabled = false;
                return;
            }

            if (_content != transform.GetChild(0).gameObject)
            {
                Debug.LogError("UICanvasGroupController's Content should be the child", this);
                enabled = false;
                return;
            }
            
            _isShown = _isShownAtStart;
            _currentShowingProgress = _isShownAtStart ? 1f : 0;
            _previousShowingProgress = _currentShowingProgress;
            UpdateShowingState();
        }

        private void Update()
        {
            var dt = _useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _currentShowingProgress = Mathf.Clamp01(_currentShowingProgress + _animationPreset.Speed * _isShown.ToSign() * dt);
            UpdateShowingState();
        }

        /// <summary>
        /// Обновить состояние показа данного CanvasGroup-контроллера.
        /// </summary>
        private void UpdateShowingState()
        {
            _canvasGroup.alpha = Easings.Calculate(_currentShowingProgress, _animationPreset.EasingType);
            _canvasGroup.interactable = _canvasGroup.alpha > FloatEpsilon.Value;
            _canvasGroup.blocksRaycasts = _canvasGroup.alpha > FloatEpsilon.Value;
            
            if (_disableContentIfHidden)
            {
                if (!_content.activeSelf && IsVisible)
                    _content.SetActive(true);
                    
                if (_content.activeSelf && !IsVisible)
                    _content.SetActive(false);
            }
            
            if (IsHidingInProgress && _previousShowingProgress > FloatEpsilon.Value && _currentShowingProgress < FloatEpsilon.Value)
                HidingCompleted?.Invoke();
            
            if (IsShowingInProgress && _previousShowingProgress < 1f - FloatEpsilon.Value && _currentShowingProgress > 1f - FloatEpsilon.Value)
                ShowingCompleted?.Invoke();
        }

        /// <summary>
        /// Показать данный CanvasGroup-контроллер.
        /// </summary>
        public void Show()
        {
            var prevIsShown = _isShown;
            _isShown = true;
            if (_isShown != prevIsShown)
                ShowingStarted?.Invoke();
        }
        
        /// <summary>
        /// Показать данный CanvasGroup-контроллер немедленно.
        /// </summary>
        public void ShowImmediate()
        {
            var prevIsShown = _isShown;
            _isShown = true;
            
            _currentShowingProgress = 1f;
            _previousShowingProgress = _currentShowingProgress;
            
            UpdateShowingState();

            if (_isShown != prevIsShown)
            {
                ShowingStarted?.Invoke();
                ShowingCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Скрыть данный CanvasGroup-контроллер.
        /// </summary>
        public void Hide()
        {
            var prevIsShown = _isShown;
            _isShown = false;
            if (_isShown != prevIsShown)
                HidingStarted?.Invoke();
        }

        /// <summary>
        /// Скрыть данный CanvasGroup-контроллер немедленно.
        /// </summary>
        public void HideImmediate()
        {
            var prevIsShown = _isShown;
            _isShown = false;
            
            _currentShowingProgress = 0f;
            _previousShowingProgress = _currentShowingProgress;
            
            UpdateShowingState();

            if (_isShown != prevIsShown)
            {
                HidingStarted?.Invoke();
                HidingCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Переключить состояние показа/скрытия данного CanvasGroup-контроллера.
        /// </summary>
        public void Switch()
        {
            if (_isShown)
                Hide();
            else
                Show();
        }
    }
}