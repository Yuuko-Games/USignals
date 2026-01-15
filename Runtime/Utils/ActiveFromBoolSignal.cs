using UnityEngine;

namespace USignals
{
    /// <summary>
    /// Toggles a GameObject's active state from a BoolSignalAsset.
    /// </summary>
    public class ActiveFromBoolSignal : MonoBehaviour
    {
        [Tooltip("Bool signal asset used as the active state source.")]
        [SerializeField] private BoolSignalAsset _source;
        [Tooltip("Invert the signal value before applying it to GameObject.SetActive.")]
        [SerializeField] private bool _invert;
        [Tooltip("When true, updates only when the value changes; otherwise on every update.")]
        [SerializeField] private bool _useChangedEvent = true;

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        /// <summary>
        /// Swap the source signal asset at runtime.
        /// </summary>
        public void SetSource(BoolSignalAsset source)
        {
            if (ReferenceEquals(_source, source))
            {
                return;
            }

            Unbind();
            _source = source;
            Bind();
        }

        private void Bind()
        {
            if (_source == null)
            {
                return;
            }

            if (_useChangedEvent)
            {
                _source.OnChanged += Apply;
            }
            else
            {
                _source.OnUpdated += Apply;
            }

            Apply();
        }

        private void Unbind()
        {
            if (_source == null)
            {
                return;
            }

            _source.OnChanged -= Apply;
            _source.OnUpdated -= Apply;
        }

        private void Apply()
        {
            var active = _source.Value;
            if (_invert)
            {
                active = !active;
            }

            gameObject.SetActive(active);
        }
    }
}
