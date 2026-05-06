using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace QWERDS
{
    /// <summary>Проверка на существование слова в словаре и наличие особых эффектов.</summary>
    public static class WordValidator
    {
        private static readonly HashSet<string> RealWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> SpecialWords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static bool IsLoaded { get; private set; }

        /// <summary>Загрузить словарь и спецслова из файлов (безопасно – отсутствие файлов не роняет игру).</summary>
        public static void Initialize()
        {
            try
            {
                if (File.Exists("../../../Content/RUS.txt"))
                {
                    foreach (var line in File.ReadLines("../../../Content/RUS.txt"))
                    {
                        var w = line.Trim();
                        if (!string.IsNullOrEmpty(w))
                            RealWords.Add(w.ToLowerInvariant());
                    }
                    Debug.WriteLine($"File exists + {RealWords.First()}");
                }
            }
            catch { Debug.WriteLine($"error"); }

            try
            {
                if (File.Exists("Content/special_words.csv"))
                {
                    foreach (var line in File.ReadLines("Content/special_words.csv"))
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 2)
                            SpecialWords[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
            catch { }

            IsLoaded = true;
        }

        /// <summary>Является ли строка реальным словом (если словарь загружен).</summary>
        public static bool IsRealWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            return RealWords.Count == 0 || RealWords.Contains(word.ToLowerInvariant());
        }

        /// <summary>Возвращает идентификатор спецэффекта или null.</summary>
        public static string GetSpecialEffect(string word)
        {
            SpecialWords.TryGetValue(word, out var effect);
            return effect;
        }
    }
}