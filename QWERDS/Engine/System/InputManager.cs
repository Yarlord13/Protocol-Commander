using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MyGameEngine;

/// <summary>
/// Статический менеджер ввода. Вызывайте Update() в начале каждого кадра.
/// Предоставляет текущие и переходные состояния клавиатуры и мыши,
/// а также символы, введённые через событие TextInput (с учётом раскладки).
/// </summary>
public static class InputManager
{
    private static KeyboardState _currentKeyboard;
    private static KeyboardState _previousKeyboard;
    private static MouseState _currentMouse;
    private static MouseState _previousMouse;

    // Очередь символов, поступающих из Window.TextInput (может пополняться в любое время)
    private static readonly Queue<char> _rawTextQueue = new Queue<char>();
    // Символы, доступные для обработки в текущем кадре
    private static readonly List<char> _textInputChars = new List<char>();

    /// <summary>Был ли менеджер обновлён хотя бы один раз.</summary>
    public static bool Initialized { get; private set; }

    /// <summary>
    /// Добавляет символ, полученный от системного события ввода.
    /// Вызывается из подписки на Window.TextInput.
    /// </summary>
    public static void FeedTextInput(char c) => _rawTextQueue.Enqueue(c);

    /// <summary>
    /// Пытается получить следующий символ, введённый в этом кадре.
    /// Возвращает true и сам символ, если он был.
    /// </summary>
    public static bool TryGetTextInput(out char c)
    {
        if (_textInputChars.Count > 0)
        {
            c = _textInputChars[0];
            _textInputChars.RemoveAt(0);
            return true;
        }
        c = default;
        return false;
    }

    /// <summary>
    /// Обновляет состояния ввода. Вызывайте строго один раз за кадр,
    /// перед любой логикой, зависящей от ввода.
    /// </summary>
    public static void Update()
    {
        _previousKeyboard = _currentKeyboard;
        _currentKeyboard = Keyboard.GetState();

        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();

        // Переносим все символы, накопившиеся с прошлого кадра, в список для чтения
        _textInputChars.Clear();
        while (_rawTextQueue.Count > 0)
        {
            _textInputChars.Add(_rawTextQueue.Dequeue());
        }

        Initialized = true;
    }

    // ================== Клавиатура ==================

    /// <summary>Зажат ли указанный клавиша в текущем кадре.</summary>
    public static bool IsKeyDown(Keys key) => _currentKeyboard.IsKeyDown(key);

    /// <summary>Отпущена ли указанная клавиша в текущем кадре.</summary>
    public static bool IsKeyUp(Keys key) => _currentKeyboard.IsKeyUp(key);

    /// <summary>Была ли клавиша нажата в этом кадре (переход из Up в Down).</summary>
    public static bool IsKeyPressed(Keys key) =>
        _currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    /// <summary>Была ли клавиша отпущена в этом кадре (переход из Down в Up).</summary>
    public static bool IsKeyReleased(Keys key) =>
        _currentKeyboard.IsKeyUp(key) && _previousKeyboard.IsKeyDown(key);

    // ================== Мышь ==================

    /// <summary>Позиция мыши в экранных координатах (без учёта масштабирования игры).</summary>
    public static Vector2 MouseScreenPosition => new Vector2(_currentMouse.X, _currentMouse.Y);

    /// <summary>Позиция мыши в эталонных координатах игры (с учётом ResolutionManager).</summary>
    public static Vector2 MouseReferencePosition =>
        ResolutionManager.ToReference(MouseScreenPosition);

    /// <summary>Зажата ли левая кнопка мыши в текущем кадре.</summary>
    public static bool IsLeftButtonDown => _currentMouse.LeftButton == ButtonState.Pressed;

    /// <summary>Была ли левая кнопка мыши нажата в этом кадре.</summary>
    public static bool IsLeftButtonPressed =>
        _currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;

    /// <summary>Была ли левая кнопка мыши отпущена в этом кадре.</summary>
    public static bool IsLeftButtonReleased =>
        _currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed;
}