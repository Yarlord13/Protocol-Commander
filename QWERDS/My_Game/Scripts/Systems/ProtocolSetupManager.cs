using System.Collections.Generic;
using MyGameEngine;

namespace QWERDS
{
    /// <summary>Логика экрана настройки протокола.</summary>
    public class ProtocolSetupManager : Behaviour
    {
        private int currentRobotIndex;
        public Robot CurrentRobot => GameState.Robots.Count > 0 ? GameState.Robots[currentRobotIndex] : null;

        public override void Start()
        {
            if (GameState.Robots.Count == 0) return;
            currentRobotIndex = 0;
        }

        /// <summary>Привязать действие к букве для текущего робота.</summary>
        public void BindAction(char letter, ActionBase action)
        {
            if (CurrentRobot == null) return;
            CurrentRobot.LetterBindings[letter] = action;
        }

        /// <summary>Убрать привязку с буквы.</summary>
        public void UnbindAction(char letter)
        {
            CurrentRobot?.LetterBindings.Remove(letter);
        }

        /// <summary>Переключиться на следующего робота.</summary>
        public void NextRobot()
        {
            if (GameState.Robots.Count == 0) return;
            currentRobotIndex = (currentRobotIndex + 1) % GameState.Robots.Count;
        }

        /// <summary>Переключиться на предыдущего робота.</summary>
        public void PreviousRobot()
        {
            if (GameState.Robots.Count == 0) return;
            currentRobotIndex = (currentRobotIndex - 1 + GameState.Robots.Count) % GameState.Robots.Count;
        }
    }
}