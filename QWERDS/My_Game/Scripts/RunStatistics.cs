using System;
using System.Collections.Generic;
using System.Linq;

namespace QWERDS
{
    /// <summary>
    /// Статический класс для сбора и хранения статистики текущего забега.
    /// </summary>
    public static class RunStatistics
    {
        private static readonly Dictionary<char, int> _letterUsage = new Dictionary<char, int>();
        private static readonly Dictionary<string, int> _skillUsage = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> _enemyDefeats = new Dictionary<string, int>();
        private static readonly List<float> _turnTimes = new List<float>();
        private static readonly HashSet<string> _usedWords = new HashSet<string>();

        public static int TotalTurns { get; private set; }
        public static int TotalBattles { get; private set; }
        public static int TotalWordsUsed => _usedWords.Count;

        /// <summary>Сбрасывает всю статистику для нового забега.</summary>
        public static void Reset()
        {
            _letterUsage.Clear();
            _skillUsage.Clear();
            _enemyDefeats.Clear();
            _turnTimes.Clear();
            _usedWords.Clear();
            TotalTurns = 0;
            TotalBattles = 0;
        }

        /// <summary>Регистрирует использование буквы в слове.</summary>
        public static void RecordLetterUsed(char letter)
        {
            letter = char.ToLowerInvariant(letter);
            if (!_letterUsage.ContainsKey(letter))
                _letterUsage[letter] = 0;
            _letterUsage[letter]++;
        }

        /// <summary>Регистрирует выполнение действия робота.</summary>
        public static void RecordSkillUsed(ActionBase skill)
        {
            string skillName = skill?.Name ?? "Unknown";
            if (!_skillUsage.ContainsKey(skillName))
                _skillUsage[skillName] = 0;
            _skillUsage[skillName]++;
        }

        /// <summary>Регистрирует время, затраченное на одно слово (ход).</summary>
        public static void RecordTurnTime(float seconds)
        {
            _turnTimes.Add(seconds);
            TotalTurns++;
        }

        /// <summary>Регистрирует использованное слово (для уникальности).</summary>
        public static void RecordWordUsed(string word)
        {
            if (!string.IsNullOrEmpty(word))
                _usedWords.Add(word.ToLowerInvariant());
        }

        /// <summary>Регистрирует победу над врагом.</summary>
        public static void RecordEnemyDefeated(string enemyName)
        {
            if (!_enemyDefeats.ContainsKey(enemyName))
                _enemyDefeats[enemyName] = 0;
            _enemyDefeats[enemyName]++;
        }

        /// <summary>Регистрирует завершение битвы.</summary>
        public static void RecordBattleFinished() => TotalBattles++;

        /// <summary>Возвращает количество использований буквы.</summary>
        public static int GetLetterUsage(char letter)
        {
            _letterUsage.TryGetValue(char.ToLowerInvariant(letter), out int count);
            return count;
        }

        /// <summary>Возвращает общее количество использованных букв.</summary>
        public static int GetTotalLettersUsed() => _letterUsage.Values.Sum();

        /// <summary>Возвращает среднее время на ход (в секундах).</summary>
        public static float GetAverageTurnTime() => _turnTimes.Count > 0 ? _turnTimes.Average() : 0f;

        /// <summary>Возвращает словарь самых используемых навыков (название -> количество).</summary>
        public static IReadOnlyDictionary<string, int> GetSkillUsage() => _skillUsage;

        /// <summary>Возвращает словарь побеждённых врагов (имя -> количество).</summary>
        public static IReadOnlyDictionary<string, int> GetEnemyDefeats() => _enemyDefeats;
    }
}