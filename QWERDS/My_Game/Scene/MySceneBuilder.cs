using Microsoft.Xna.Framework;
using MyGameEngine;

/// <summary>
/// Класс-строитель сцены. Здесь описываются все игровые объекты.
/// Изменения в игре вносятся только здесь и в собственных компонентах поведения.
/// Класс Game1 при этом остаётся нетронутым.
/// </summary>
public static class MySceneBuilder
{
    public static void Build()
    {
        System.Diagnostics.Debug.WriteLine("Build started");

        Scene.Initialize();

        // === Пример 1: Игрок в центре экрана с фиксированным размером ===
        var playerTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f), // привязка к центру экрана
            Position = Vector2.Zero,          // без смещения
            Size = new Vector2(200, 200),     // фиксированный размер 200x200 эталонных пикселей
            Origin = new Vector2(0.5f, 0.5f)  // центр объекта в точке привязки
        };
        var player = Scene.CreateGameObject("Player", playerTransform);

        // === Пример 2: Кнопка в правом нижнем углу с отступом 20 пикселей от краёв ===
        var buttonTransform = new Transform
        {
            Anchor = new Vector2(1f, 1f),     // правый нижний угол экрана
            Position = new Vector2(-20, -20), // смещение влево и вверх (отрицательные, т.к. Anchor в углу)
            Size = new Vector2(300, 100),     // фиксированный размер
            Origin = new Vector2(1f, 1f)      // правый нижний угол объекта привязывается к точке
        };
        var button = Scene.CreateGameObject("Button", buttonTransform,
            new SpriteRenderer("Sprites/RobotGG"));

        // === Пример 3: Панель, растянутая по ширине с отступами 50 слева/справа, фиксированная высота ===
        var panelTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),   // привязка к центру верхней границы экрана
            Position = new Vector2(0, 30),    // отступ сверху 30 пикселей
            SizeModeX = SizeMode.Stretch,     // растяжение по X
            StretchLeft = 50,
            StretchRight = 50,
            SizeModeY = SizeMode.Fixed,
            Size = new Vector2(0, 100),       // высота 100, ширина игнорируется из-за Stretch
            Origin = new Vector2(0.5f, 0f)    // привязка по центру верхнего края
        };
        var panel = Scene.CreateGameObject("TopPanel", panelTransform,
            new SpriteRenderer("Sprites/RobotGG"));

        // === Пример 4: Дочерний объект внутри панели, растянутый по высоте панели с отступами ===
        var childTransform = new Transform
        {
            Anchor = new Vector2(0f, 0f),     // левый верхний угол панели
            Position = new Vector2(20, 20),   // отступ от левого верхнего угла панели
            SizeModeX = SizeMode.Fixed,
            Size = new Vector2(150, 0),       // фиксированная ширина
            SizeModeY = SizeMode.Stretch,
            StretchTop = 20,
            StretchBottom = 20,
            Origin = Vector2.Zero
        };
        // Устанавливаем родителя
        childTransform.SetParent(panel.Transform);
        var child = Scene.CreateGameObject("PanelChild", childTransform,
            new SpriteRenderer("Sprites/RobotGG"));
    }
}