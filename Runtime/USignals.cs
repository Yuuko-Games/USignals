using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace USignals
{
    public class Signal<T> : IDisposable
    {
        private T _value;
        private bool _isEvaluating = false;
        private readonly Func<T> _computeFunc;
        public event Action OnChanged;
        private readonly List<Signal<T>> _dependencies = new();

        public T Value
        {
            get => _value;
            set
            {
                if (!_isEvaluating && !EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    NotifyChange();
                }
            }
        }

        // Constructor for constant value signals
        public Signal(T initialValue)
        {
            _value = initialValue;
        }

        // Constructor for computed signals
        public Signal(Func<T> computeFunc, params Signal<T>[] dependencies)
        {
            _computeFunc = computeFunc;

            foreach (var dependency in dependencies)
            {
                ValidateNoCircularDependency(dependency);
                _dependencies.Add(dependency);
                dependency.OnChanged += Recompute;
            }

            Recompute(computeFunc);
        }

        public override string ToString() => $"{Value}";

        private void Recompute()
        {
            Recompute(_computeFunc);
        }

        // Recomputes the value based on dependencies
        private void Recompute(Func<T> computeFunc)
        {
            try
            {
                _isEvaluating = true;
                _value = computeFunc();
                NotifyChange();
            }
            finally
            {
                _isEvaluating = false;
            }

        }

        // Triggers the OnChanged event
        private void NotifyChange()
        {
            OnChanged?.Invoke();
        }

        // Destructor
        ~Signal()
        {
            Dispose();
        }

        // Dispose the signal and clean dependencies
        public void Dispose()
        {
            foreach (var dependency in _dependencies)
            {
                dependency.OnChanged -= Recompute;
            }
            _dependencies.Clear();
            OnChanged = null;
        }

        // Validates that no circular dependencies exist
        private void ValidateNoCircularDependency(Signal<T> dependency)
        {
            if (dependency == this || _dependencies.Contains(dependency))
            {
                throw new InvalidOperationException("Circular dependency detected.");
            }

            foreach (var dep in dependency._dependencies)
            {
                ValidateNoCircularDependency(dep);
            }
        }
    }
}
