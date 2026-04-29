using Microsoft.Xna.Framework;
using System;

namespace MyGameEngine
{
    public class UIButton : Behaviour
    {
        private bool _isHovered;
        private bool _isPressed;

        public bool Interactable { get; set; } = true;

        public event Action OnFocusEnter;
        public event Action OnFocusExit;
        public event Action OnPointerDown;
        public event Action OnPointerUp;
        public event Action OnClick;
        public event Action OnPressed;
        public event Action OnDisable;
        public event Action OnEnable;

        public UIButton() { }

        public UIButton(
            Action onClick = null,
            Action onFocusEnter = null,
            Action onFocusExit = null,
            Action onPointerDown = null,
            Action onPointerUp = null,
            bool interactable = true)
        {
            Interactable = interactable;
            if (onClick != null) OnClick += onClick;
            if (onFocusEnter != null) OnFocusEnter += onFocusEnter;
            if (onFocusExit != null) OnFocusExit += onFocusExit;
            if (onPointerDown != null) OnPointerDown += onPointerDown;
            if (onPointerUp != null) OnPointerUp += onPointerUp;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Interactable) return;

            Vector2 mouseRef = InputManager.MouseReferencePosition;
            Vector2 worldPos = Transform.GetWorldPosition();
            Vector2 worldSize = Transform.GetWorldSize();
            Vector2 originOffset = new Vector2(Transform.Origin.X * worldSize.X, Transform.Origin.Y * worldSize.Y);
            Vector2 visualPos = worldPos - originOffset;
            RectangleF bounds = new RectangleF(visualPos.X, visualPos.Y, worldSize.X, worldSize.Y);

            bool isInside = bounds.Contains(mouseRef);

            // Наведение
            if (isInside && !_isHovered)
            {
                _isHovered = true;
                OnFocusEnter?.Invoke();
            }
            else if (!isInside && _isHovered)
            {
                _isHovered = false;
                OnFocusExit?.Invoke();
            }

            // Нажатия кнопки
            if (InputManager.IsLeftButtonPressed && isInside)
            {
                _isPressed = true;
                OnPointerDown?.Invoke();
            }

            if (InputManager.IsLeftButtonReleased)
            {
                if (_isPressed)
                {
                    OnPointerUp?.Invoke();
                    if (isInside)
                        OnClick?.Invoke();
                }
                _isPressed = false;
            }

            if (_isPressed && InputManager.IsLeftButtonDown)
            {
                OnPressed?.Invoke();
            }
        }

        private struct RectangleF
        {
            public float X, Y, Width, Height;
            public RectangleF(float x, float y, float w, float h)
            { X = x; Y = y; Width = w; Height = h; }
            public bool Contains(Vector2 point)
            { return point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height; }
        }
    }
}