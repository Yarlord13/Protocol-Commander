using System;
using System.Collections.Generic;
using System.Linq;

namespace QWERDS
{
    /// <summary>
    /// Управляет сложностью игры, генерацией врагов и параметрами силы.
    /// </summary>
    public static class DifficultyManager
    {
        private static int _currentBattleIndex = 0; // 0-based, 0 = первый бой
        private static readonly Random _random = new Random();

        /// <summary>Текущий множитель сложности (растёт с каждым боем).</summary>
        public static float CurrentDifficultyMultiplier => 1f + (_currentBattleIndex * 0.1f);

        /// <summary>Увеличить индекс боя (вызывать после победы).</summary>
        public static void AdvanceBattle() => _currentBattleIndex++;

        /// <summary>Сброс для нового забега.</summary>
        public static void Reset() => _currentBattleIndex = 0;

        /// <summary>
        /// Генерирует список врагов для текущего боя.
        /// Количество, здоровье, урон – зависят от сложности.
        /// Буквы и действия врагов пока случайные, но с учётом частоты использования букв игроком.
        /// </summary>
        public static List<Enemy> GenerateEnemies()
        {
            int baseCount = 1 + _random.Next(3); // 1-3 врага
            int enemyCount = Math.Min(baseCount + (_currentBattleIndex / 3), 4); // максимум 4
            var enemies = new List<Enemy>();

            for (int i = 1; i <= enemyCount; i++)
            {
                string name = $"Враг {i}";
                int baseHp = 40 + _random.Next(20) + (int)(10 * CurrentDifficultyMultiplier);
                int hp = (int)(baseHp * CurrentDifficultyMultiplier);
                var enemy = new Enemy(name, hp);

                // Определяем, какие буквы будут у врага: все буквы, которые игрок уже использовал,
                // плюс несколько случайных. Пока упростим: берём все буквы русского алфавита,
                // но для каждой буквы с вероятностью 50% даём действие.
                foreach (char letter in LetterFrequency.AllRussianLetters)
                {
                    // Вероятность действия растёт со сложностью и частотой использования буквы игроком
                    float usageFactor = 0.5f + (RunStatistics.GetLetterUsage(letter) / 20f);
                    float chance = 0.3f + (CurrentDifficultyMultiplier * 0.1f) * usageFactor;
                    if (_random.NextDouble() < chance)
                    {
                        int damage = 5 + _random.Next(10) + (int)(5 * CurrentDifficultyMultiplier);
                        enemy.Actions[letter] = new EnemyAction
                        {
                            Damage = damage,
                            Description = "атакует"
                        };
                    }
                }

                enemies.Add(enemy);
            }
            return enemies;
        }

        /// <summary>
        /// Возвращает модификатор силы навыка в зависимости от частоты буквы.
        /// Чем реже буква, тем выше множитель (от 0.7 до 1.5).
        /// </summary>
        public static float GetLetterPowerModifier(char letter)
        {
            float baseFreq = LetterFrequency.GetBaseFrequency(letter);
            float runFreq = RunStatistics.GetTotalLettersUsed() > 0
                ? RunStatistics.GetLetterUsage(letter) / (float)RunStatistics.GetTotalLettersUsed()
                : 0f;
            // Общая частота имеет вес 70%, частота в забеге – 30%
            float combined = baseFreq * 0.7f + runFreq * 0.3f;
            // Чем выше частота, тем ниже множитель (от 0.7 до 1.5)
            // Частота от 0.01 до 0.12 (макс. для гласных) – преобразуем
            float multiplier = 1.5f - (combined * 8f);
            return Math.Clamp(multiplier, 0.7f, 1.5f);
        }
    }
}