using MyGameEngine;

namespace QWERDS
{
    /// <summary>Контекст выполнения действия: текущий бой, исполнитель и цели.</summary>
    public class BattleContext
    {
        public BattleManager Battle { get; set; }
        public Robot User { get; set; }
        // Можно расширить списком врагов, союзников и т.д.
    }

    /// <summary>Базовый класс действия робота. Легко расширяется созданием новых наследников.</summary>
    public abstract class ActionBase
    {
        /// <summary>Отображаемое имя действия (для UI).</summary>
        public string Name { get; set; } = "Действие";

        /// <summary>Описание, показываемое при выборе.</summary>
        public string Description { get; set; } = "";

        /// <summary>Выполнить команду в рамках боя.</summary>
        /// <param name="context">Контекст выполнения.</param>
        public abstract void Execute(BattleContext context);
    }
}