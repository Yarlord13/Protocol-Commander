using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGameEngine
{
    /// <summary>
    /// Компонент для отрисовки 2D спрайта с учётом масштабирования экрана и привязки (Anchor).
    /// </summary>
    public class SpriteRenderer : Behaviour
    {
        public string TexturePath { get; set; }
        public Texture2D Texture { get; internal set; }
        public Color Color { get; set; } = Color.White;
        public Rectangle? SourceRectangle { get; set; }
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        public float LayerDepth { get; set; } = 0f;

        public SpriteRenderer(string texturePath = null)
        {
            TexturePath = texturePath;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture == null)
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
                Transform.Origin.X * Texture.Width,
                Transform.Origin.Y * Texture.Height
            );

            spriteBatch.Draw(
                Texture,
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