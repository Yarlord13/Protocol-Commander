using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGameEngine
{
    public enum TextAlignment { Left, Center, Right }

    public class UIText : Behaviour
    {
        private string _text = string.Empty;
        private SpriteFont _font;

        public string FontPath { get; set; }
        public SpriteFont Font { get => _font; internal set => _font = value; }
        public string Text { get => _text; set => _text = value ?? string.Empty; }
        public Color Color { get; set; } = Color.White;
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public float Scale { get; set; } = 1f;
        public float LayerDepth { get; set; } = 0f;
        public int SortingLayer { get; set; } = 0;

        public UIText() { }

        public UIText(string fontPath, string text = "", Color? color = null, float scale = 1f, TextAlignment alignment = TextAlignment.Left)
        {
            FontPath = fontPath;
            Text = text;
            Color = color ?? Color.White;
            Scale = scale;
            Alignment = alignment;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Font == null || string.IsNullOrEmpty(Text)) return;

            Vector2 screenPos = Transform.GetScreenPosition();
            Vector2 screenSize = Transform.GetScreenSize();
            Vector2 textSize = Font.MeasureString(Text) * Scale;

            Vector2 offset = Vector2.Zero;
            switch (Alignment)
            {
                case TextAlignment.Center:
                    offset.X = (screenSize.X - textSize.X) * 0.5f;
                    offset.Y = (screenSize.Y - textSize.Y) * 0.5f;
                    break;
                case TextAlignment.Right:
                    offset.X = screenSize.X - textSize.X;
                    offset.Y = (screenSize.Y - textSize.Y) * 0.5f;
                    break;
                default:
                    offset.Y = (screenSize.Y - textSize.Y) * 0.5f;
                    break;
            }

            Vector2 finalPos = screenPos + offset;

            spriteBatch.DrawString(
                Font,
                Text,
                finalPos,
                Color,
                Transform.Rotation,
                Vector2.Zero,
                Scale,
                SpriteEffects.None,
                LayerDepth
            );
        }
    }
}