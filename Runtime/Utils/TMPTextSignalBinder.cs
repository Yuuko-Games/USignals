using TMPro;
using UnityEngine;

namespace USignals
{
    /// <summary>
    /// Binds a TMP_Text to a SignalAsset and updates its text when the signal changes.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TMPTextSignalBinder : MonoBehaviour
    {
        [Tooltip("Signal asset used as a data source for the text.")]
        [SerializeField] private SignalAssetBase _source;
        [Tooltip("String.Format pattern. Use {0} to insert the signal value.")]
        [SerializeField] private readonly string _format = "{0}";
        [Tooltip("When true, updates only when the value changes; otherwise on every update.")]
        [SerializeField] private readonly bool _useChangedEvent = true;

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

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
        public void SetSource(SignalAssetBase source)
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
            if (_source == null || _text == null)
            {
                return;
            }

            if (_useChangedEvent)
            {
                _source.OnChanged += UpdateText;
            }
            else
            {
                _source.OnUpdated += UpdateText;
            }

            UpdateText();
        }

        private void Unbind()
        {
            if (_source == null)
            {
                return;
            }

            _source.OnChanged -= UpdateText;
            _source.OnUpdated -= UpdateText;
        }

        private void UpdateText()
        {
            if (_text == null || _source == null)
            {
                return;
            }

            _text.text = string.Format(_format, _source.ValueAsString);
        }
    }
}
