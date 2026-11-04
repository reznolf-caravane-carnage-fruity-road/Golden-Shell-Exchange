using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CCFR.ShellExchange
{
    internal sealed class EscapeCloser : MonoBehaviour
    {
        private Action _onEscape;

        public void Bind(Action onEscape)
        {
            _onEscape = onEscape;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Plugin.Log.LogInfo("Escape pressed - closing confirmation dialog");
                _onEscape?.Invoke();
            }
        }
    }
}
