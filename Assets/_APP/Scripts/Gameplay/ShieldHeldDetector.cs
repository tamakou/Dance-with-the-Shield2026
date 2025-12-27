using System;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace DWS
{
    /// <summary>
    /// Detects when a Meta Interaction SDK Grabbable becomes held (grabbed) by watching GrabPoints.
    /// This avoids relying on version-specific UnityEvent wrappers.
    /// </summary>
    public sealed class ShieldHeldDetector : MonoBehaviour
    {
        [SerializeField] private Grabbable _grabbable;

        [Header("Events")]
        public UnityEvent WhenFirstHeld;
        public UnityEvent WhenReleased;

        public event Action FirstHeld;
        public event Action Released;

        public Grabbable Grabbable
        {
            get => _grabbable;
            set => _grabbable = value;
        }

        public bool IsHeld { get; private set; }

        private bool _everHeld;

        private void Reset()
        {
            // Best-effort auto-wire when added.
            _grabbable = GetComponentInParent<Grabbable>();
        }

        private void Update()
        {
            if (_grabbable == null) return;

            // GrabPoints is documented as "A list of the current grab points" used in transformations.
            // When grabbed, this list becomes non-empty.
            var heldNow = _grabbable.GrabPoints != null && _grabbable.GrabPoints.Count > 0;
            if (heldNow == IsHeld) return;

            IsHeld = heldNow;

            if (IsHeld)
            {
                if (!_everHeld)
                {
                    _everHeld = true;
                    WhenFirstHeld?.Invoke();
                    FirstHeld?.Invoke();
                }
            }
            else
            {
                WhenReleased?.Invoke();
                Released?.Invoke();
            }
        }
    }
}
