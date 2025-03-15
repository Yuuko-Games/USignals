using System;
using System.Collections.Generic;

namespace USignals
{
    public interface ISignal
    {
        event Action OnUpdated;
    }

    public class Signal<T> : IDisposable, ISignal
    {
        private T _value;
        private bool _isEvaluating = false;
        private readonly Func<T> _computeFunc;
        private readonly List<ISignal> _dependencies = new();

        /// <summary>
        /// Event that is triggered when the value of the signal updates.
        /// </summary>
        public event Action OnUpdated;

        /// <summary>
        /// Event that is triggered when the value of the signal value changes.
        /// </summary>
        public event Action OnUpdatedDistinct;

        // Ignore event is never used warning
#pragma warning disable CS0067
        [Obsolete("Use OnUpdatedDistinct instead. This event is deprecated and it has no effect. It will be removed in future versions.", true)]
        public event Action OnChanged;
#pragma warning restore CS0067

        /// <summary>
        /// Value of the signal.
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (_computeFunc != null)
                {
                    throw new InvalidOperationException("Cannot set value on a computed signal");
                }

                if (!_isEvaluating)
                {
                    bool isDifferent = !EqualityComparer<T>.Default.Equals(_value, value);

                    _value = value;

                    OnUpdated?.Invoke();
                    if (isDifferent) OnUpdatedDistinct?.Invoke();
                }
            }
        }

        /// <summary>
        /// Constructor for constant value signals
        /// </summary>
        /// <param name="initialValue">The initial value of the signal.</param>
        public Signal(T initialValue)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Constructor for computed signals.
        /// </summary>
        /// <param name="computeFunc">The function that computes the value of the signal.</param>
        /// <param name="dependencies">
        ///     The signals that this signal depends on.<br /><br />
        ///     It NEEDS to exist ATLEAST ONE DEPENDENCY on the computed signals.<br />
        ///     When any of the dependencies change, the signal will be recomputed.<br /><br />
        ///     The dependencies can be other types of signals,<br />
        ///     for example this can be `Signal{int}` and a dependency be a `Signal{bool}`.
        /// </param>
        /// <exception cref="ArgumentNullException">If dependencies are null or empty</exception>
        public Signal(Func<T> computeFunc, params ISignal[] dependencies)
        {
            _computeFunc = computeFunc;

            if (dependencies?.Length == 0)
            {
                throw new ArgumentNullException("Dependencies cannot be null or empty");
            }

            foreach (var dependency in dependencies)
            {
                if (dependency == null)
                {
                    throw new ArgumentNullException("Dependency cannot be null");
                }

                _dependencies.Add(dependency);
                dependency.OnUpdated += Recompute;
            }

            Recompute(computeFunc);
        }

        /// <returns>The value as a string</returns>
        public override string ToString() => $"{Value}";

        /// <summary>
        /// Recomputes the value based on dependencies
        /// </summary>
        private void Recompute()
        {
            Recompute(_computeFunc);
        }

        /// <summary>
        /// Recomputes the value based on dependencies
        /// </summary>
        /// <param name="computeFunc">The function that computes the value of the signal.</param>
        private void Recompute(Func<T> computeFunc)
        {
            try
            {
                _isEvaluating = true;
                var finalValue = computeFunc();

                bool isDifferent = !EqualityComparer<T>.Default.Equals(_value, finalValue);
                _value = finalValue;

                OnUpdated?.Invoke();
                if (isDifferent) OnUpdatedDistinct?.Invoke();
            }
            finally
            {
                _isEvaluating = false;
            }
        }

        /// <summary>
        /// Triggers the OnUpdated event. It will notify all the child signals that depend on this signal.
        /// </summary>
        public void Refresh()
        {
            OnUpdated?.Invoke();
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Signal()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose the signal and clean dependencies.
        /// </summary>
        public void Dispose()
        {
            foreach (var dependency in _dependencies)
            {
                dependency.OnUpdated -= Recompute;
            }

            _dependencies.Clear();
            OnUpdated = null;
            OnUpdatedDistinct = null;
            _value = default;
        }
    }
}
