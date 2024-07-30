using System;

namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// Encapsulates a value of type <typeparamref name="T"/>, providing notification when the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the value to be encapsulated.</typeparam>
    public class BuildValue<T>
    {
        private T _value;
        
        /// <summary>
        /// Event triggered when the value changes.
        /// </summary>
        public event Action<T> OnValueChanged;
        
        /// <summary>
        /// Gets or sets the encapsulated value. Setting this property to a new value triggers the <see cref="OnValueChanged"/> event.
        /// </summary>
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