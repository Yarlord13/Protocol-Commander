using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MyGameEngine
{
    /// <summary>
    /// Статический класс, управляющий всеми игровыми объектами, их обновлением и отрисовкой.
    /// Так как в игре одна сцена, он сделан статическим для простоты доступа.
    /// </summary>
    public static class Scene
    {
        private static readonly List<GameObject> _gameObjects = new List<GameObject>();
        private static readonly List<GameObject> _toAdd = new List<GameObject>();
        private static readonly List<GameObject> _toRemove = new List<GameObject>();
        private static readonly List<Behaviour> _pendingStart = new List<Behaviour>();

        public static IReadOnlyList<GameObject> GameObjects => _gameObjects;
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Инициализирует сцену. Должен быть вызван до создания любых объектов.
        /// Обычно вызывается из MySceneBuilder.Build().
        /// </summary>
        public static void Initialize()
        {
            IsInitialized = true;
        }

        /// <summary>
        /// Создаёт новый GameObject с указанным Transform и компонентами, и сразу добавляет его в сцену.
        /// </summary>
        /// <param name="name">Имя объекта.</param>
        /// <param name="transform">Сконфигурированный Transform (если null, создаётся стандартный).</param>
        /// <param name="components">Компоненты, которые нужно добавить к объекту (кроме Transform).</param>
        /// <returns>Созданный GameObject.</returns>
        public static GameObject CreateGameObject(string name = "GameObject", Transform transform = null, params Behaviour[] components)
        {
            var go = new GameObject(name, transform, components);
            AddGameObject(go);
            return go;
        }

        /// <summary>Добавляет существующий GameObject в сцену.</summary>
        public static void AddGameObject(GameObject go)
        {
            System.Diagnostics.Debug.WriteLine($"AddGameObject: {go.Name}");
            _toAdd.Add(go);
        }

        /// <summary>Удаляет GameObject из сцены.</summary>
        public static void RemoveGameObject(GameObject go)
        {
            _toRemove.Add(go);
        }

        internal static void RegisterPendingStart(Behaviour comp)
        {
            if (!comp.Started && !_pendingStart.Contains(comp))
                _pendingStart.Add(comp);
        }

        public static void LoadContent(ContentManager content)
        {
            ProcessPendingAddRemove();

            foreach (var go in _gameObjects)
            {
                // Загрузка текстур для SpriteRenderer
                foreach (var renderer in go.GetComponents<SpriteRenderer>())
                {
                    if (!string.IsNullOrEmpty(renderer.TexturePath))
                    {
                        try
                        {
                            renderer.Texture = content.Load<Texture2D>(renderer.TexturePath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load texture '{renderer.TexturePath}': {ex.Message}");
                        }
                    }
                }

                // Загрузка шрифтов для UIText
                foreach (var text in go.GetComponents<UIText>())
                {
                    if (!string.IsNullOrEmpty(text.FontPath))
                    {
                        try
                        {
                            text.Font = content.Load<SpriteFont>(text.FontPath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load font '{text.FontPath}': {ex.Message}");
                        }
                    }
                }
            }
        }

        public static void Update(GameTime gameTime)
        {
            ProcessPendingAddRemove();

            foreach (var comp in _pendingStart)
            {
                if (comp.GameObject != null && comp.GameObject.ActiveInHierarchy && !comp.Started)
                {
                    comp.Start();
                    comp.Started = true;
                }
            }
            _pendingStart.Clear();

            foreach (var go in _gameObjects)
            {
                if (go.ActiveInHierarchy)
                    go.UpdateComponents(gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            // Собираем все SpriteRenderer и UIText
            var drawables = new List<(object component, int sortingLayer, float layerDepth, Action<SpriteBatch> drawAction)>();

            foreach (var go in _gameObjects)
            {
                if (!go.ActiveInHierarchy)
                    continue;

                foreach (var renderer in go.GetComponents<SpriteRenderer>())
                {
                    if (renderer.Enabled && renderer.Texture != null)
                    {
                        drawables.Add((
                            renderer,
                            renderer.SortingLayer,
                            renderer.LayerDepth,
                            renderer.Draw
                        ));
                    }
                }

                foreach (var text in go.GetComponents<UIText>())
                {
                    if (text.Enabled && text.Font != null && !string.IsNullOrEmpty(text.Text))
                    {
                        drawables.Add((
                            text,
                            text.SortingLayer,
                            text.LayerDepth,
                            text.Draw
                        ));
                    }
                }
            }

            // Сортировка: сначала SortingLayer (по возрастанию), затем LayerDepth (по возрастанию)
            drawables.Sort((a, b) =>
            {
                int layerCompare = a.sortingLayer.CompareTo(b.sortingLayer);
                if (layerCompare != 0)
                    return layerCompare;
                return a.layerDepth.CompareTo(b.layerDepth);
            });

            // Отрисовка в порядке сортировки
            foreach (var item in drawables)
                item.drawAction(spriteBatch);
        }

        public static IEnumerable<T> FindComponentsOfType<T>() where T : Behaviour
        {
            foreach (var go in _gameObjects)
                foreach (var comp in go.GetComponents<T>())
                    yield return comp;
        }

        private static void ProcessPendingAddRemove()
        {
            foreach (var go in _toAdd)
            {
                if (!_gameObjects.Contains(go))
                    _gameObjects.Add(go);
            }
            _toAdd.Clear();

            foreach (var go in _toRemove)
            {
                _gameObjects.Remove(go);
            }
            _toRemove.Clear();
        }
    }
}