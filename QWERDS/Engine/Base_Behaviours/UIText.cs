using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGameEngine
{
    public enum TextAlignment { Left, Center, Right }
    public enum VerticalAlignment { Top, Center, Bottom }

    public class UIText : Behaviour
    {
        private string _text = string.Empty;
        private SpriteFont _font;

        public string FontPath { get; set; }
        public SpriteFont Font { get => _font; internal set => _font = value; }
        public string Text { get => _text; set => _text = value ?? string.Empty; }
        public Color Color { get; set; } = Color.White;
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;
        public float Scale { get; set; } = 1f;
        public float LayerDepth { get; set; } = 0f;
        public int SortingLayer { get; set; } = 0;

        public UIText() { }

        public UIText(string fontPath, string text = "", Color? color = null, float scale = 1f,
                      TextAlignment alignment = TextAlignment.Left,
                      VerticalAlignment verticalAlignment = VerticalAlignment.Center)
        {
            FontPath = fontPath;
            Text = text;
            Color = color ?? Color.White;
            Scale = scale;
            Alignment = alignment;
            VerticalAlignment = verticalAlignment;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Font == null || string.IsNullOrEmpty(Text)) return;

            Vector2 screenPos = Transform.GetScreenPosition();
            Vector2 screenSize = Transform.GetScreenSize();
            Vector2 textSize = Font.MeasureString(Text) * Scale;

            // Сдвиг, вызванный настройкой Origin (аналогично SpriteRenderer)
            Vector2 originOffset = new Vector2(Transform.Origin.X * screenSize.X, Transform.Origin.Y * screenSize.Y);

            Vector2 offset = Vector2.Zero;

            // Горизонтальное выравнивание
            switch (Alignment)
            {
                case TextAlignment.Center:
                    offset.X = (screenSize.X - textSize.X) * 0.5f;
                    break;
                case TextAlignment.Right:
                    offset.X = screenSize.X - textSize.X;
                    break;
                // Left: offset.X = 0 (уже)
            }

            // Вертикальное выравнивание
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    offset.Y = (screenSize.Y - textSize.Y) * 0.5f;
                    break;
                case VerticalAlignment.Bottom:
                    offset.Y = screenSize.Y - textSize.Y;
                    break;
                // Top: offset.Y = 0 (уже)
            }

            Vector2 finalPos = screenPos - originOffset + offset;

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

        // В класс UIText нужно добавить следующий публичный метод:
        /// <summary>
        /// Возвращает локальную позицию левого края символа на указанном индексе (0..Text.Length)
        /// в эталонных координатах относительно родительского Transform.
        /// </summary>
        public Vector2 GetTextLocalPositionAtIndex(int index)
        {
            if (Font == null) return Vector2.Zero;
            string text = Text ?? "";
            if (index < 0) index = 0;
            if (index > text.Length) index = text.Length;
            string substr = text.Substring(0, index);
            float substrWidth = Font.MeasureString(substr).X * Scale;
            float fullHeight = Font.MeasureString("A").Y * Scale; // высота строки
            Vector2 screenSize = Transform.GetWorldSize();
            Vector2 originOffset = new Vector2(Transform.Origin.X * screenSize.X, Transform.Origin.Y * screenSize.Y);
            // Вычисляем начальную точку выравнивания (как в Draw)
            Vector2 offset = Vector2.Zero;
            switch (Alignment)
            {
                case TextAlignment.Center: offset.X = (screenSize.X - Font.MeasureString(text).X * Scale) * 0.5f; break;
                case TextAlignment.Right: offset.X = screenSize.X - Font.MeasureString(text).X * Scale; break;
            }
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center: offset.Y = (screenSize.Y - fullHeight) * 0.5f; break;
                case VerticalAlignment.Bottom: offset.Y = screenSize.Y - fullHeight; break;
            }
            // Позиция начала строки (локальная, относительно точки Origin)
            Vector2 textStart = -originOffset + offset;
            // Позиция индекса: начало + ширина подстроки
            return textStart + new Vector2(substrWidth, 0);
        }
    }
}