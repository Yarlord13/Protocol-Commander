using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace MyGameEngine
{
    /// <summary>
    /// Базовый игровой объект, который может содержать компоненты (Behaviour).
    /// Всегда имеет компонент Transform.
    /// </summary>
    public class GameObject
    {
        private readonly List<Behaviour> _components = new List<Behaviour>();
        private bool _activeSelf = true;
        private Transform _transform;

        /// <summary>Имя объекта (для отладки и поиска).</summary>
        public string Name { get; set; }

        /// <summary>Компонент Transform этого объекта (всегда существует).</summary>
        public Transform Transform => _transform;

        /// <summary>
        /// Локальная активность объекта. Если false, объект и все его компоненты не обновляются и не рисуются.
        /// При изменении вызываются OnEnable/OnDisable у компонентов, реализующих IActivatable.
        /// </summary>
        public bool ActiveSelf
        {
            get => _activeSelf;
            set
            {
                if (_activeSelf == value)
                    return;
                _activeSelf = value;
                foreach (var comp in _components)
                {
                    if (comp is IActivatable activatable)
                    {
                        if (value)
                            activatable.OnEnable();
                        else
                            activatable.OnDisable();
                    }
                }
                if (_activeSelf && ActiveInHierarchy)
                {
                    // Запускаем Start себе и всем дочерним объектам
                    RegisterStartRecursive();
                }
            }
        }

        private void RegisterStartRecursive()
        {
            foreach (var comp in _components)
            {
                if (!comp.Started)
                    Scene.RegisterPendingStart(comp);
            }
            if (Transform != null)
            {
                foreach (var child in Transform.Children)
                {
                    child.GameObject?.RegisterStartRecursive();
                }
            }
        }

        /// <summary>
        /// Активен ли объект с учётом иерархии: он сам активен и все его родители активны.
        /// </summary>
        public bool ActiveInHierarchy => ActiveSelf && (Transform.Parent?.GameObject?.ActiveInHierarchy ?? true);

        /// <summary>
        /// Конструктор GameObject.
        /// </summary>
        /// <param name="name">Имя объекта.</param>
        /// <param name="transform">
        /// Уже сконфигурированный Transform. Если null, будет создан Transform с параметрами по умолчанию.
        /// </param>
        /// <param name="components">Дополнительные компоненты (кроме Transform).</param>
        public GameObject(string name = "GameObject", Transform transform = null, params Behaviour[] components)
        {
            Name = name;

            // 1. Устанавливаем Transform (либо переданный, либо новый)
            if (transform != null)
            {
                _transform = transform;
                _transform.GameObject = this;
            }
            else
            {
                _transform = new Transform();
                _transform.GameObject = this;
            }

            _components.Add(_transform);

            // Если сцена уже инициализирована и объект активен, регистрируем Start для Transform
            if (Scene.IsInitialized && ActiveInHierarchy)
                Scene.RegisterPendingStart(_transform);

            // 2. Добавляем остальные компоненты
            foreach (var comp in components)
                AddComponentInstance(comp);
        }

        // Внутренний метод добавления компонента с регистрацией Start
        private void AddComponentInstance(Behaviour component)
        {
            component.GameObject = this;
            _components.Add(component);
            if (Scene.IsInitialized && ActiveInHierarchy)
                Scene.RegisterPendingStart(component);
        }

        /// <summary>
        /// Создаёт и добавляет компонент заданного типа.
        /// </summary>
        /// <typeparam name="T">Тип компонента (должен быть унаследован от Behaviour).</typeparam>
        /// <returns>Созданный компонент.</returns>
        public T AddComponent<T>() where T : Behaviour, new()
        {
            T component = new T();
            AddComponentInstance(component);
            return component;
        }

        /// <summary>
        /// Возвращает первый компонент указанного типа.
        /// </summary>
        public T GetComponent<T>() where T : Behaviour
            => _components.OfType<T>().FirstOrDefault();
        
        /// <summary>
        /// Рекурсивно ищет компонент указанного типа в этом GameObject и всех его дочерних объектах.
        /// </summary>
        /// <typeparam name="T">Тип компонента (наследник Behaviour).</typeparam>
        /// <returns>Первый найденный компонент или null, если не найден.</returns>
        public T GetComponentInChildren<T>() where T : Behaviour
        {
            // Сначала ищем в самом объекте
            var component = GetComponent<T>();
            if (component != null)
                return component;

            // Рекурсивно обходим всех детей через Transform.Children
            if (Transform != null)
            {
                foreach (var childTransform in Transform.Children)
                {
                    var childGo = childTransform.GameObject;
                    if (childGo != null)
                    {
                        var found = childGo.GetComponentInChildren<T>();
                        if (found != null)
                            return found;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Возвращает все компоненты указанного типа.
        /// </summary>
        public IEnumerable<T> GetComponents<T>() where T : Behaviour
            => _components.OfType<T>();

        /// <summary>
        /// Удаляет компонент из объекта. Вызывается OnDestroy().
        /// </summary>
        public void RemoveComponent(Behaviour component)
        {
            if (_components.Remove(component))
            {
                component.OnDestroy();
                component.GameObject = null;
            }
        }

        /// <summary>
        /// Внутренний метод: вызывает Update у всех компонентов, если объект активен.
        /// </summary>
        internal void UpdateComponents(GameTime gameTime)
        {
            if (!ActiveInHierarchy)
                return;
            foreach (var comp in _components)
                if (comp.Enabled)
                    comp.Update(gameTime);
        }

        /// <summary>
        /// Уничтожает объект: вызывает OnDestroy у всех компонентов, удаляет из сцены.
        /// </summary>
        public void Destroy()
        {
            foreach (var comp in _components)
                comp.OnDestroy();
            _components.Clear();
            Scene.RemoveGameObject(this);
        }
    }
}