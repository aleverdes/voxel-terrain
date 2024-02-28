using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TravkinGames
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float LONG_PRESS_DURATION = 0.5f;
        
        [Header("Components")]
        [SerializeField] protected Button _button;
        [SerializeField] protected RectTransform _rectTransform;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onClick;
        [SerializeField] private UnityEvent _onLongPressStart;
        [SerializeField] private UnityEvent<float> _onLongPressUpdate;
        [SerializeField] private UnityEvent _onLongPressClick;
        [SerializeField] private UnityEvent<bool> _onInteractableChanged;
        
        [Header("Other")]
        [SerializeField] private bool _disablePressedEffect = true;
        
        public UnityEvent OnClick => _onClick;
        public UnityEvent OnLongPressStart => _onLongPressStart;
        public UnityEvent<float> OnLongPressUpdate => _onLongPressUpdate;
        public UnityEvent OnLongPressClick => _onLongPressClick;
        public UnityEvent<bool> OnInteractableChanged => _onInteractableChanged;
        
        public Image ButtonGraphic => _button.image;

        private bool _longPressed;
        private bool _isPointerDown;
        private float _pressDuration;

        public bool Interactable
        {
            get => _button.interactable;
            set
            {
                if (_button.interactable == value)
                    return;

                _button.interactable = value;
                _onInteractableChanged?.Invoke(value);
            }
        }

        private void Reset()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(ProcessClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(ProcessClick);
        }

        private void ProcessClick()
        {
            if (_longPressed)
            {
                _longPressed = false;
                _onLongPressClick?.Invoke();
            }
            else
            {
                _onClick.Invoke();
            }
        }

        private void Update()
        {
            UpdateLongPress();
        }

        private void UpdateLongPress()
        {
            if (_isPointerDown)
            {
                _pressDuration += Time.deltaTime;

                if (_pressDuration > LONG_PRESS_DURATION)
                {
                    if (!_longPressed)
                    {
                        _longPressed = true;
                        _onLongPressStart?.Invoke();
                    }

                    _onLongPressUpdate?.Invoke(_pressDuration);
                }
            }
            else
            {
                _pressDuration = 0;
            }
        }

        private void DisablePressedEffect()
        {
            if (!_disablePressedEffect) 
                return;
            if (!EventSystem.current) 
                return;
            if (EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            _longPressed = false;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _isPointerDown = false;
            DisablePressedEffect();
        }
    }
}