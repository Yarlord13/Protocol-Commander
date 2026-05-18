using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGameEngine
{
    /// <summary>
    /// Компонент для отрисовки 2D спрайта с учётом масштабирования экрана и привязки (Anchor).
    /// </summary>
    public class SpriteRenderer : Behaviour
    {
        /// <summary>Путь к текстуре относительно папки Content (без расширения).</summary>
        public string TexturePath { get; set; }
        private Texture2D _texture;

        /// <summary>Загруженная текстура (заполняется автоматически в Scene.LoadContent).</summary>
        public Texture2D Texture
        {
            get
            {
                if (_texture == null && !string.IsNullOrEmpty(TexturePath) && Scene.Content != null)
                    _texture = Scene.Content.Load<Texture2D>(TexturePath);
                return _texture;
            }
            internal set => _texture = value;
        }

        /// <summary>Цветовой оттенок спрайта.</summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>Область текстуры для отрисовки (null – вся текстура).</summary>
        public Rectangle? SourceRectangle { get; set; }

        /// <summary>Эффекты отражения спрайта.</summary>
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        /// <summary>Глубина слоя (0.0 – передний план, 1.0 – задний). Используется для сортировки внутри одного SortingLayer.</summary>
        public float LayerDepth { get; set; } = 0f;

        /// <summary>
        /// Слой отрисовки (целое число). Чем больше значение, тем позже рисуется объект (выше).
        /// По умолчанию 0, может быть отрицательным.
        /// </summary>
        public int SortingLayer { get; set; } = 0;

        public SpriteRenderer(string texturePath = null)
        {
            TexturePath = texturePath;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] {GameObject?.Name}: Texture is NULL, path={TexturePath}");
                return;
            }

            Vector2 screenPos = Transform.GetScreenPosition();
            Vector2 screenSize = Transform.GetScreenSize();

            var destinationRect = new Rectangle(
                (int)screenPos.X,
                (int)screenPos.Y,
                (int)screenSize.X,
                (int)screenSize.Y
            );

            Vector2 originInTexture = new Vector2(
                Transform.Origin.X * _texture.Width,
                Transform.Origin.Y * _texture.Height
            );

            spriteBatch.Draw(
                _texture,
                destinationRect,
                SourceRectangle,
                Color,
                Transform.Rotation,
                originInTexture,
                Effects,
                LayerDepth
            );
        }
    }
}