using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TravkinGames.Saves;
using TravkinGames.Utils;
using UnityEngine;

namespace TravkinGames.Onboarding
{
    public class OnboardingManager : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private List<OnboardingState> _states;
        [SerializeField] private Canvas _canvas;

        [Header("Components")] 
        [SerializeField] private OnboardingArrow _onboardingArrow;
        [SerializeField] private OnboardingVignette _onboardingVignette;
        [SerializeField] private OnboardingNarrator _onboardingNarrator;
        [SerializeField] private OnboardingTapToContinue _onboardingTapToContinue;
        [SerializeField] private OnboardingFinger _onboardingFinger;
        
        public bool InProgress { get; private set; }
        public OnboardingState CurrentState { get; private set; }

        public OnboardingArrow Arrow => _onboardingArrow;
        public OnboardingVignette Vignette => _onboardingVignette;
        public OnboardingNarrator Narrator => _onboardingNarrator;
        public OnboardingTapToContinue TapToContinue => _onboardingTapToContinue;
        public OnboardingFinger Finger => _onboardingFinger;

        private int _currentStateIndex;

        public event Action OnboardingStarted;
        public event Action OnboardingFinished;
        public event Action StateChanged;
        
        public IGameData Data { get; set; }

        private void Reset()
        {
            Initialize();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => Data != null);

            if (Data.OnboardingFinished)
            {
                _root.SetActive(false);
                FinishOnboarding();
            }
            else
            {
                _root.SetActive(true);
                StartOnboarding();
            }
        }

        private void StartOnboarding()
        {
            _currentStateIndex = -1;
            OnboardingStarted?.Invoke();
            Next();
        }

        private void FinishOnboarding()
        {
            Data.OnboardingFinished = true;
            _root.SetActive(false);
            InProgress = false;
            OnboardingFinished?.Invoke();
        }

        public void Next()
        {
            _currentStateIndex++;

            if (_currentStateIndex < _states.Count)
            {
                CurrentState = _states[_currentStateIndex];
                CurrentState.Completed += OnStateCompleted;
                CurrentState.Clear();
                CurrentState.OnStart();
                InProgress = true;
                StateChanged?.Invoke();
            }
            else
            {
                CurrentState.Clear();
                FinishOnboarding();
            }
        }

        private void OnStateCompleted()
        {
            CurrentState.Completed -= OnStateCompleted;
            Next();
        }

        [Button("Initialize")]
        private void Initialize()
        {
            if (!TryGetComponent(out _onboardingArrow))
            {
                _onboardingArrow = gameObject.AddComponent<OnboardingArrow>();
            }
            
            if (!TryGetComponent(out _onboardingVignette))
            {
                _onboardingVignette = gameObject.AddComponent<OnboardingVignette>();
            }
            
            if (!TryGetComponent(out _onboardingNarrator))
            {
                _onboardingNarrator = gameObject.AddComponent<OnboardingNarrator>();
            }
            
            if (!TryGetComponent(out _onboardingTapToContinue))
            {
                _onboardingTapToContinue = gameObject.AddComponent<OnboardingTapToContinue>();
            }
            
            if (!TryGetComponent(out _onboardingFinger))
            {
                _onboardingFinger = gameObject.AddComponent<OnboardingFinger>();
            }
        }
        
        [Button("Fill states")]
        private void Fill()
        {
            _states = GetComponentsInChildren<OnboardingState>().ToList();
        }

        public Vector2 GetRectTransformCenter(Transform t)
        {
            return GetRectTransformCenter((RectTransform)t);
        }
        
        public Vector2 GetRectTransformCenter(RectTransform rectTransform)
        {
            var rt = rectTransform.GetWorldRect();
            var width = rectTransform.rect.width * _canvas.scaleFactor;
            var pos = new Vector2(rt.xMin + width * 0.5f, rt.center.y);
            return pos;
        }
    }
}