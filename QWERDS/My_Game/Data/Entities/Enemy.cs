using System.Collections.Generic;

namespace QWERDS
{
    /// <summary>Противник в битве.</summary>
    public class Enemy
    {
        public string Name { get; set; }
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>Действия на буквы (генерируются при старте боя).</summary>
        public Dictionary<char, EnemyAction> Actions { get; } = new Dictionary<char, EnemyAction>();

        public Enemy(string name, int maxHealth)
        {
            Name = name;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth = System.Math.Max(CurrentHealth - damage, 0);
        }

        public void Heal(int amount) { /* враги могут лечиться */ }

        /// <summary>Выполнить своё действие на указанную букву.</summary>
        public void ActOnLetter(char letter, BattleContext context)
        {
            if (Actions.TryGetValue(letter, out var act))
            {
                act.Execute(this, context);
            }
        }
    }

    /// <summary>Действие врага – просто пример с уроном.</summary>
    public class EnemyAction
    {
        public int Damage { get; set; } = 5;
        public string Description { get; set; } = "Атакует";

        public void Execute(Enemy source, BattleContext context)
        {
            var robot = context.Battle.GetRandomAliveRobot();
            if (robot != null)
            {
                robot.TakeDamage(Damage);
                context.Battle.LogMessage($"{source.Name} {Description} на {Damage} урона по {robot.Name}.");
            }
        }
    }
}