using System;
using UnityEngine;

namespace TravkinGames.Onboarding
{
    public abstract class OnboardingState : MonoBehaviour
    {
        [SerializeField] protected OnboardingManager Manager;

        public event Action Completed;

        public bool IsActive => Manager.CurrentState == this;

        protected OnboardingArrow Arrow => Manager.Arrow;
        protected OnboardingVignette Vignette => Manager.Vignette;
        protected OnboardingNarrator Narrator => Manager.Narrator;
        protected OnboardingTapToContinue TapToContinue => Manager.TapToContinue;
        public OnboardingFinger Finger => Manager.Finger;
        
        protected virtual void Reset()
        {
            Manager = FindObjectOfType<OnboardingManager>();
        }

        public void Complete()
        {
            OnCompleted();
            Completed?.Invoke();
        }

        private void Update()
        {
            if (IsActive)
            {
                OnUpdate();
            }
        }

        public void Clear()
        {
            Arrow.Hide();
            Vignette.Hide();
            TapToContinue.Hide();
            Finger.Hide();
            Narrator.Hide();
        }

        public abstract void OnStart();
        public abstract void OnUpdate();
        public abstract void OnCompleted();
    }
}