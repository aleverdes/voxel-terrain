using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TaigaGames.Utils
{
    public abstract class BaseDatabase<T> : ScriptableObject where T : ScriptableObject
    {
        [SerializeField] protected List<T> Elements;
        
        private Dictionary<T, int> _indexes;
        private Dictionary<string, T> _byNames;
        
        public void Reset()
        {
            if (!AssetDatabaseUtils.IsCreated(this))
                return;

            Elements = AssetDatabaseUtils.FindObjects<T>();
            Reinitialize();
        }

        [Button("Fill Elements")]
        public void FillWithNewElements()
        {
            var newElements = AssetDatabaseUtils.FindObjects<T>();
            if (newElements.Count == 0)
                return;
            foreach (var newElement in newElements)
                if (!Elements.Contains(newElement))
                    Elements.Add(newElement);
            Reinitialize();
        }

        [ContextMenu("Reinitialize Dictionaries")]
        [Button("Reinitialize Dictionaries")]
        public void Reinitialize()
        {
            _indexes = null;
            _byNames = null;
            InitializeDictionaries();
        }

        public int GetIndexOf(string descriptorName)
        {
            return GetIndexOf(GetElementByName(descriptorName));
        }
        
        public bool TryGetIndexOf(string descriptorName, out int index)
        {
            index = -1;
            if (!TryGetElementByName(descriptorName, out var descriptor))
                return false;
            
            return TryGetIndexOf(descriptor, out index);
        }

        public int GetIndexOf(T descriptor)
        {
            InitializeDictionaries();
            return _indexes[descriptor];
        }
        
        public bool TryGetIndexOf(T descriptor, out int index)
        {
            InitializeDictionaries();
            return _indexes.TryGetValue(descriptor, out index);
        }

        public T GetElementByName(string itemName)
        {
            InitializeDictionaries();
            return _byNames[itemName];
        }
        
        public bool TryGetElementByName(string itemName, out T element)
        {
            InitializeDictionaries();
            return _byNames.TryGetValue(itemName, out element);
        }
        
        public T GetElementByIndex(int index)
        {
            return Elements[index];
        }
        
        public bool TryGetElementByIndex(int index, out T element)
        {
            if (index < 0 || index >= Elements.Count)
            {
                element = null;
                return false;
            }

            element = Elements[index];
            return true;
        }
        
        public IEnumerable<T> GetElements()
        {
            return Elements;
        }
        
        public int GetCount()
        {
            return Elements.Count;
        }

        private void InitializeDictionaries()
        {
            if (_indexes == null)
            {
                _indexes = new Dictionary<T, int>();
                for (ushort i = 0; i < Elements.Count; i++) 
                    if (!_indexes.TryAdd(Elements[i], i))
                        Debug.LogError($"Duplicate element {Elements[i].name} in {name}", this);
            }
            
            if (_byNames == null)
            {
                _byNames = new Dictionary<string, T>();
                for (ushort i = 0; i < Elements.Count; i++)
                    if (!_byNames.TryAdd(Elements[i].name, Elements[i]))
                        Debug.LogError($"Duplicate name {Elements[i].name} in {name}", this);
            }
        }
        
        public T this[int index] => Elements[index];
        public T this[string name] => GetElementByName(name);
    }
}