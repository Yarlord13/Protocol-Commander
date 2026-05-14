using System;
using System.Collections.Generic;

namespace QWERDS
{
    /// <summary>
    /// Предопределённые частоты букв русского алфавита (в порядке убывания).
    /// Значения основаны на реальной статистике (приблизительные проценты).
    /// </summary>
    public static class LetterFrequency
    {
        public static readonly IReadOnlyList<char> AllRussianLetters = Array.AsReadOnly(new char[]
        {
            'а', 'б', 'в', 'г', 'д', 'е', 'ё', 'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п',
            'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ъ', 'ы', 'ь', 'э', 'ю', 'я'
        });

        private static readonly Dictionary<char, float> _baseFrequencies = new Dictionary<char, float>
        {
            {'о', 0.109f}, {'е', 0.084f}, {'а', 0.075f}, {'и', 0.073f}, {'н', 0.067f},
            {'т', 0.063f}, {'с', 0.054f}, {'р', 0.047f}, {'в', 0.045f}, {'л', 0.044f},
            {'к', 0.034f}, {'м', 0.032f}, {'д', 0.030f}, {'п', 0.028f}, {'у', 0.026f},
            {'я', 0.023f}, {'ы', 0.021f}, {'ь', 0.019f}, {'г', 0.018f}, {'з', 0.017f},
            {'б', 0.016f}, {'ч', 0.014f}, {'й', 0.012f}, {'х', 0.010f}, {'ж', 0.009f},
            {'ю', 0.007f}, {'ш', 0.006f}, {'ц', 0.005f}, {'щ', 0.003f}, {'э', 0.003f},
            {'ф', 0.002f}, {'ъ', 0.0004f}, {'ё', 0.0001f}
        };

        /// <summary>Возвращает базовую частоту буквы (от 0 до 1).</summary>
        public static float GetBaseFrequency(char letter)
        {
            letter = char.ToLowerInvariant(letter);
            return _baseFrequencies.TryGetValue(letter, out float freq) ? freq : 0.01f;
        }
    }
}