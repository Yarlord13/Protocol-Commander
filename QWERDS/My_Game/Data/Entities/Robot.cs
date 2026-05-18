using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace QWERDS
{
    public class Robot
    {
        public string Name { get; set; }
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        public List<ActionBase> SkillSlots { get; } = new List<ActionBase>();
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

        public void ActOnLetter(char letter, BattleContext context)
        {
            if (LetterBindings.TryGetValue(letter, out var action))
            {
                context.User = this;
                action.Execute(context);
                RunStatistics.RecordSkillUsed(action);
            }
        }

        /// <summary>Сбрасывает здоровье, очищает привязки букв, восстанавливает слоты навыков по умолчанию.</summary>
        public void Reset()
        {
            CurrentHealth = MaxHealth;
            LetterBindings.Clear();
            SkillSlots.Clear();
            SkillSlots.Add(new AttackAction());
            SkillSlots.Add(new HealAction());
        }
    }
}