using System;

namespace ImverGames.CustomBuildSettings.Data
{
    public class BuildValue<T>
    {
        private T _value;
        
        public event Action<T> OnValueChanged;
        
        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public BuildValue()
        {
            
        }
    }
}