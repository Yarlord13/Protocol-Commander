using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MyGameEngine
{
    /// <summary>
    /// Управляет преобразованием координат из эталонного разрешения (1920x1080) в фактическое разрешение экрана.
    /// Позволяет корректно отображать игру на любых мониторах с сохранением пропорций.
    /// </summary>
    public static class ResolutionManager
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        public static float ActualWidth { get; private set; }
        public static float ActualHeight { get; private set; }
        public static float Scale { get; private set; }

        private static bool _initialized = false;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            // Убрана блокировка повторной инициализации, чтобы можно было обновлять размеры
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice), "GraphicsDevice не может быть null.");

            ActualWidth = graphicsDevice.Viewport.Width;
            ActualHeight = graphicsDevice.Viewport.Height;

            if (ActualWidth <= 0 || ActualHeight <= 0)
                throw new InvalidOperationException("Некорректные размеры экрана.");

            Scale = ActualHeight / ReferenceHeight;
            _initialized = true;
        }

        public static Vector2 ToScreen(Vector2 referencePosition)
        {
            if (!_initialized)
                throw new InvalidOperationException("ResolutionManager не инициализирован. Вызовите Initialize() перед использованием.");
            return new Vector2(
                referencePosition.X * Scale,
                referencePosition.Y * Scale
            );
        }

        public static Vector2 ToScreenSize(Vector2 referenceSize)
        {
            if (!_initialized)
                throw new InvalidOperationException("ResolutionManager не инициализирован. Вызовите Initialize() перед использованием.");
            return referenceSize * Scale;
        }

        public static Vector2 ToReference(Vector2 screenPosition)
        {
            if (!_initialized)
                throw new InvalidOperationException("ResolutionManager не инициализирован. Вызовите Initialize() перед использованием.");
            return new Vector2(
                screenPosition.X / Scale,
                screenPosition.Y / Scale
            );
        }
    }
}