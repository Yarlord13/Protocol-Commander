using Microsoft.Xna.Framework;
using MyGameEngine;
using System;

namespace QWERDS
{
    public class ProtocolKeyboardHandler : Behaviour
    {
        public static GameObject[] LetterButtons;
        private int _pendingLetterIndex = -1;

        public override void Update(GameTime gameTime)
        {
            if (!Enabled) return;
            char c;
            while (InputManager.TryGetTextInput(out c)) // обрабатываем все символы, накопившиеся за кадр
            {
                c = char.ToLowerInvariant(c);
                int index = Array.IndexOf(MySceneBuilder.AllLetters, c);
                if (index >= 0 && LetterButtons != null && index < LetterButtons.Length)
                {
                    System.Diagnostics.Debug.WriteLine($"Keyboard char: {c}, index {index}");
                    MySceneBuilder.OnLetterClicked(index);
                }
            }
        }
    }
}