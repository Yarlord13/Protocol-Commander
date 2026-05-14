using System.Collections.Generic;
using Microsoft.Xna.Framework; // для MathHelper

namespace QWERDS
{
    /// <summary>Игровой юнит-робот.</summary>
    public class Robot
    {
        public string Name { get; set; }
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>Навыки, доступные для назначения на буквы.</summary>
        public List<ActionBase> SkillSlots { get; } = new List<ActionBase>();

        /// <summary>Привязка: буква -> действие.</summary>
        public Dictionary<char, ActionBase> LetterBindings { get; } = new Dictionary<char, ActionBase>();

        public Robot(string name, int maxHealth = 100)
        {
            Name = name;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void Heal(int amount)
        {
            CurrentHealth = MathHelper.Min(CurrentHealth + amount, MaxHealth);
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth = MathHelper.Max(CurrentHealth - damage, 0);
        }

        /// <summary>Выполняет действие на заданную букву (если привязано).</summary>
        public void ActOnLetter(char letter, BattleContext context)
        {
            if (LetterBindings.TryGetValue(letter, out var action))
            {
                context.User = this;
                action.Execute(context);
                RunStatistics.RecordSkillUsed(action);
            }
        }
    }
}