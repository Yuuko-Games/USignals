using System;
using System.Collections.Generic;

namespace USignals
{
    internal interface IComputedSignal
    {
        void RegisterDependency(ISignal dependency);
        void ClearDependencies();
        void EnsureHasDependencies();
    }

    internal static class SignalDependencyTracker
    {
        [ThreadStatic] private static Stack<IComputedSignal> _stack;

        public static void Begin(IComputedSignal signal)
        {
            _stack ??= new Stack<IComputedSignal>();

            _stack.Push(signal);
        }

        public static void End()
        {
            _stack.Pop();
        }

        public static void TrackDependency(ISignal dependency)
        {
            if (_stack == null || _stack.Count == 0)
            {
                return;
            }

            _stack.Peek().RegisterDependency(dependency);
        }
    }

    public interface ISignal
    {
        event Action OnUpdated;
    }

    public class Signal<T> : IDisposable, ISignal, IComputedSignal
    {
        private T _value;
        private bool _isEvaluating = false;
        private Func<T> _computeFunc;
        private readonly HashSet<ISignal> _dependencies = new();

        /// <summary>
        /// Event that is triggered when the value of the signal updates.
        /// </summary>
        public event Action OnUpdated;

        /// <summary>
        /// Event that is triggered when the value of the signal value changes.
        /// </summary>
        public event Action OnUpdatedDistinct;

        /// <summary>
        /// Value of the signal.
        /// </summary>
        public T Value
        {
            get
            {
                SignalDependencyTracker.TrackDependency(this);
                return _value;
            }
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
        public Signal(Func<T> computeFunc)
        {
            if (computeFunc == null)
            {
                throw new ArgumentNullException(nameof(computeFunc));
            }

            _computeFunc = computeFunc;
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
            if (computeFunc == null)
            {
                throw new ArgumentNullException(nameof(computeFunc));
            }

            bool trackingDependencies = _computeFunc != null;

            try
            {
                _isEvaluating = true;

                if (trackingDependencies)
                {
                    ClearDependencies();
                    SignalDependencyTracker.Begin(this);
                }

                var finalValue = computeFunc();

                if (trackingDependencies)
                {
                    EnsureHasDependencies();
                }

                bool isDifferent = !EqualityComparer<T>.Default.Equals(_value, finalValue);
                _value = finalValue;

                OnUpdated?.Invoke();
                if (isDifferent) OnUpdatedDistinct?.Invoke();
            }
            finally
            {
                if (trackingDependencies)
                {
                    SignalDependencyTracker.End();
                }

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
        /// Updates the compute function for computed signals and recomputes the value.
        /// </summary>
        /// <param name="computeFunc">The new function that computes the value of the signal.</param>
        public void UpdateCompute(Func<T> computeFunc)
        {
            if (_computeFunc == null)
            {
                throw new InvalidOperationException("Cannot update compute function on a non-computed signal");
            }

            if (computeFunc == null)
            {
                throw new ArgumentNullException(nameof(computeFunc));
            }

            _computeFunc = computeFunc;
            Recompute(_computeFunc);
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
            ClearDependencies();
            OnUpdated = null;
            OnUpdatedDistinct = null;
            _value = default;
        }

        void IComputedSignal.RegisterDependency(ISignal dependency)
        {
            if (dependency == null || ReferenceEquals(dependency, this))
            {
                return;
            }

            if (_dependencies.Add(dependency))
            {
                dependency.OnUpdated += Recompute;
            }
        }

        void IComputedSignal.ClearDependencies()
        {
            foreach (var dependency in _dependencies)
            {
                dependency.OnUpdated -= Recompute;
            }

            _dependencies.Clear();
        }

        void IComputedSignal.EnsureHasDependencies()
        {
            if (_dependencies.Count == 0)
            {
                throw new InvalidOperationException("Computed signal must depend on at least one signal");
            }
        }

        private void ClearDependencies()
        {
            ((IComputedSignal)this).ClearDependencies();
        }

        private void EnsureHasDependencies()
        {
            ((IComputedSignal)this).EnsureHasDependencies();
        }
    }
}
