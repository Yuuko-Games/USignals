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

    internal interface IDependencyGraphNode
    {
        bool DependsOn(ISignal target, HashSet<ISignal> visited);
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

    public class Signal<T> : IDisposable, ISignal, IComputedSignal, IDependencyGraphNode
    {
        private const string CircularDependencyMessage = "Circular dependency detected";

        private T _value;
        private bool _isEvaluating = false;
        private bool _isDisposed = false;
        private Func<T> _computeFunc;
        private readonly HashSet<ISignal> _dependencies = new();
        private HashSet<ISignal> _pendingDependencies;

        /// <summary>
        /// Event that is triggered when the value of the signal updates (even if unchanged).
        /// </summary>
        public event Action OnUpdated;

        /// <summary>
        /// Event that is triggered when the value of the signal changes.
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// Value of the signal.
        /// </summary>
        public T Value
        {
            get
            {
                ThrowIfDisposed();
                SignalDependencyTracker.TrackDependency(this);
                return _value;
            }
            set
            {
                ThrowIfDisposed();

                if (_computeFunc != null)
                {
                    throw new InvalidOperationException("Cannot set value on a computed signal");
                }

                if (!_isEvaluating)
                {
                    bool isDifferent = !EqualityComparer<T>.Default.Equals(_value, value);

                    _value = value;

                    OnUpdated?.Invoke();
                    if (isDifferent) OnChanged?.Invoke();
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

            var evaluation = Evaluate(computeFunc, trackingDependencies: true);
            _computeFunc = computeFunc;
            ApplyEvaluation(evaluation.finalValue, evaluation.dependencies);
        }

        /// <returns>The value as a string</returns>
        public override string ToString() => $"{Value}";

        /// <summary>
        /// Recomputes the value based on dependencies
        /// </summary>
        private void Recompute()
        {
            if (_isDisposed)
            {
                return;
            }

            Recompute(_computeFunc);
        }

        /// <summary>
        /// Recomputes the value based on dependencies
        /// </summary>
        /// <param name="computeFunc">The function that computes the value of the signal.</param>
        private void Recompute(Func<T> computeFunc)
        {
            if (_isDisposed)
            {
                return;
            }

            if (computeFunc == null)
            {
                throw new ArgumentNullException(nameof(computeFunc));
            }

            var evaluation = Evaluate(computeFunc, trackingDependencies: _computeFunc != null);
            ApplyEvaluation(evaluation.finalValue, evaluation.dependencies);
        }

        /// <summary>
        /// Triggers the OnUpdated event. It will notify all the child signals that depend on this signal.
        /// </summary>
        public void Refresh()
        {
            ThrowIfDisposed();
            OnUpdated?.Invoke();
        }

        /// <summary>
        /// Updates the compute function for computed signals and recomputes the value.
        /// </summary>
        /// <param name="computeFunc">The new function that computes the value of the signal.</param>
        public void UpdateCompute(Func<T> computeFunc)
        {
            ThrowIfDisposed();

            if (_computeFunc == null)
            {
                throw new InvalidOperationException("Cannot update compute function on a non-computed signal");
            }

            if (computeFunc == null)
            {
                throw new ArgumentNullException(nameof(computeFunc));
            }

            var evaluation = Evaluate(computeFunc, trackingDependencies: true);
            _computeFunc = computeFunc;
            ApplyEvaluation(evaluation.finalValue, evaluation.dependencies);
        }

        /// <summary>
        /// Dispose the signal and clean dependencies.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            ClearDependencies();
            OnUpdated = null;
            OnChanged = null;
            _pendingDependencies = null;
            _computeFunc = null;
            _value = default;
            _isDisposed = true;
        }

        void IComputedSignal.RegisterDependency(ISignal dependency)
        {
            if (dependency == null)
            {
                return;
            }

            if (ReferenceEquals(dependency, this))
            {
                throw new InvalidOperationException(CircularDependencyMessage);
            }

            if (dependency is IDependencyGraphNode dependencyNode &&
                dependencyNode.DependsOn(this, new HashSet<ISignal>()))
            {
                throw new InvalidOperationException(CircularDependencyMessage);
            }

            (_pendingDependencies ?? _dependencies).Add(dependency);
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
            if ((_pendingDependencies ?? _dependencies).Count == 0)
            {
                throw new InvalidOperationException("Computed signal must depend on at least one signal");
            }
        }

        bool IDependencyGraphNode.DependsOn(ISignal target, HashSet<ISignal> visited)
        {
            if (target == null || !visited.Add(this))
            {
                return false;
            }

            foreach (var dependency in _dependencies)
            {
                if (ReferenceEquals(dependency, target))
                {
                    return true;
                }

                if (dependency is IDependencyGraphNode dependencyNode &&
                    dependencyNode.DependsOn(target, visited))
                {
                    return true;
                }
            }

            return false;
        }

        private void ReplaceDependencies(HashSet<ISignal> nextDependencies)
        {
            foreach (var dependency in _dependencies)
            {
                if (!nextDependencies.Contains(dependency))
                {
                    dependency.OnUpdated -= Recompute;
                }
            }

            foreach (var dependency in nextDependencies)
            {
                if (_dependencies.Add(dependency))
                {
                    dependency.OnUpdated += Recompute;
                }
            }

            _dependencies.RemoveWhere(dependency => !nextDependencies.Contains(dependency));
        }

        private (T finalValue, HashSet<ISignal> dependencies) Evaluate(Func<T> computeFunc, bool trackingDependencies)
        {
            bool startedDependencyTracking = false;
            _pendingDependencies = trackingDependencies ? new HashSet<ISignal>() : null;

            try
            {
                _isEvaluating = true;

                if (trackingDependencies)
                {
                    SignalDependencyTracker.Begin(this);
                    startedDependencyTracking = true;
                }

                var finalValue = computeFunc();

                if (trackingDependencies)
                {
                    EnsureHasDependencies();
                }

                return (finalValue, _pendingDependencies);
            }
            finally
            {
                if (startedDependencyTracking)
                {
                    SignalDependencyTracker.End();
                }

                _pendingDependencies = null;
                _isEvaluating = false;
            }
        }

        private void ApplyEvaluation(T finalValue, HashSet<ISignal> dependencies)
        {
            if (dependencies != null)
            {
                ReplaceDependencies(dependencies);
            }

            bool isDifferent = !EqualityComparer<T>.Default.Equals(_value, finalValue);
            _value = finalValue;

            OnUpdated?.Invoke();
            if (isDifferent) OnChanged?.Invoke();
        }

        private void ClearDependencies()
        {
            ((IComputedSignal)this).ClearDependencies();
        }

        private void EnsureHasDependencies()
        {
            ((IComputedSignal)this).EnsureHasDependencies();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(Signal<T>));
            }
        }
    }
}
