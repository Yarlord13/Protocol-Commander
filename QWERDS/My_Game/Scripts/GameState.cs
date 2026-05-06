using System.Collections.Generic;

namespace QWERDS
{
    /// <summary>Глобальное состояние забега.</summary>
    public static class GameState
    {
        public static List<Robot> Robots { get; } = new List<Robot>();
        public static HashSet<string> UsedWords { get; } = new HashSet<string>();

        public static void Reset()
        {
            Robots.Clear();
            UsedWords.Clear();
        }
    }
}