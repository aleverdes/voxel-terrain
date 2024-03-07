using System;

namespace TaigaGames.Utils
{
    public class EditorData<T>
    {
        private T _value;
        
        public event Action<T> OnValueChanged;
        
        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;
                
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
        
        public EditorData()
        {
            _value = default;
        }
        
        public EditorData(T value)
        {
            _value = value;
        }
        
        public static implicit operator T(EditorData<T> data) => data._value;
    }
}