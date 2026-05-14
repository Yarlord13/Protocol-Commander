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

        public event Action<string> OnLogMessage;
        public event Action<string> OnWordAccepted;
        public event Action<bool> OnBattleEnd;

        private readonly Queue<string> logBuffer = new Queue<string>();
        private UIInputField inputField;
        private DateTime _wordStartTime; // для замера времени хода

        public override void Start()
        {
            inputField = Transform.GameObject?.GetComponentInChildren<UIInputField>();
            if (inputField != null)
                inputField.OnSubmit += OnWordSubmitted;

            InitializeBattle();
        }

        private void InitializeBattle()
        {
            Robots = GameState.Robots.ToList();
            Enemies = DifficultyManager.GenerateEnemies();
            logBuffer.Clear();
            LogMessage("Бой начался!");
        }

        private void OnWordSubmitted(string word)
        {
            if (string.IsNullOrEmpty(word)) return;

            // Замер времени
            float turnTime = (float)(DateTime.Now - _wordStartTime).TotalSeconds;
            RunStatistics.RecordTurnTime(turnTime);
            RunStatistics.RecordWordUsed(word);

            if (!WordValidator.IsRealWord(word))
            {
                LogMessage($"Слово \"{word}\" не распознано протоколом.");
                return;
            }

            string specialEffect = WordValidator.GetSpecialEffect(word);
            if (specialEffect != null)
            {
                LogMessage($"Обнаружен спецэффект: {specialEffect}");
                // Здесь будет вызов соответствующей логики
            }

            OnWordAccepted?.Invoke(word);
            ExecuteWord(word);

            // Очищаем поле после отправки
            inputField?.Clear();
            // Запускаем таймер для следующего слова (здесь не реализован, но можно добавить)
            _wordStartTime = DateTime.Now;
        }

        private void ExecuteWord(string word)
        {
            var context = new BattleContext { Battle = this };
            foreach (char letter in word.ToLowerInvariant())
            {
                // Регистрируем использование буквы в статистике
                RunStatistics.RecordLetterUsed(letter);

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

            CheckEndCondition();
        }

        private bool CheckEndCondition()
        {
            if (Enemies.All(e => !e.IsAlive))
            {
                LogMessage("Все враги уничтожены - ПОБЕДА!");
                foreach (var enemy in Enemies.Where(e => !e.IsAlive))
                    RunStatistics.RecordEnemyDefeated(enemy.Name);
                RunStatistics.RecordBattleFinished();
                DifficultyManager.AdvanceBattle();
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