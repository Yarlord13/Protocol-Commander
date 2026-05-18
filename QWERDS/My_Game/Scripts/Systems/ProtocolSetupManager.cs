using System;
using System.Collections.Generic;
using System.Linq;
using MyGameEngine;

namespace QWERDS
{
    public class ProtocolSetupManager : Behaviour
    {
        private Robot _currentRobot;
        private List<ActionBase> _unboundSkills = new List<ActionBase>();
        private int _bindingsMadeThisSession = 0;
        public int MaxBindingsPerSession = 3;

        public Robot CurrentRobot => _currentRobot;
        public IReadOnlyList<ActionBase> UnboundSkills => _unboundSkills;
        public event Action OnDataChanged;

        public void Initialize()
        {
            if (GameState.Robots.Count == 0)
                GameState.Robots.Add(new Robot("Герой", 100));
            _currentRobot = GameState.Robots[0];
            RefreshAvailableSkills();
        }

        public override void Start()
        {
            if (_currentRobot == null)
                Initialize();
        }

        public void StartNewSession()
        {
            _bindingsMadeThisSession = 0;
        }

        public void ClearAllBindings()
        {
            if (_currentRobot != null)
            {
                _currentRobot.LetterBindings.Clear();
                OnDataChanged?.Invoke();
            }
        }

        /// <summary>Генерирует 5 случайных навыков (с возможными повторами).</summary>
        public void RefreshAvailableSkills()
        {
            var skillPool = new List<ActionBase>
            {
                new AttackAction(),
                new HealAction(),
            };
            var rng = new Random();
            _unboundSkills.Clear();
            for (int i = 0; i < 5; i++)
            {
                int idx = rng.Next(skillPool.Count);
                var original = skillPool[idx];
                ActionBase copy;
                if (original is AttackAction atk)
                    copy = new AttackAction { Power = atk.Power };
                else if (original is HealAction heal)
                    copy = new HealAction { Amount = heal.Amount };
                else
                    copy = original;
                _unboundSkills.Add(copy);
            }
            OnDataChanged?.Invoke();
        }

        public bool BindAction(char letter, ActionBase action)
        {
            if (_currentRobot == null || action == null) return false;
            if (_bindingsMadeThisSession >= MaxBindingsPerSession) return false;

            // Если навык уже привязан к другой букве – отвязываем
            char? existingLetter = null;
            foreach (var kv in _currentRobot.LetterBindings)
            {
                if (kv.Value == action)
                {
                    existingLetter = kv.Key;
                    break;
                }
            }
            if (existingLetter.HasValue)
                UnbindAction(existingLetter.Value);

            // Если буква уже занята, возвращаем старый навык в пул
            if (_currentRobot.LetterBindings.TryGetValue(letter, out var oldAction) && oldAction != action)
            {
                if (!_unboundSkills.Contains(oldAction))
                    _unboundSkills.Add(oldAction);
            }

            // Убираем навык из пула
            if (!_unboundSkills.Contains(action))
                return false;
            _unboundSkills.Remove(action);

            _currentRobot.LetterBindings[letter] = action;
            _bindingsMadeThisSession++;
            OnDataChanged?.Invoke();
            return true;
        }

        public void UnbindAction(char letter)
        {
            if (_currentRobot?.LetterBindings.TryGetValue(letter, out var action) == true)
            {
                _currentRobot.LetterBindings.Remove(letter);
                if (!_unboundSkills.Contains(action))
                    _unboundSkills.Add(action);
                OnDataChanged?.Invoke();
            }
        }

        /// <summary>Возвращает множитель силы для буквы (всегда на основе частоты буквы, даже без навыка).</summary>
        public float GetSkillPowerForLetter(char letter)
        {
            return DifficultyManager.GetLetterPowerModifier(letter);
        }

        public ActionBase GetBoundSkill(char letter)
        {
            _currentRobot.LetterBindings.TryGetValue(letter, out var action);
            return action;
        }
    }
}