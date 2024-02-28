using System.Collections.Generic;
using UnityEngine;

namespace TravkinGames.Navigation
{
    /// <summary>
    /// WIP
    /// </summary>
    public class UINavigationElement : MonoBehaviour
    {
        private static readonly List<UINavigationElement> _elements = new List<UINavigationElement>();

        public bool IsSelected => UINavigationSystem.Current.Selected == this;

        public void OnEnable()
        {
            RegisterNavigationElement(this);
        }
        
        public void OnDisable()
        {
            UnregisterNavigationElement(this);
        }
        
        private static void RegisterNavigationElement(UINavigationElement navigationElement)
        {
            _elements.Add(navigationElement);
        }
        
        private static void UnregisterNavigationElement(UINavigationElement navigationElement)
        {
            _elements.Remove(navigationElement);
        }
        
        public static IEnumerable<UINavigationElement> GetElements()
        {
            return _elements;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatic()
        {
            _elements.Clear();
        }
    }
}