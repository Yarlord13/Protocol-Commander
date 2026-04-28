using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MyGameEngine
{
    public class UIButton : Behaviour
    {
        private bool _isHovered;
        private bool _isPressed;
        private MouseState _prevMouseState;

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

            MouseState mouse = Mouse.GetState();
            Vector2 mouseRef = ResolutionManager.ToReference(new Vector2(mouse.X, mouse.Y));

            Vector2 worldPos = Transform.GetWorldPosition();
            Vector2 worldSize = Transform.GetWorldSize();

            Vector2 originOffset = new Vector2(Transform.Origin.X * worldSize.X, Transform.Origin.Y * worldSize.Y);
            Vector2 visualPos = worldPos - originOffset;

            RectangleF bounds = new RectangleF(visualPos.X, visualPos.Y, worldSize.X, worldSize.Y);

            bool isInside = bounds.Contains(mouseRef);
            bool leftPressed = mouse.LeftButton == ButtonState.Pressed;
            bool leftWasPressed = _prevMouseState.LeftButton == ButtonState.Pressed;

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

            if (isInside)
            {
                if (leftPressed && !leftWasPressed)
                {
                    _isPressed = true;
                    OnPointerDown?.Invoke();
                }

                if (!leftPressed && leftWasPressed && _isPressed)
                {
                    _isPressed = false;
                    OnPointerUp?.Invoke();
                    OnClick?.Invoke();
                }

                if (_isPressed && leftPressed)
                {
                    OnPressed?.Invoke();
                }
            }
            else
            {
                if (_isPressed)
                {
                    _isPressed = false;
                }
            }

            if (!leftPressed) _isPressed = false;

            _prevMouseState = mouse;
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