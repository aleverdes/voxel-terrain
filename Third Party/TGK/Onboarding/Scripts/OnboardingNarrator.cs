using TaigaGames.Localization;
using TaigaGames.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TaigaGames.Onboarding
{
    public class OnboardingNarrator : MonoBehaviour
    {
        [Header("Narrator")]
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _messageTypewritingDelay = 0.01f;

        private string _targetMessage;
        private int _currentMessageLength;
        private float _currentMessageDelay;

        private void Awake()
        {
            Hide();
        }
        
        public OnboardingNarrator Show()
        {
            _root.SetActive(true);
            return this;
        }

        public OnboardingNarrator SetText(LocalizableString localizableString)
        {
            _targetMessage = localizableString.Localize();
            _currentMessageLength = 0;
            _currentMessageDelay = _messageTypewritingDelay;
            return this;
        }

        public OnboardingNarrator SetText(string text)
        {
            _targetMessage = text;
            _currentMessageLength = 0;
            _currentMessageDelay = _messageTypewritingDelay;
            return this;
        }

        public OnboardingNarrator Hide()
        {
            _root.SetActive(false);
            return this;
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(_targetMessage))
            {
                _text.text = "";
                _currentMessageLength = 0;
                _currentMessageDelay = _messageTypewritingDelay;
                return;
            }
            
            if (_currentMessageDelay <= FloatEpsilon.Value)
            {
                _currentMessageLength = Mathf.Min(_currentMessageLength + 3, _targetMessage.Length);
                _currentMessageDelay = _messageTypewritingDelay;
                UpdateText();
                return;
            }

            _currentMessageDelay -= Time.deltaTime;
        }
        
        private void UpdateText()
        {
            _text.text = _targetMessage[.._currentMessageLength];
        }
    }
}