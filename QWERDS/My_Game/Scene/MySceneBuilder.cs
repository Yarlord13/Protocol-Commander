using Microsoft.Xna.Framework;
using MyGameEngine;
using QWERDS;

public enum GameMode
{
    None,
    MainMenu,
    Battle,
    ProtocolSetup
}

public static class MySceneBuilder
{
    private static Transform CreateRootTransform() => new Transform() { SizeModeX = SizeMode.Stretch, SizeModeY = SizeMode.Stretch, StretchBottom = 0, StretchLeft = 0, StretchRight = 0, StretchTop = 0 };
    // Корневые объекты для каждого режима (создаются один раз)
    private static GameObject _mainMenuRoot;
    private static GameObject _battleRoot;
    private static GameObject _protocolSetupRoot;

    // Текущий активный режим
    public static GameMode CurrentMode { get; private set; } = GameMode.None;

    public static void Build()
    {
        Scene.Initialize();

        // Создаём корневые объекты (неактивные)
        CreateMainMenu();
        CreateBattleMode();
        CreateProtocolSetupMode();

        // Запускаем главное меню
        SwitchToMainMenu();
    }

    // ==================== Главное меню ====================
    private static void CreateMainMenu()
    {
        if (_mainMenuRoot != null) return;

        _mainMenuRoot = Scene.CreateGameObject("MainMenuRoot", CreateRootTransform());
        _mainMenuRoot.ActiveSelf = false;

        var baseColor = new Color(106, 138, 57);

        // Заголовок
        var titleTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),
            Position = new Vector2(0, 80),
            Size = new Vector2(800, 100),
            Origin = new Vector2(0.5f, 0f)
        };
        var title = Scene.CreateGameObject("Title", titleTransform,
            new UIText("Fonts/PixelFont", "Protocol Commander", baseColor, 3.0f, TextAlignment.Center)
        );
        title.Transform.SetParent(_mainMenuRoot.Transform);

        // Кнопки
        string[] buttonTexts = { "Играть", "О проекте", "Выход" };
        Vector2[] offsets = { new Vector2(0, -80), new Vector2(0, 0), new Vector2(0, 80) };

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            var btnTransform = new Transform
            {
                Anchor = new Vector2(0.5f, 0.5f),
                Position = offsets[i],
                Size = new Vector2(300, 60),
                Origin = new Vector2(0.5f, 0.5f)
            };
            var btnObj = Scene.CreateGameObject($"Button_{buttonTexts[i]}", btnTransform,
                new SpriteRenderer("Sprites/Panel") { Color = new Color(80, 80, 80) },
                new UIText("Fonts/PixelFont", buttonTexts[i], baseColor, 1.5f, TextAlignment.Center),
                new UIButton()
            );
            btnObj.Transform.SetParent(_mainMenuRoot.Transform);

            // Настройка действий для кнопки "Играть"
            if (buttonTexts[i] == "Играть")
            {
                var btn = btnObj.GetComponent<UIButton>();
                btn.OnClick += () => SwitchToBattle(); // Пока сразу в бой (позже можно настройку)
            }
            else if (buttonTexts[i] == "Выход")
            {
                var btn = btnObj.GetComponent<UIButton>();
                btn.OnClick += () => Game1.InstanceGame?.Exit();
            }

            // Визуальный отклик
            var spr = btnObj.GetComponent<SpriteRenderer>();
            var uiBtn = btnObj.GetComponent<UIButton>();
            if (spr != null && uiBtn != null)
            {
                uiBtn.OnFocusEnter += () => spr.Color = new Color(120, 120, 120);
                uiBtn.OnFocusExit += () => spr.Color = new Color(80, 80, 80);
            }
        }
    }

    // ==================== Режим битвы ====================
    private static void CreateBattleMode()
    {
        if (_battleRoot != null) return;

        _battleRoot = Scene.CreateGameObject("BattleRoot", CreateRootTransform());
        _battleRoot.ActiveSelf = false;

        // Поле ввода слова
        var inputTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Position = new Vector2(0, 250),
            Size = new Vector2(600, 60),
            Origin = new Vector2(0.5f, 0.5f)
        };
        var inputObj = Scene.CreateGameObject("WordInput", inputTransform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(40, 40, 40) },
            new UIText("Fonts/PixelFont", "", Color.White, 1.8f, TextAlignment.Left, VerticalAlignment.Center) { LayerDepth = 0.1f },
            new UIInputField { Placeholder = "Введите слово...", MaxLength = 20 }
        );
        inputObj.Transform.SetParent(_battleRoot.Transform);

        // Подписка на события
        var inputField = inputObj.GetComponent<UIInputField>();
        if (inputField != null)
        {
            inputField.OnTextChanged += (text) =>
                System.Diagnostics.Debug.WriteLine($"Текущий текст: '{text}'");
            inputField.OnSubmit += (word) =>
            {
                System.Diagnostics.Debug.WriteLine($"Отправлено слово: '{word}'");
                // В будущем: проверка уникальности, посимвольное выполнение
            };
        }

        // Заглушка: можно добавить надпись "Бой"
        var battleLabelTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),
            Position = new Vector2(0, 30),
            Size = new Vector2(400, 40),
            Origin = new Vector2(0.5f, 0f)
        };
        var battleLabel = Scene.CreateGameObject("BattleLabel", battleLabelTransform,
            new UIText("Fonts/PixelFont", "Режим боя", Color.Red, 2.0f, TextAlignment.Center)
        );
        battleLabel.Transform.SetParent(_battleRoot.Transform);

        // Кнопка назад в меню
        var backTransform = new Transform
        {
            Anchor = new Vector2(0, 1),
            Position = new Vector2(20, -20),
            Size = new Vector2(150, 50),
            Origin = new Vector2(0, 1)
        };
        var backObj = Scene.CreateGameObject("BackToMenu_Battle", backTransform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(80, 80, 80) },
            new UIText("Fonts/PixelFont", "В меню", Color.White, 1.2f, TextAlignment.Center),
            new UIButton()
        );
        backObj.Transform.SetParent(_battleRoot.Transform);

        var backBtn = backObj.GetComponent<UIButton>();
        var backSpr = backObj.GetComponent<SpriteRenderer>();
        if (backBtn != null && backSpr != null)
        {
            backBtn.OnClick += SwitchToMainMenu;
            backBtn.OnFocusEnter += () => backSpr.Color = new Color(120, 120, 120);
            backBtn.OnFocusExit += () => backSpr.Color = new Color(80, 80, 80);
        }
    }

    // ==================== Настройка протокола ====================
    private static void CreateProtocolSetupMode()
    {
        if (_protocolSetupRoot != null) return;

        _protocolSetupRoot = Scene.CreateGameObject("ProtocolSetupRoot", CreateRootTransform());
        _protocolSetupRoot.ActiveSelf = false;

        // Заглушка
        var stubTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Size = new Vector2(400, 100),
            Origin = new Vector2(0.5f, 0.5f)
        };
        var stubObj = Scene.CreateGameObject("SetupStub", stubTransform,
            new UIText("Fonts/PixelFont", "Настройка протокола\n(пока здесь ничего нет)", Color.Gray, 1.5f, TextAlignment.Center)
        );
        stubObj.Transform.SetParent(_protocolSetupRoot.Transform);

        // Кнопка перехода к бою (для теста)
        var toBattleTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Position = new Vector2(0, 80),
            Size = new Vector2(200, 50),
            Origin = new Vector2(0.5f, 0.5f)
        };
        var toBattleObj = Scene.CreateGameObject("ToBattleButton", toBattleTransform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(80, 80, 80) },
            new UIText("Fonts/PixelFont", "В бой", Color.White, 1.2f, TextAlignment.Center),
            new UIButton()
        );
        toBattleObj.Transform.SetParent(_protocolSetupRoot.Transform);

        var toBattleBtn = toBattleObj.GetComponent<UIButton>();
        var toBattleSpr = toBattleObj.GetComponent<SpriteRenderer>();
        if (toBattleBtn != null && toBattleSpr != null)
        {
            toBattleBtn.OnClick += SwitchToBattle;
            toBattleBtn.OnFocusEnter += () => toBattleSpr.Color = new Color(120, 120, 120);
            toBattleBtn.OnFocusExit += () => toBattleSpr.Color = new Color(80, 80, 80);
        }
    }

    // ==================== Управление режимами ====================
    public static void SwitchToMainMenu()
    {
        SetActiveRoot(_mainMenuRoot);
        CurrentMode = GameMode.MainMenu;
    }

    public static void SwitchToBattle()
    {
        // Очистка поля ввода при входе в режим
        if (_battleRoot != null)
        {
            var inputField = _battleRoot.GetComponentInChildren<UIInputField>();
            inputField?.Clear();
        }
        SetActiveRoot(_battleRoot);
        CurrentMode = GameMode.Battle;
    }

    public static void SwitchToProtocolSetup()
    {
        SetActiveRoot(_protocolSetupRoot);
        CurrentMode = GameMode.ProtocolSetup;
    }

    private static void SetActiveRoot(GameObject activeRoot)
    {
        // Деактивируем все корневые объекты
        if (_mainMenuRoot != null) _mainMenuRoot.ActiveSelf = false;
        if (_battleRoot != null) _battleRoot.ActiveSelf = false;
        if (_protocolSetupRoot != null) _protocolSetupRoot.ActiveSelf = false;

        // Активируем нужный
        if (activeRoot != null)
            activeRoot.ActiveSelf = true;
    }
}