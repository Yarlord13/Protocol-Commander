using Microsoft.Xna.Framework;

namespace MyGameEngine
{
    /// <summary>
    /// Базовый класс для всех компонентов, которые можно прикрепить к GameObject.
    /// Аналог MonoBehaviour в Unity.
    /// </summary>
    public abstract class Behaviour
    {
        /// <summary>Ссылка на GameObject, которому принадлежит этот компонент.</summary>
        public GameObject GameObject { get; internal set; }

        /// <summary>Удобное свойство для доступа к Transform (сокращение для GameObject.Transform).</summary>
        public Transform Transform => GameObject?.Transform;

        /// <summary>Включён ли компонент. Если false, метод Update() вызываться не будет.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Внутренний флаг: был ли уже вызван Start().</summary>
        internal bool Started { get; set; }

        /// <summary>
        /// Вызывается один раз перед первым Update(), когда объект становится активным.
        /// Используйте для инициализации, получения ссылок на другие компоненты.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Вызывается каждый кадр, если компонент включён (Enabled == true) и GameObject активен.
        /// </summary>
        /// <param name="gameTime">Предоставляет информацию о времени, прошедшем с последнего кадра.</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Вызывается при уничтожении компонента или его GameObject.
        /// Используйте для освобождения ресурсов, отписки от событий.
        /// </summary>
        public virtual void OnDestroy() { }
    }
}