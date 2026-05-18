using Microsoft.Xna.Framework;
using MyGameEngine;
using System;

namespace QWERDS
{
    public class ProtocolKeyboardHandler : Behaviour
    {
        public static GameObject[] LetterButtons;

        public override void Update(GameTime gameTime)
        {
            if (!Enabled) return;
            if (LetterButtons == null) return;

            char c;
            while (InputManager.TryGetTextInput(out c))
            {
                c = char.ToLowerInvariant(c);
                int index = Array.IndexOf(MySceneBuilder.AllLetters, c);
                if (index >= 0 && index < LetterButtons.Length)
                {
                    MySceneBuilder.OnLetterClicked(index);
                }
            }
        }
    }
}