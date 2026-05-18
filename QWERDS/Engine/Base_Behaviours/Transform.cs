using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MyGameEngine
{
    /// <summary>
    /// Режим определения размера по отдельной оси.
    /// </summary>
    public enum SizeMode
    {
        /// <summary>Фиксированный размер, задаётся свойством <see cref="Transform.Size"/>.</summary>
        Fixed,
        /// <summary>Растягивается относительно родителя с отступами, указанными в StretchLeft/Right/Top/Bottom.</summary>
        Stretch
    }

    /// <summary>
    /// Компонент, задающий пространственные свойства объекта.
    /// Поддерживает иерархию, привязку (Anchor) и два режима размера: фиксированный или растянутый.
    /// Аналог RectTransform в Unity. Все координаты задаются в эталонных единицах (1920x1080) 
    /// и автоматически масштабируются через ResolutionManager.
    /// </summary>
    public class Transform : Behaviour
    {
        private Vector2 _position;
        private float _rotation;
        private Vector2 _size = new Vector2(100, 100);
        private Vector2 _origin = Vector2.Zero;
        private Vector2 _anchor = Vector2.Zero;

        private Transform _parent;
        private readonly List<Transform> _children = new List<Transform>();

        private SizeMode _sizeModeX = SizeMode.Fixed;
        private SizeMode _sizeModeY = SizeMode.Fixed;

        private float _stretchLeft;
        private float _stretchRight;
        private float _stretchTop;
        private float _stretchBottom;

        // ========== Свойства позиции и привязки ==========

        /// <summary>
        /// Эталонное смещение от точки привязки <see cref="Anchor"/>.
        /// </summary>
        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        /// <summary>
        /// Точка привязки на родительском прямоугольнике в нормализованных координатах (0..1).
        /// (0,0) – левый верхний угол родителя, (1,1) – правый нижний.
        /// </summary>
        public Vector2 Anchor
        {
            get => _anchor;
            set
            {
                _anchor.X = MathHelper.Clamp(value.X, 0f, 1f);
                _anchor.Y = MathHelper.Clamp(value.Y, 0f, 1f);
            }
        }

        /// <summary>Угол поворота в радианах (положительный – по часовой стрелке).</summary>
        public float Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        /// <summary>
        /// Точка поворота/привязки внутри объекта в нормализованных координатах (0..1) относительно его размера.
        /// </summary>
        public Vector2 Origin
        {
            get => _origin;
            set
            {
                _origin.X = MathHelper.Clamp(value.X, 0f, 1f);
                _origin.Y = MathHelper.Clamp(value.Y, 0f, 1f);
            }
        }

        // ========== Свойства размера ==========

        /// <summary>
        /// Фиксированный размер объекта в эталонных единицах. Используется, когда режим размера – Fixed.
        /// </summary>
        public Vector2 Size
        {
            get => _size;
            set => _size = value;
        }

        /// <summary>Режим размера по оси X (Fixed или Stretch).</summary>
        public SizeMode SizeModeX
        {
            get => _sizeModeX;
            set => _sizeModeX = value;
        }

        /// <summary>Режим размера по оси Y (Fixed или Stretch).</summary>
        public SizeMode SizeModeY
        {
            get => _sizeModeY;
            set => _sizeModeY = value;
        }

        /// <summary>Отступ слева от родительского края (в эталонных единицах) при растяжении по X.</summary>
        public float StretchLeft
        {
            get => _stretchLeft;
            set => _stretchLeft = value;
        }

        /// <summary>Отступ справа от родительского края (в эталонных единицах) при растяжении по X.</summary>
        public float StretchRight
        {
            get => _stretchRight;
            set => _stretchRight = value;
        }

        /// <summary>Отступ сверху от родительского края (в эталонных единицах) при растяжении по Y.</summary>
        public float StretchTop
        {
            get => _stretchTop;
            set => _stretchTop = value;
        }

        /// <summary>Отступ снизу от родительского края (в эталонных единицах) при растяжении по Y.</summary>
        public float StretchBottom
        {
            get => _stretchBottom;
            set => _stretchBottom = value;
        }

        // ========== Иерархия ==========

        /// <summary>Родительский Transform. Null для корневых объектов.</summary>
        public Transform Parent => _parent;
        /// <summary>Список дочерних трансформов.</summary>
        public IReadOnlyList<Transform> Children => _children;

        /// <summary>Устанавливает родительский Transform.</summary>
        /// <param name="parent">Новый родитель (может быть null).</param>
        /// <param name="worldPositionStays">Если true, пытается сохранить мировую позицию объекта.</param>
        public void SetParent(Transform parent, bool worldPositionStays = true)
        {
            if (_parent == parent)
                return;

            Vector2 worldPos = worldPositionStays ? GetWorldPosition() : Vector2.Zero;

            _parent?._children.Remove(this);
            _parent = parent;
            _parent?._children.Add(this);

            // Упрощённая логика сохранения мировой позиции. 
            // Для полноценного пересчёта Anchor/Position требуется обратное преобразование.
        }

        // ========== Методы вычисления мировых координат ==========

        /// <summary>
        /// Возвращает прямоугольник родителя в мировых (эталонных) координатах.
        /// Для корневого объекта возвращает эффективный размер видимой области экрана.
        /// </summary>
        private RectangleF GetParentWorldRect()
        {
            if (_parent != null)
            {
                Vector2 parentPos = _parent.GetWorldPosition();
                Vector2 parentSize = _parent.GetWorldSize();
                return new RectangleF(parentPos.X, parentPos.Y, parentSize.X, parentSize.Y);
            }
            else
            {
                // Для корня используем эффективный размер экрана в эталонных единицах.
                // Это гарантирует, что Anchor и Stretch работают относительно видимой области.
                var eff = ResolutionManager.EffectiveReferenceSize;
                return new RectangleF(0, 0, eff.X, eff.Y);
            }
        }

        /// <summary>
        /// Возвращает мировую позицию верхнего левого угла объекта (до применения Origin) в эталонных единицах.
        /// </summary>
        public Vector2 GetWorldPosition()
        {
            RectangleF parentRect = GetParentWorldRect();

            // Точка привязки на родителе (в эталонных единицах)
            Vector2 anchorPoint = new Vector2(
                parentRect.X + Anchor.X * parentRect.Width,
                parentRect.Y + Anchor.Y * parentRect.Height
            );

            // Добавляем смещение Position (эталонное)
            return anchorPoint + Position;
        }

        /// <summary>
        /// Возвращает мировой размер объекта в эталонных единицах.
        /// Учитывает режимы SizeModeX/Y.
        /// </summary>
        public Vector2 GetWorldSize()
        {
            RectangleF parentRect = GetParentWorldRect();
            float width, height;

            // Ширина
            if (SizeModeX == SizeMode.Fixed)
            {
                width = Size.X;
            }
            else // Stretch
            {
                width = parentRect.Width - StretchLeft - StretchRight;
                if (width < 0) width = 0;
            }

            // Высота
            if (SizeModeY == SizeMode.Fixed)
            {
                height = Size.Y;
            }
            else // Stretch
            {
                height = parentRect.Height - StretchTop - StretchBottom;
                if (height < 0) height = 0;
            }

            return new Vector2(width, height);
        }

        /// <summary>Возвращает фактическую позицию объекта на экране в пикселях.</summary>
        public Vector2 GetScreenPosition()
        {
            return ResolutionManager.ToScreen(GetWorldPosition());
        }

        /// <summary>Возвращает фактический размер объекта в пикселях.</summary>
        public Vector2 GetScreenSize()
        {
            return ResolutionManager.ToScreenSize(GetWorldSize());
        }

        /// <summary>Вспомогательная структура для передачи прямоугольника.</summary>
        private struct RectangleF
        {
            public float X, Y, Width, Height;
            public RectangleF(float x, float y, float w, float h) 
            { 
                X = x; Y = y; Width = w; Height = h; 
            }
        }
    }
}