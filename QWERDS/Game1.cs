using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MyGameEngine;

namespace QWERDS
{
    public class Game1 : Core
    {
        public Game1() : base("Protocol commander", 1920, 1080, true)
        {
            // Настройка полного экрана с максимальным разрешением монитора
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            // Применяем изменения сразу, чтобы окно создалось с нужными параметрами
            Graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // Строим сцену (все координаты и размеры будут пересчитаны автоматически)
            MySceneBuilder.Build();

            // Сначала вызываем базовую инициализацию, чтобы GraphicsDevice стал доступен
            base.Initialize();

            // Теперь можно инициализировать менеджер разрешения
            ResolutionManager.Initialize(GraphicsDevice);
        }

        //private Texture2D _logo;
        protected override void LoadContent()
        {
            System.Diagnostics.Debug.WriteLine("Game1.LoadContent is running");
            MyGameEngine.Scene.LoadContent(Content);
            //_logo = Content.Load<Texture2D>("Sprites/RobotGG");
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MyGameEngine.Scene.Update(gameTime);
            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(26, 26, 26));

            SpriteBatch.Begin();
            MyGameEngine.Scene.Draw(SpriteBatch);
            //SpriteBatch.Draw(_logo, Vector2.Zero, Color.White);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}