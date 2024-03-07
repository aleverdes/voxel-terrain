using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TaigaGames.Navigation
{
    /// <summary>
    /// WIP
    /// </summary>
    [RequireComponent(typeof(EventSystem))]
    public class UINavigationSystem : MonoBehaviour
    {
        public static UINavigationSystem Current { get; private set; }
        private static readonly List<UINavigationSystem> _navigationSystems = new List<UINavigationSystem>();
        
        [Header("General")]
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField, ReadOnly] private UINavigationElement _selected;
        [SerializeField] private Vector2 _axisWeights = new Vector2(0.25f, 0.75f);

        [Header("Controls")]
        [SerializeField] private bool _useTabButton = true;
        [SerializeField] private bool _useArrowKeys = true;
        
        public UINavigationElement Selected => _selected;
        
        public event SelectedObjectChangedDelegate SelectedObjectChanged;
        
        private void Reset()
        {
            _eventSystem = GetComponent<EventSystem>();
        }

        public void OnEnable()
        {
            RegisterNavigationSystem(this);
        }
        
        public void OnDisable()
        {
            UnregisterNavigationSystem(this);
        }

        private void Update()
        {
            if (_useTabButton)
                ProcessTabButton();
        }

        private void ProcessTabButton()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    SelectPrevious();
                else
                    SelectNext();
        }

        public void SelectNext()
        {
        }

        public void SelectPrevious()
        {
        }

        public void Select(UINavigationElement selected)
        {
            if (_selected == selected)
                return;
            
            var previousSelectedObject = _selected;
            _selected = selected;
            SelectedObjectChanged?.Invoke(selected, previousSelectedObject);
        }
        
        private static void RegisterNavigationSystem(UINavigationSystem navigationSystem)
        {
            _navigationSystems.Add(navigationSystem);
            if (navigationSystem._eventSystem == EventSystem.current)
                Current = navigationSystem;
        }
        
        private static void UnregisterNavigationSystem(UINavigationSystem navigationSystem)
        {
            _navigationSystems.Remove(navigationSystem);
            if (Current == navigationSystem)
                foreach (var other in _navigationSystems)
                    if (other._eventSystem == EventSystem.current)
                        Current = other;
        }
        
        public static void UpdateCurrentNavigation()
        {
            foreach (var navigation in _navigationSystems)
                if (navigation._eventSystem == EventSystem.current)
                    Current = navigation;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatic()
        {
            _navigationSystems.Clear();
            Current = null;
        }
        
        public delegate void SelectedObjectChangedDelegate(UINavigationElement newSelectedObject, UINavigationElement oldSelectedObject);
    }
}