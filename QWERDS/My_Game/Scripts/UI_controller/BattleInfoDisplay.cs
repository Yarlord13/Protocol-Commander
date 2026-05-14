using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using MyGameEngine;

namespace QWERDS
{
    /// <summary>
    /// Компонент, который подписывается на события BattleManager и собирает полную информацию о ходе боя.
    /// Вывод информации (UI) пока не реализован, но класс предоставляет методы для получения данных.
    /// </summary>
    public class BattleInfoDisplay : Behaviour
    {
        private BattleManager _battleManager;
        private readonly List<string> _battleLog = new List<string>();
        private int _currentTurnNumber;

        /// <summary>Последние 10 сообщений боя.</summary>
        public IReadOnlyList<string> RecentLogs => _battleLog;

        /// <summary>Краткое состояние роботов (здоровье и статусы).</summary>
        public string RobotsStatus { get; private set; } = "";

        /// <summary>Краткое состояние врагов (здоровье, если известно).</summary>
        public string EnemiesStatus { get; private set; } = "";

        /// <summary>Номер текущего хода (слова).</summary>
        public int CurrentTurn => _currentTurnNumber;

        public override void Start()
        {
            _battleManager = GameObject.GetComponent<BattleManager>();
            if (_battleManager == null)
            {
                _battleManager = Transform.GameObject?.GetComponent<BattleManager>();
            }

            if (_battleManager != null)
            {
                _battleManager.OnLogMessage += OnLogMessage;
                _battleManager.OnWordAccepted += OnWordAccepted;
                _battleManager.OnBattleEnd += OnBattleEnd;
            }
        }

        private void OnLogMessage(string msg)
        {
            _battleLog.Insert(0, msg);
            if (_battleLog.Count > 10) _battleLog.RemoveAt(10);
            UpdateStatus(); // обновляем состояния
        }

        private void OnWordAccepted(string word)
        {
            _currentTurnNumber++;
            UpdateStatus();
        }

        private void OnBattleEnd(bool victory)
        {
            // Можно сохранить финальную статистику
        }

        private void UpdateStatus()
        {
            if (_battleManager == null) return;

            var robots = _battleManager.Robots;
            var enemies = _battleManager.Enemies;

            StringBuilder robotSb = new StringBuilder();
            foreach (var robot in robots)
            {
                robotSb.Append($"{robot.Name}: {robot.CurrentHealth}/{robot.MaxHealth} HP");
                if (!robot.IsAlive) robotSb.Append(" (погиб)");
                robotSb.AppendLine();
            }
            RobotsStatus = robotSb.ToString();

            StringBuilder enemySb = new StringBuilder();
            foreach (var enemy in enemies)
            {
                enemySb.Append($"{enemy.Name}: ");
                // Если враг не сканирован – показываем "???" (здесь заглушка)
                enemySb.Append(enemy.IsAlive ? $"{enemy.CurrentHealth} HP" : "уничтожен");
                enemySb.AppendLine();
            }
            EnemiesStatus = enemySb.ToString();
        }

        /// <summary>Возвращает полную информацию о бое (текст для отладки).</summary>
        public string GetFullBattleReport()
        {
            return $"Ход {_currentTurnNumber}\n=== Роботы ===\n{RobotsStatus}\n=== Враги ===\n{EnemiesStatus}\n=== Лог ===\n{string.Join("\n", RecentLogs)}";
        }

        public override void OnDestroy()
        {
            if (_battleManager != null)
            {
                _battleManager.OnLogMessage -= OnLogMessage;
                _battleManager.OnWordAccepted -= OnWordAccepted;
                _battleManager.OnBattleEnd -= OnBattleEnd;
            }
        }
    }
}