using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MyGameEngine
{
    /// <summary>
    /// Управляет масштабированием эталонного разрешения (1920x1080) на реальное окно.
    /// Обеспечивает вписывание игрового поля с сохранением пропорций (letterboxing/pillarboxing).
    /// </summary>
    public static class ResolutionManager
    {
        /// <summary>Эталонная ширина игрового поля.</summary>
        public const float ReferenceWidth = 1920f;
        
        /// <summary>Эталонная высота игрового поля.</summary>
        public const float ReferenceHeight = 1080f;

        /// <summary>Текущая ширина окна в физических пикселях.</summary>
        public static float ActualWidth { get; private set; }
        
        /// <summary>Текущая высота окна в физических пикселях.</summary>
        public static float ActualHeight { get; private set; }
        
        /// <summary>Единый коэффициент масштабирования для вписывания эталонного разрешения.</summary>
        public static float Scale { get; private set; }
        
        /// <summary>Смещение отрисовки для центрирования игрового поля на экране.</summary>
        public static Vector2 Offset { get; private set; }

        /// <summary>
        /// Эффективный размер видимой области в эталонных единицах.
        /// Равен (ActualWidth / Scale, ActualHeight / Scale). 
        /// При изменении соотношения сторон одна из координат увеличивается, 
        /// что позволяет UI растягиваться на всю видимую область экрана.
        /// </summary>
        public static Vector2 EffectiveReferenceSize { get; private set; }

        private static bool _initialized = false;

        /// <summary>Инициализирует менеджер на основе текущего состояния графического устройства.</summary>
        /// <param name="graphicsDevice">Активный GraphicsDevice.</param>
        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            ActualWidth = graphicsDevice.Viewport.Width;
            ActualHeight = graphicsDevice.Viewport.Height;

            if (ActualWidth <= 0 || ActualHeight <= 0)
                throw new InvalidOperationException("Invalid screen dimensions.");

            float scaleX = ActualWidth / ReferenceWidth;
            float scaleY = ActualHeight / ReferenceHeight;
            Scale = Math.Min(scaleX, scaleY);

            float scaledWidth = ReferenceWidth * Scale;
            float scaledHeight = ReferenceHeight * Scale;
            
            Offset = new Vector2(
                (ActualWidth - scaledWidth) / 2f,
                (ActualHeight - scaledHeight) / 2f
            );

            // Эффективный размер = фактический размер окна, делённый на масштаб.
            // Представляет собой видимую область в эталонных координатах.
            EffectiveReferenceSize = new Vector2(ActualWidth / Scale, ActualHeight / Scale);

            _initialized = true;
        }

        /// <summary>Преобразует эталонные координаты в экранные пиксели.</summary>
        public static Vector2 ToScreen(Vector2 referencePosition)
        {
            if (!_initialized) throw new InvalidOperationException("ResolutionManager not initialized.");
            return referencePosition * Scale;
        }

        /// <summary>Преобразует эталонный размер в экранные пиксели.</summary>
        public static Vector2 ToScreenSize(Vector2 referenceSize)
        {
            if (!_initialized) throw new InvalidOperationException("ResolutionManager not initialized.");
            return referenceSize * Scale;
        }

        /// <summary>Преобразует экранные координаты мыши в эталонные (с учётом letterboxing).</summary>
        public static Vector2 ToReference(Vector2 screenPosition)
        {
            if (!_initialized) throw new InvalidOperationException("ResolutionManager not initialized.");
            return screenPosition / Scale;
        }
    }
}