using Microsoft.Xna.Framework;
using MyGameEngine;
using QWERDS;

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

        // Строим главное меню
        MainMenu();
    }

    /// <summary>
    /// Создаёт адаптивное главное меню с заголовком и тремя кнопками по центру.
    /// </summary>
    public static void MainMenu()
    {
        var baseColor = new Color(106, 138, 57);

        // === Заголовок игры ===
        var titleTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),      // центр верхней границы экрана
            Position = new Vector2(0, 80),        // отступ сверху 80 эталонных пикселей
            Size = new Vector2(800, 100),         // фиксированная область для текста
            Origin = new Vector2(0.5f, 0f),       // точка привязки: центр верхней границы
            SizeModeX = SizeMode.Fixed,
            SizeModeY = SizeMode.Fixed
        };

        Scene.CreateGameObject("Title", titleTransform,
            new UIText("Fonts/PixelFont", "Protocol Commander", baseColor, 3.0f, TextAlignment.Center)
        );

        // === Кнопки меню ===
        // Массив с текстами кнопок и их вертикальными смещениями относительно центра экрана
        string[] buttonTexts = { "Играть", "О проекте", "Выход" };
        Vector2[] buttonOffsets = {
            new Vector2(0, -80),   // первая кнопка выше центра
            new Vector2(0, 0),     // вторая кнопка точно в центре
            new Vector2(0, 80)     // третья кнопка ниже центра
        };

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            var btnTransform = new Transform
            {
                Anchor = new Vector2(0.5f, 0.5f),   // центр экрана
                Position = buttonOffsets[i],
                Size = new Vector2(300, 60),         // фиксированный размер кнопки
                Origin = new Vector2(0.5f, 0.5f),    // точка привязки – центр кнопки
                SizeModeX = SizeMode.Fixed,
                SizeModeY = SizeMode.Fixed
            };

            // Создаём объект кнопки с фоном, текстом и логикой взаимодействия
            GameObject btnObj = Scene.CreateGameObject($"Button_{buttonTexts[i]}", btnTransform,
                new SpriteRenderer("Sprites/Panel") { Color = new Color(80, 80, 80) },
                new UIText("Fonts/PixelFont", buttonTexts[i], baseColor, 1.5f, TextAlignment.Center),
                new UIButton()
            );

            // Для кнопки "Выход" назначаем действие закрытия игры
            if (buttonTexts[i] == "Выход")
            {
                var exitBtn = btnObj.GetComponent<UIButton>();
                if (exitBtn != null)
                    exitBtn.OnClick += () => Game1.InstanceGame.Exit();
            }

            // Визуальная обратная связь при наведении мыши
            var btnSprite = btnObj.GetComponent<SpriteRenderer>();
            var btnUI = btnObj.GetComponent<UIButton>();
            if (btnUI != null && btnSprite != null)
            {
                btnUI.OnFocusEnter += () => btnSprite.Color = new Color(120, 120, 120);
                btnUI.OnFocusExit += () => btnSprite.Color = new Color(80, 80, 80);
            }
        }
    }
}