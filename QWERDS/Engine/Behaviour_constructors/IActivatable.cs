namespace MyGameEngine
{
    /// <summary>
    /// Интерфейс, который могут реализовать компоненты, чтобы получать уведомления
    /// об изменении активности GameObject в иерархии.
    /// </summary>
    public interface IActivatable
    {
        /// <summary>Вызывается, когда GameObject становится активным (ActiveSelf = true).</summary>
        void OnEnable();

        /// <summary>Вызывается, когда GameObject становится неактивным (ActiveSelf = false).</summary>
        void OnDisable();
    }
}