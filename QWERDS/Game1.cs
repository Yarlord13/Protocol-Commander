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

        public Game1() : base("Protocol commander", 1920, 1080, true)
        {
            InstanceGame = this;
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _isFullscreen = true;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnWindowResized;

            // Подписка на системный ввод символов (раскладка учитывается автоматически)
            Window.TextInput += (s, args) => InputManager.FeedTextInput(args.Character);

            Graphics.ApplyChanges();
        }

        private void OnWindowResized(object sender, EventArgs e)
        {
            if (GraphicsDevice == null) return;

            if (!_isFullscreen)
            {
                _windowedWidth = Window.ClientBounds.Width;
                _windowedHeight = Window.ClientBounds.Height;
            }

            ResolutionManager.Initialize(GraphicsDevice);
            // Обновляем вьюпорт
            GraphicsDevice.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        private void ToggleFullScreen()
        {
            _isFullscreen = !_isFullscreen;

            if (_isFullscreen)
            {
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

            if (GraphicsDevice != null)
            {
                ResolutionManager.Initialize(GraphicsDevice);
                GraphicsDevice.Viewport = new Viewport(0, 0, Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);
            }
        }

        protected override void Initialize()
        {
            MySceneBuilder.Build();
            base.Initialize();
            ResolutionManager.Initialize(GraphicsDevice);
            GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        protected override void LoadContent()
        {
            System.Diagnostics.Debug.WriteLine("Game1.LoadContent is running");
            MyGameEngine.Scene.LoadContent(Content);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Обновляем состояния ввода один раз за кадр
            InputManager.Update();

            // Выход по Escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                InputManager.IsKeyPressed(Keys.Escape))
                Exit();

            // Переключение полноэкранного режима по F11 (однократное нажатие)
            if (InputManager.IsKeyPressed(Keys.F11))
            {
                ToggleFullScreen();
            }

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