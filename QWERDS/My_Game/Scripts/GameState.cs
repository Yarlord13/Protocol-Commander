using System.Collections.Generic;

namespace QWERDS
{
    public static class GameState
    {
        public static List<Robot> Robots { get; } = new List<Robot>();
        public static HashSet<string> UsedWords { get; } = new HashSet<string>();

        public static void Reset()
        {
            Robots.Clear();
            UsedWords.Clear();
        }

        /// <summary>
        /// Полный сброс забега: очистка слов, восстановление робота до начального состояния.
        /// </summary>
        public static void ResetRun()
        {
            UsedWords.Clear();
            if (Robots.Count > 0)
            {
                var robot = Robots[0];
                robot.Reset();  // добавим этот метод в Robot
            }
            else
            {
                // Создаём робота заново, если его нет
                var hero = new Robot("Герой", 120);
                hero.SkillSlots.Clear();
                hero.SkillSlots.Add(new AttackAction());
                hero.SkillSlots.Add(new HealAction());
                Robots.Add(hero);
            }
        }
    }
}