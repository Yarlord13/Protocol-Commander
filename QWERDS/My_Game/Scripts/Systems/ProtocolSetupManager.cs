using System;
using System.Collections.Generic;
using System.Linq;
using MyGameEngine;

namespace QWERDS
{
    public class ProtocolSetupManager : Behaviour
    {
        private Robot _currentRobot;
        private List<ActionBase> _availableSkills = new List<ActionBase>();
        private int _bindingsMadeThisSession = 0;
        public int MaxBindingsPerSession = 3;

        public Robot CurrentRobot => _currentRobot;
        public event Action OnDataChanged;
        public event Action OnBindingsLimitReached;

        public override void Start()
        {
            if (GameState.Robots.Count == 0)
                GameState.Robots.Add(new Robot("Герой", 100));
            _currentRobot = GameState.Robots[0];
            RefreshAvailableSkills();
        }

        public void StartNewSession()
        {
            _bindingsMadeThisSession = 0;
        }

        public void RefreshAvailableSkills()
        {
            var allPossibleSkills = new List<ActionBase>
            {
                new AttackAction(),
                new HealAction(),
            };
            var rng = new Random();
            _availableSkills = allPossibleSkills.OrderBy(x => rng.Next()).Take(3).ToList();
        }

        public IReadOnlyList<ActionBase> GetAvailableSkills() => _availableSkills;

        public bool BindAction(char letter, ActionBase action)
        {
            if (_bindingsMadeThisSession >= MaxBindingsPerSession)
            {
                OnBindingsLimitReached?.Invoke();
                return false;
            }
            if (_currentRobot == null || action == null) return false;
            _currentRobot.LetterBindings[letter] = action;
            _bindingsMadeThisSession++;
            OnDataChanged?.Invoke();
            return true;
        }

        public void UnbindAction(char letter)
        {
            if (_currentRobot?.LetterBindings.Remove(letter) == true)
            {
                OnDataChanged?.Invoke();
            }
        }

        public float GetSkillPowerForLetter(char letter)
        {
            if (!_currentRobot.LetterBindings.TryGetValue(letter, out var action))
                return 1f;
            return DifficultyManager.GetLetterPowerModifier(letter);
        }
    }
}