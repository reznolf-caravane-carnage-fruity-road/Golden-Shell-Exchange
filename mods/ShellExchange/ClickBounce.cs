using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CCFR.ShellExchange
{
    internal sealed class ClickBounce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Transform _target;
        private Graphic _glow;
        private Vector3 _baseScale = Vector3.one;
        private Color _glowColor = Color.white;
        private float _time;
        private int _state;

        public void Bind(Transform target, Graphic glow)
        {
            _target = target;
            _glow = glow;
            _baseScale = target != null ? target.localScale : Vector3.one;

            if (_glow != null)
            {
                _glowColor = _glow.color;
                _glowColor.a = 0f;
                _glow.color = _glowColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_target == null)
            {
                return;
            }

            _state = 1;
            _time = 0f;
            _target.localScale = _baseScale * 0.88f;

            if (_glow != null)
            {
                _glowColor.a = 0.9f;
                _glow.color = _glowColor;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_target == null)
            {
                return;
            }

            _state = 2;
            _time = 0f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_state == 1)
            {
                _state = 2;
                _time = 0f;
            }
        }

        private void Update()
        {
            if (_target == null)
            {
                return;
            }

            if (_state == 2)
            {
                _time += Time.unscaledDeltaTime / 0.28f;
                float t = Mathf.Clamp01(_time);
                float bounce = 1f + 0.18f * Mathf.Sin(t * Mathf.PI) * (1f - 0.4f * t);
                _target.localScale = _baseScale * bounce;

                if (_glow != null)
                {
                    _glowColor.a = Mathf.Lerp(0.9f, 0f, t);
                    _glow.color = _glowColor;
                }

                if (t >= 1f)
                {
                    _target.localScale = _baseScale;
                    _state = 0;
                }
            }
            else if (_state == 1 && _glow != null)
            {
                _glowColor.a = Mathf.Lerp(_glowColor.a, 1f, Time.unscaledDeltaTime * 18f);
                _glow.color = _glowColor;
            }
        }
    }
}
