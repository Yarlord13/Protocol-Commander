using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MyGameEngine
{
    public class UIInputField : Behaviour
    {
        private UIText _textComponent;
        private bool _isFocused;
        private string _currentText = "";
        private int _cursorIndex;
        private int _maxLength = 20;
        private string _placeholder = "";
        private bool _readOnly;

        // Таймеры автоповтора
        private double _backspaceTimer, _deleteTimer, _leftTimer, _rightTimer;
        private const double InitialDelay = 0.5;
        private const double RepeatInterval = 0.05;

        private GameObject _cursor;
        private double _cursorBlinkTimer;

        public event Action<string> OnTextChanged;
        public event Action<string> OnSubmit;

        public string Text
        {
            get => _currentText;
            set
            {
                if (_currentText != value && value.Length <= MaxLength)
                {
                    _currentText = value;
                    _cursorIndex = Math.Clamp(_cursorIndex, 0, _currentText.Length);
                    UpdateDisplayText();
                    OnTextChanged?.Invoke(_currentText);
                }
            }
        }

        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = Math.Max(1, value);
        }

        public string Placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value ?? "";
                UpdateDisplayText();
            }
        }

        public bool ReadOnly
        {
            get => _readOnly;
            set => _readOnly = value;
        }

        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                _isFocused = value;
                UpdateDisplayText();
                if (!_isFocused)
                {
                    _backspaceTimer = _deleteTimer = _leftTimer = _rightTimer = 0;
                }
            }
        }

        public override void Start()
        {
            _textComponent = GameObject?.GetComponent<UIText>();
            _cursorIndex = _currentText.Length;
            UpdateDisplayText();
            CreateCursor();
        }

        private void CreateCursor()
        {
            if (Scene.Content == null)
            {
                System.Diagnostics.Debug.WriteLine("UIInputField: Scene.Content is null, cannot create cursor.");
                return;
            }

            var cursorTransform = new Transform
            {
                Anchor = Vector2.Zero,
                Position = Vector2.Zero,
                Size = new Vector2(2, 20),
                Origin = Vector2.Zero,
                SizeModeX = SizeMode.Fixed,
                SizeModeY = SizeMode.Fixed
            };

            _cursor = Scene.CreateGameObject("Cursor", cursorTransform,
                new SpriteRenderer("Sprites/Panel")
                {
                    Color = Color.White,
                    SortingLayer = 100
                }
            );

            try
            {
                var renderer = _cursor.GetComponent<SpriteRenderer>();
                if (renderer != null && renderer.Texture == null)
                {
                    renderer.Texture = Scene.Content.Load<Texture2D>(renderer.TexturePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load cursor texture: {ex.Message}");
            }

            _cursor.Transform.SetParent(Transform);
            _cursor.ActiveSelf = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Enabled || !Transform.GameObject.ActiveInHierarchy) return;

            double delta = gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 mouseRef = InputManager.MouseReferencePosition;
            Vector2 worldPos = Transform.GetWorldPosition();
            Vector2 worldSize = Transform.GetWorldSize();
            Vector2 originOffset = new Vector2(Transform.Origin.X * worldSize.X, Transform.Origin.Y * worldSize.Y);
            var bounds = new RectangleF(worldPos.X - originOffset.X, worldPos.Y - originOffset.Y, worldSize.X, worldSize.Y);

            // Клик по полю — переключение фокуса
            if (InputManager.IsLeftButtonPressed)
            {
                IsFocused = bounds.Contains(mouseRef);
            }

            if (_isFocused && !ReadOnly)
            {
                ProcessKeyboardInput(delta);
            }

            // Обновление курсора
            if (_cursor != null)
            {
                if (_isFocused)
                {
                    _cursorBlinkTimer += delta;
                    _cursor.ActiveSelf = (_cursorBlinkTimer % 1.0) < 0.5;

                    if (_textComponent != null && _textComponent.Font != null)
                    {
                        Vector2 cursorPos = _textComponent.GetTextLocalPositionAtIndex(_cursorIndex);
                        _cursor.Transform.Position = cursorPos;
                        float textHeight = _textComponent.Font.MeasureString("A").Y * _textComponent.Scale;
                        _cursor.Transform.Size = new Vector2(2, textHeight);
                    }
                }
                else
                {
                    _cursor.ActiveSelf = false;
                    _cursorBlinkTimer = 0;
                }
            }
        }

        private void ProcessKeyboardInput(double delta)
        {
            // 1. Однократные нажатия управляющих клавиш
            if (InputManager.IsKeyPressed(Keys.Enter))
            {
                OnSubmit?.Invoke(_currentText);
                Text = "";
                _cursorIndex = 0;
                return;
            }
            if (InputManager.IsKeyPressed(Keys.Back))
            {
                DeleteCharBeforeCursor();
                _backspaceTimer = 0;
            }
            if (InputManager.IsKeyPressed(Keys.Delete))
            {
                DeleteCharAfterCursor();
                _deleteTimer = 0;
            }
            if (InputManager.IsKeyPressed(Keys.Left))
            {
                MoveCursorLeft();
                _leftTimer = 0;
            }
            if (InputManager.IsKeyPressed(Keys.Right))
            {
                MoveCursorRight();
                _rightTimer = 0;
            }
            if (InputManager.IsKeyPressed(Keys.Home)) _cursorIndex = 0;
            if (InputManager.IsKeyPressed(Keys.End)) _cursorIndex = _currentText.Length;

            // 2. Ввод символов через InputManager (TextInput) — принимаем любые символы, кроме управляющих
            while (InputManager.TryGetTextInput(out char c))
            {
                if (!char.IsControl(c))  // игнорируем управляющие символы (Backspace, Enter и т.п.)
                {
                    InsertChar(c);        // вставляем как есть: раскладка, регистр сохраняются
                }
            }

            // 3. Автоповтор удерживаемых клавиш (Backspace, Delete, стрелки)
            if (InputManager.IsKeyDown(Keys.Back)) HandleRepeatAction(delta, ref _backspaceTimer, DeleteCharBeforeCursor);
            if (InputManager.IsKeyDown(Keys.Delete)) HandleRepeatAction(delta, ref _deleteTimer, DeleteCharAfterCursor);
            if (InputManager.IsKeyDown(Keys.Left)) HandleRepeatAction(delta, ref _leftTimer, MoveCursorLeft);
            if (InputManager.IsKeyDown(Keys.Right)) HandleRepeatAction(delta, ref _rightTimer, MoveCursorRight);
        }

        private void HandleRepeatAction(double delta, ref double timer, Action action)
        {
            timer += delta;
            if (timer >= InitialDelay)
            {
                double excess = timer - InitialDelay;
                int steps = (int)(excess / RepeatInterval) + 1;
                for (int i = 0; i < steps; i++)
                {
                    action();
                }
                timer -= steps * RepeatInterval;
            }
        }

        private void InsertChar(char c)
        {
            if (_currentText.Length >= MaxLength) return;
            _currentText = _currentText.Insert(_cursorIndex, c.ToString());
            _cursorIndex++;
            UpdateDisplayText();
            OnTextChanged?.Invoke(_currentText);
        }

        private void DeleteCharBeforeCursor()
        {
            if (_cursorIndex > 0)
            {
                _currentText = _currentText.Remove(_cursorIndex - 1, 1);
                _cursorIndex--;
                UpdateDisplayText();
                OnTextChanged?.Invoke(_currentText);
            }
        }

        private void DeleteCharAfterCursor()
        {
            if (_cursorIndex < _currentText.Length)
            {
                _currentText = _currentText.Remove(_cursorIndex, 1);
                UpdateDisplayText();
                OnTextChanged?.Invoke(_currentText);
            }
        }

        private void MoveCursorLeft()
        {
            if (_cursorIndex > 0) _cursorIndex--;
        }

        private void MoveCursorRight()
        {
            if (_cursorIndex < _currentText.Length) _cursorIndex++;
        }

        public void Clear()
        {
            Text = "";
            _cursorIndex = 0;
        }

        private void UpdateDisplayText()
        {
            if (_textComponent == null) return;
            _textComponent.Text = (!string.IsNullOrEmpty(_currentText) || _isFocused) ? _currentText : _placeholder ?? "";
        }

        private struct RectangleF
        {
            public float X, Y, Width, Height;
            public RectangleF(float x, float y, float w, float h) { X = x; Y = y; Width = w; Height = h; }
            public bool Contains(Vector2 point) => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
        }
    }
}