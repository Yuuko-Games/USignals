using System;
using UnityEngine;

namespace USignals
{
    /// <summary>
    /// Base class for exposing signals as ScriptableObjects to share them safely between scenes and components.
    /// </summary>
    public abstract class SignalAssetBase : ScriptableObject
    {
        /// <summary>
        /// Current value formatted as text for generic UI bindings.
        /// </summary>
        public abstract string ValueAsString { get; }

        /// <summary>
        /// Fired on any update, even if the value is the same.
        /// </summary>
        public abstract event Action OnUpdated;

        /// <summary>
        /// Fired only when the value actually changes.
        /// </summary>
        public abstract event Action OnChanged;
    }

    /// <summary>
    /// Generic ScriptableObject wrapper for a Signal value with an initial default.
    /// </summary>
    public abstract class SignalAsset<T> : SignalAssetBase
    {
        [Tooltip("Initial value used when the asset is first accessed or enabled.")]
        [SerializeField] private T _initialValue;
        private Signal<T> _signal;

        /// <summary>
        /// The runtime signal instance backing this asset.
        /// </summary>
        public Signal<T> Signal
        {
            get
            {
                EnsureSignal();
                return _signal;
            }
        }

        /// <summary>
        /// Convenience getter/setter for the signal value.
        /// </summary>
        public T Value
        {
            get => Signal.Value;
            set => Signal.Value = value;
        }

        public override string ValueAsString
        {
            get
            {
                var value = Signal.Value;
                return value != null ? value.ToString() : string.Empty;
            }
        }

        public override event Action OnUpdated
        {
            add
            {
                EnsureSignal();
                _signal.OnUpdated += value;
            }
            remove
            {
                if (_signal != null)
                {
                    _signal.OnUpdated -= value;
                }
            }
        }

        public override event Action OnChanged
        {
            add
            {
                EnsureSignal();
                _signal.OnChanged += value;
            }
            remove
            {
                if (_signal != null)
                {
                    _signal.OnChanged -= value;
                }
            }
        }

        protected virtual void OnEnable()
        {
            EnsureSignal();
        }

        protected virtual void OnDisable()
        {
            if (_signal != null)
            {
                _signal.Dispose();
                _signal = null;
            }
        }

        private void EnsureSignal()
        {
            if (_signal == null)
            {
                _signal = new Signal<T>(_initialValue);
            }
        }
    }

    /// <summary>
    /// Int signal asset.
    /// </summary>
    [CreateAssetMenu(menuName = "USignals/Signal Int", fileName = "IntSignal")]
    public sealed class IntSignalAsset : SignalAsset<int> { }

    /// <summary>
    /// Float signal asset.
    /// </summary>
    [CreateAssetMenu(menuName = "USignals/Signal Float", fileName = "FloatSignal")]
    public sealed class FloatSignalAsset : SignalAsset<float> { }

    /// <summary>
    /// Bool signal asset.
    /// </summary>
    [CreateAssetMenu(menuName = "USignals/Signal Bool", fileName = "BoolSignal")]
    public sealed class BoolSignalAsset : SignalAsset<bool> { }

    /// <summary>
    /// String signal asset.
    /// </summary>
    [CreateAssetMenu(menuName = "USignals/Signal String", fileName = "StringSignal")]
    public sealed class StringSignalAsset : SignalAsset<string> { }
}
