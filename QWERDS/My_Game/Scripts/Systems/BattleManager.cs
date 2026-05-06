using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MyGameEngine;

namespace QWERDS
{
    /// <summary>Управляет ходом битвы: приём слов, выполнение букв, проверка условий.</summary>
    public class BattleManager : Behaviour
    {
        public List<Robot> Robots { get; private set; } = new List<Robot>();
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();

        /// <summary>Событие для UI: новое сообщение в лог.</summary>
        public event Action<string> OnLogMessage;
        /// <summary>Событие: слово принято и начинается обработка.</summary>
        public event Action<string> OnWordAccepted;
        /// <summary>Событие: битва завершена (победа/поражение).</summary>
        public event Action<bool> OnBattleEnd;

        private readonly Queue<string> logBuffer = new Queue<string>();

        // Ссылки на UI компоненты (передаются при построении сцены)
        private UIInputField inputField;
        private UIText logDisplay;

        public override void Start()
        {
            // Находим компоненты в иерархии
            inputField = Transform.GameObject?.GetComponentInChildren<UIInputField>();
            // Подписка на ввод слова
            if (inputField != null)
                inputField.OnSubmit += OnWordSubmitted;

            // Инициализация боя (происходит один раз при активации корня)
            InitializeBattle();
        }

        private void InitializeBattle()
        {
            // Роботы берутся из глобального состояния (созданы в начале игры)
            Robots = GameState.Robots.ToList();
            // Генерация врагов (1-4)
            Enemies.Clear();
            int count = new Random().Next(1, 5);
            for (int i = 1; i <= count; i++)
            {
                var enemy = new Enemy($"Враг {i}", 50 + i * 10);
                // Назначаем случайные действия на все буквы, которыми пользуются роботы
                var usedLetters = Robots.SelectMany(r => r.LetterBindings.Keys).Distinct();
                foreach (char c in usedLetters)
                {
                    enemy.Actions[c] = new EnemyAction { Damage = 5 + i * 2, Description = "атакует" };
                }
                Enemies.Add(enemy);
            }
            logBuffer.Clear();
            LogMessage("Бой начался!");
        }

        private void OnWordSubmitted(string word)
        {
            if (string.IsNullOrEmpty(word)) return;

            // 1. Проверка на реальность слова
            if (!WordValidator.IsRealWord(word))
            {
                LogMessage($"Слово \"{word}\" не распознано протоколом.");
                return;
            }

            // 2. Проверка уникальности (будет дополнена позже)
            // if (GameState.UsedWords.Contains(word.ToLower())) ...

            // 3. Проверка спецэффектов
            string specialEffect = WordValidator.GetSpecialEffect(word);
            if (specialEffect != null)
            {
                LogMessage($"Обнаружен спецэффект: {specialEffect}");
                // Здесь будет вызов соответствующей логики
            }

            OnWordAccepted?.Invoke(word);
            ExecuteWord(word);

            // Очищаем поле после успешной отправки
            inputField?.Clear();
        }

        /// <summary>Главный метод выполнения слова.</summary>
        private void ExecuteWord(string word)
        {
            var context = new BattleContext { Battle = this };
            foreach (char letter in word.ToLowerInvariant())
            {
                // Фаза роботов
                foreach (var robot in Robots.Where(r => r.IsAlive))
                {
                    robot.ActOnLetter(letter, context);
                }
                // Фаза врагов
                foreach (var enemy in Enemies.Where(e => e.IsAlive))
                {
                    enemy.ActOnLetter(letter, context);
                }
            }

            // После всех букв – проверка конца
            CheckEndCondition();
        }

        private bool CheckEndCondition()
        {
            if (Enemies.All(e => !e.IsAlive))
            {
                LogMessage("Все враги уничтожены - ПОБЕДА!");
                OnBattleEnd?.Invoke(true);
                Cleanup();
                return true;
            }
            if (Robots.All(r => !r.IsAlive))
            {
                LogMessage("Все роботы пали - поражение...");
                OnBattleEnd?.Invoke(false);
                Cleanup();
                return true;
            }
            return false;
        }

        private void Cleanup()
        {
            // Отписка от событий
            if (inputField != null)
                inputField.OnSubmit -= OnWordSubmitted;
            inputField = null;
        }

        public void LogMessage(string msg)
        {
            logBuffer.Enqueue(msg);
            if (logBuffer.Count > 5) logBuffer.Dequeue();
            OnLogMessage?.Invoke(string.Join("\n", logBuffer));
        }

        public Robot GetRandomAliveRobot()
        {
            var alive = Robots.Where(r => r.IsAlive).ToList();
            return alive.Count > 0 ? alive[new Random().Next(alive.Count)] : null;
        }

        public Enemy GetRandomAliveEnemy()
        {
            var alive = Enemies.Where(e => e.IsAlive).ToList();
            return alive.Count > 0 ? alive[new Random().Next(alive.Count)] : null;
        }
    }
}