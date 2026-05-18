using Microsoft.Xna.Framework;
using MyGameEngine;

namespace QWERDS
{
    /// <summary>Компонент, добавляющий задержку перед переходом к настройке протокола после победы.</summary>
    public class VictoryDelayHandler : Behaviour
    {
        private float _delay = 2.0f;
        private float _timer;
        private bool _victoryTriggered;

        public void StartVictoryDelay()
        {
            _victoryTriggered = true;
            _timer = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            if (!_victoryTriggered) return;

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _delay)
            {
                _victoryTriggered = false;
                MySceneBuilder.SwitchToProtocolSetup();
            }
        }
    }
}