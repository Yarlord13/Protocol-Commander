using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MyGameEngine;
using System;

namespace QWERDS
{
    public class Game1 : Core
    {
        public static Game1 InstanceGame { get; private set; }
        private int _windowedWidth = 1280;
        private int _windowedHeight = 720;
        private bool _isFullscreen;
        private KeyboardState _prevKeyState;

        public Game1() : base("Protocol commander", 1920, 1080, true)
        {
            InstanceGame = this;
            // Настраиваем полноэкранный режим по умолчанию
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _isFullscreen = true;

            // Разрешаем изменение размера окна
            Window.AllowUserResizing = true;
            
            // ВАЖНО: подписываемся на событие, но проверка GraphicsDevice внутри обработчика
            // предотвратит ошибки до инициализации.
            Window.ClientSizeChanged += OnWindowResized;

            Graphics.ApplyChanges();
        }

        private void OnWindowResized(object sender, EventArgs e)
        {
            // Если GraphicsDevice ещё не создан, ничего не делаем.
            if (GraphicsDevice == null)
                return;

            // Сохраняем размеры окна только в оконном режиме
            if (!_isFullscreen)
            {
                _windowedWidth = Window.ClientBounds.Width;
                _windowedHeight = Window.ClientBounds.Height;
            }

            // Обновляем менеджер разрешения, чтобы пересчитать масштаб
            ResolutionManager.Initialize(GraphicsDevice);
        }

        private void ToggleFullScreen()
        {
            _isFullscreen = !_isFullscreen;

            if (_isFullscreen)
            {
                // Сохраняем текущий размер окна перед переходом в полноэкранный режим
                _windowedWidth = Window.ClientBounds.Width;
                _windowedHeight = Window.ClientBounds.Height;

                Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Graphics.IsFullScreen = true;
            }
            else
            {
                Graphics.PreferredBackBufferWidth = _windowedWidth;
                Graphics.PreferredBackBufferHeight = _windowedHeight;
                Graphics.IsFullScreen = false;
            }

            Graphics.ApplyChanges();
            
            // После применения изменений GraphicsDevice уже существует, можно обновить ResolutionManager
            if (GraphicsDevice != null)
                ResolutionManager.Initialize(GraphicsDevice);
        }

        protected override void Initialize()
        {
            // Строим сцену
            MySceneBuilder.Build();

            // Базовая инициализация (создаёт GraphicsDevice)
            base.Initialize();

            // Теперь GraphicsDevice точно готов, инициализируем менеджер разрешения
            ResolutionManager.Initialize(GraphicsDevice);
        }

        protected override void LoadContent()
        {
            System.Diagnostics.Debug.WriteLine("Game1.LoadContent is running");
            MyGameEngine.Scene.LoadContent(Content);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Переключение полноэкранного режима по F11 (однократное срабатывание)
            if (keyboardState.IsKeyDown(Keys.F11) && _prevKeyState.IsKeyUp(Keys.F11))
            {
                ToggleFullScreen();
            }
            _prevKeyState = keyboardState;

            MyGameEngine.Scene.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(26, 26, 26));

            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            MyGameEngine.Scene.Draw(SpriteBatch);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}