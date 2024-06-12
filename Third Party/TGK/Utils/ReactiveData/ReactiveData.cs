using System;
using Newtonsoft.Json;

namespace TaigaGames
{
    [Serializable]
    public class ReactiveData<T>
    {
        [JsonProperty("v")] 
        private T _value;
        
        public event OnValueChangedDelegate OnValueChanged;
        
        [JsonIgnore]
        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                var oldValue = _value;
                _value = value;
                OnValueChanged?.Invoke(_value, oldValue);
            }
        }

        public ReactiveData()
        {
            _value = default;
        }
        
        public ReactiveData(T value)
        {
            _value = value;
        }
        
        public static implicit operator T(ReactiveData<T> field) => field._value;
        
        public delegate void OnValueChangedDelegate(T newValue, T oldValue);
    }
}