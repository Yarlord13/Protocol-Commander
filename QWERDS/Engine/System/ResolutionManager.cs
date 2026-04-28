using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MyGameEngine
{
    /// <summary>
    /// Управляет преобразованием координат из эталонного разрешения (1920x1080) в фактическое разрешение экрана.
    /// Позволяет корректно отображать игру на любых мониторах с сохранением пропорций и центрированием.
    /// </summary>
    public static class ResolutionManager
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        public static float ActualWidth { get; private set; }
        public static float ActualHeight { get; private set; }
        
        /// <summary>
        /// Масштабный коэффициент, сохраняющий пропорции эталонного разрешения.
        /// Все координаты умножаются на этот коэффициент.
        /// </summary>
        public static float Scale { get; private set; }

        /// <summary>
        /// Смещение в пикселях для центрирования игровой области на экране.
        /// </summary>
        public static Vector2 Offset { get; private set; }

        private static bool _initialized = false;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice), "GraphicsDevice не может быть null.");

            ActualWidth = graphicsDevice.Viewport.Width;
            ActualHeight = graphicsDevice.Viewport.Height;

            if (ActualWidth <= 0 || ActualHeight <= 0)
                throw new InvalidOperationException("Некорректные размеры экрана.");

            // Масштаб как минимальное отношение, чтобы вписать эталон в экран без обрезания
            float scaleX = ActualWidth / ReferenceWidth;
            float scaleY = ActualHeight / ReferenceHeight;
            Scale = Math.Min(scaleX, scaleY);

            // Смещение для центрирования итогового прямоугольника
            float scaledWidth = ReferenceWidth * Scale;
            float scaledHeight = ReferenceHeight * Scale;
            Offset = new Vector2(
                (ActualWidth - scaledWidth) / 2f,
                (ActualHeight - scaledHeight) / 2f
            );

            _initialized = true;
        }

        /// <summary>
        /// Преобразует эталонные координаты в экранные с учётом масштаба и центрирования.
        /// </summary>
        public static Vector2 ToScreen(Vector2 referencePosition)
        {
            if (!_initialized)
                throw new InvalidOperationException("ResolutionManager не инициализирован. Вызовите Initialize() перед использованием.");
            return referencePosition * Scale + Offset;
        }

        /// <summary>
        /// Преобразует эталонный размер в экранный (смещение не применяется).
        /// </summary>
        public static Vector2 ToScreenSize(Vector2 referenceSize)
        {
            if (!_initialized)
                throw new InvalidOperationException("ResolutionManager не инициализирован. Вызовите Initialize() перед использованием.");
            return referenceSize * Scale;
        }

        /// <summary>
        /// Преобразует экранные координаты мыши в эталонные.
        /// </summary>
        public static Vector2 ToReference(Vector2 screenPosition)
        {
            if (!_initialized)
                throw new InvalidOperationException("ResolutionManager не инициализирован. Вызовите Initialize() перед использованием.");
            return (screenPosition - Offset) / Scale;
        }
    }
}