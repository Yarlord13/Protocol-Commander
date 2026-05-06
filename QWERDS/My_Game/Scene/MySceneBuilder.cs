using System.Collections.Generic;
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
    // Все буквы русского алфавита
    private static readonly char[] AllLetters = "абвгдежзийклмнопрстуфхцчшщъыьэюя".ToCharArray();

    private static Transform CreateRootTransform() => new Transform()
    {
        SizeModeX = SizeMode.Stretch,
        SizeModeY = SizeMode.Stretch,
        StretchBottom = 0,
        StretchLeft = 0,
        StretchRight = 0,
        StretchTop = 0
    };

    // Корневые объекты
    private static GameObject _mainMenuRoot;
    private static GameObject _battleRoot;
    private static GameObject _protocolSetupRoot;

    // Менеджеры
    private static ProtocolSetupManager _protocolSetupMgr;

    // UI настройки протокола
    private static UIText _robotNameText;
    private static GameObject[] _letterButtons; // по индексу буквы в AllLetters
    private static GameObject _selectedLetterButton;
    private static int _selectedLetterIndex = -1; // -1 = нет выбора
    private static Transform _skillListPanel;
    private static readonly List<GameObject> _skillButtons = new List<GameObject>();

    public static GameMode CurrentMode { get; private set; } = GameMode.None;
    public static void Build()
    {
        Scene.Initialize();

        // Инициализация глобального состояния
        GameState.Reset();
        // Три робота с начальными навыками
        GameState.Robots.Add(new Robot("Альфа"));
        GameState.Robots.Add(new Robot("Браво"));
        GameState.Robots.Add(new Robot("Чарли"));
        // Начальные привязки (пример)
        GameState.Robots[0].SkillSlots.Add(new AttackAction());
        GameState.Robots[0].SkillSlots.Add(new HealAction());
        GameState.Robots[0].LetterBindings['а'] = GameState.Robots[0].SkillSlots[0];
        GameState.Robots[0].LetterBindings['л'] = GameState.Robots[0].SkillSlots[1];

        WordValidator.Initialize();

        CreateMainMenu();
        CreateBattleMode();
        CreateProtocolSetupMode();

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

    // ==================== Режим битвы (небольшие дополнения) ====================
    private static void CreateBattleMode()
    {
        if (_battleRoot != null) return;

        var battleRootTransform = new Transform()
        {
            SizeModeX = SizeMode.Stretch,
            SizeModeY = SizeMode.Stretch,
            StretchBottom = 0,
            StretchLeft = 0,
            StretchRight = 960,
            StretchTop = 0
        };
        _battleRoot = Scene.CreateGameObject("BattleRoot", battleRootTransform);
        _battleRoot.ActiveSelf = false;

        // Поле ввода (как раньше)
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

        // Лог битвы
        var logTransform = new Transform
        {
            Anchor = new Vector2(0, 0),
            Position = new Vector2(20, 20),
            Size = new Vector2(500, 200),
            Origin = Vector2.Zero
        };
        var logObj = Scene.CreateGameObject("BattleLog", logTransform,
            new UIText("Fonts/PixelFont", "", Color.White, 1.2f, TextAlignment.Left, VerticalAlignment.Top)
        );
        logObj.Transform.SetParent(_battleRoot.Transform);

        // Менеджер битвы
        var battleMgr = _battleRoot.AddComponent<BattleManager>();
        var logText = logObj.GetComponent<UIText>();
        battleMgr.OnLogMessage += (msg) => logText.Text = msg;

        // Заголовок
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

        // Кнопка "В меню"
        var backTransform = new Transform
        {
            Anchor = new Vector2(0, 1),
            Position = new Vector2(20, -20),
            Size = new Vector2(150, 50),
            Origin = new Vector2(0, 1)
        };
        var backObj = CreateButton("BackToMenu_Battle", backTransform, "В меню", SwitchToMainMenu);
        backObj.Transform.SetParent(_battleRoot.Transform);

        // Кнопка "Протокол" (справа снизу, для теста)
        var protocolTransform = new Transform
        {
            Anchor = new Vector2(1, 1),          // правый нижний угол родителя (BattleRoot)
            Position = new Vector2(-20, -20),    // отступ по 20 пикселей от краёв
            Size = new Vector2(150, 50),
            Origin = new Vector2(1, 1)           // правый нижний угол самой кнопки
        };
        var protocolObj = CreateButton("ToProtocol_Battle", protocolTransform, "Протокол", SwitchToProtocolSetup);
        protocolObj.Transform.SetParent(_battleRoot.Transform);
    }

    // ==================== Настройка протокола ====================
    private static void CreateProtocolSetupMode()
    {
        if (_protocolSetupRoot != null) return;

        _protocolSetupRoot = Scene.CreateGameObject("ProtocolSetupRoot", CreateRootTransform());
        _protocolSetupRoot.ActiveSelf = false;

        // Менеджер настройки
        _protocolSetupMgr = _protocolSetupRoot.AddComponent<ProtocolSetupManager>();

        // === Верхняя панель: выбор робота ===
        var topPanel = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),
            Position = new Vector2(0, 20),
            Size = new Vector2(1000, 60),
            Origin = new Vector2(0.5f, 0f)
        };
        var topObj = Scene.CreateGameObject("TopPanel", topPanel);
        topObj.Transform.SetParent(_protocolSetupRoot.Transform);

        // Кнопка "<"
        var prevBtn = CreateButton("PrevRobot", new Transform
        {
            Anchor = Vector2.Zero,
            Position = new Vector2(10, 10),
            Size = new Vector2(50, 40),
            Origin = Vector2.Zero
        }, "<", () =>
        {
            _protocolSetupMgr.PreviousRobot();
            RefreshProtocolUI();
        });
        prevBtn.Transform.SetParent(topObj.Transform);

        // Кнопка ">"
        var nextBtn = CreateButton("NextRobot", new Transform
        {
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-100, 10),
            Size = new Vector2(50, 40),
            Origin = Vector2.Zero
        }, ">", () =>
        {
            _protocolSetupMgr.NextRobot();
            RefreshProtocolUI();
        });
        nextBtn.Transform.SetParent(topObj.Transform);

        // Имя робота
        _robotNameText = new UIText("Fonts/PixelFont", "", Color.White, 1.8f, TextAlignment.Center)
        {
            LayerDepth = 0f,
            SortingLayer = 1
        };
        var nameObj = Scene.CreateGameObject("RobotName", new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Size = new Vector2(300, 40),
            Origin = new Vector2(0.5f, 0.5f)
        }, _robotNameText);
        nameObj.Transform.SetParent(topObj.Transform);

        // === Сетка букв ===
        _letterButtons = new GameObject[AllLetters.Length];
        const float startX = 100, startY = 120;
        const float cellWidth = 70, cellHeight = 80;
        const int columns = 11;

        for (int i = 0; i < AllLetters.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            float x = startX + col * cellWidth;
            float y = startY + row * cellHeight;

            var letter = AllLetters[i];
            var btnGo = CreateLetterButton(letter, i, x, y);
            btnGo.Transform.SetParent(_protocolSetupRoot.Transform);
            _letterButtons[i] = btnGo;
        }

        // === Панель навыков (справа) ===
        _skillListPanel = new Transform
        {
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-300, 120),
            Size = new Vector2(250, 400),
            Origin = new Vector2(0, 0)
        };
        var panelGo = Scene.CreateGameObject("SkillListPanel", _skillListPanel,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(30, 30, 30), SortingLayer = 2 }
        );
        panelGo.Transform.SetParent(_protocolSetupRoot.Transform);

        // Кнопка "Удалить привязку"
        var removeBtn = CreateButton("RemoveBinding", new Transform
        {
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-300, 540),
            Size = new Vector2(250, 40),
            Origin = new Vector2(0, 0)
        }, "Удалить привязку", RemoveSelectedBinding);
        removeBtn.Transform.SetParent(_protocolSetupRoot.Transform);

        // Кнопка "В бой"
        var toBattleBtn = CreateButton("ToBattle", new Transform
        {
            Anchor = new Vector2(1, 1),
            Position = new Vector2(-300, -80),
            Size = new Vector2(250, 50),
            Origin = new Vector2(0, 1)
        }, "В бой", SwitchToBattle);
        toBattleBtn.Transform.SetParent(_protocolSetupRoot.Transform);

        // Кнопка "В меню"
        var toMenuBtn = CreateButton("ToMenu_Setup", new Transform
        {
            Anchor = new Vector2(0, 1),
            Position = new Vector2(20, -20),
            Size = new Vector2(150, 50),
            Origin = new Vector2(0, 1)
        }, "В меню", SwitchToMainMenu);
        toMenuBtn.Transform.SetParent(_protocolSetupRoot.Transform);
    }

    private static GameObject CreateLetterButton(char letter, int index, float x, float y)
    {
        var transform = new Transform
        {
            Anchor = new Vector2(0, 0),
            Position = new Vector2(x, y),
            Size = new Vector2(60, 60),
            Origin = new Vector2(0.5f, 0.5f)
        };

        var go = Scene.CreateGameObject($"Letter_{letter}", transform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(50, 50, 50) },
            new UIText("Fonts/PixelFont", letter.ToString(), Color.White, 1.5f, TextAlignment.Center, VerticalAlignment.Center) { SortingLayer = 1 },
            new UIButton()
        );

        var btn = go.GetComponent<UIButton>();
        var spr = go.GetComponent<SpriteRenderer>();

        btn.OnClick += () => OnLetterClicked(index);

        // Визуальный отклик
        btn.OnFocusEnter += () => { if (_selectedLetterIndex != index) spr.Color = new Color(80, 80, 80); };
        btn.OnFocusExit += () => { if (_selectedLetterIndex != index) spr.Color = new Color(50, 50, 50); };

        return go;
    }

    private static void OnLetterClicked(int index)
    {
        if (_selectedLetterIndex == index)
        {
            // Повторный клик – снять выделение
            ClearLetterSelection();
            return;
        }

        // Снять выделение с предыдущей
        ClearLetterSelection();

        // Выделить новую
        _selectedLetterIndex = index;
        _selectedLetterButton = _letterButtons[index];
        var spr = _selectedLetterButton.GetComponent<SpriteRenderer>();
        if (spr != null)
            spr.Color = new Color(100, 100, 200);
    }

    private static void ClearLetterSelection()
    {
        if (_selectedLetterButton != null)
        {
            var spr = _selectedLetterButton.GetComponent<SpriteRenderer>();
            if (spr != null)
                spr.Color = new Color(50, 50, 50);
        }
        _selectedLetterButton = null;
        _selectedLetterIndex = -1;
    }

    private static void RemoveSelectedBinding()
    {
        if (_selectedLetterIndex < 0 || _selectedLetterIndex >= AllLetters.Length)
            return;

        char letter = AllLetters[_selectedLetterIndex];
        _protocolSetupMgr.UnbindAction(letter);
        RefreshProtocolUI();
    }

    private static void OnSkillClicked(ActionBase skill)
    {
        if (_selectedLetterIndex < 0 || _selectedLetterIndex >= AllLetters.Length)
            return;

        char letter = AllLetters[_selectedLetterIndex];
        _protocolSetupMgr.BindAction(letter, skill);
        RefreshProtocolUI();
    }

    // Утилита создания кнопки с текстом
    private static GameObject CreateButton(string name, Transform transform, string text, System.Action onClick)
    {
        var go = Scene.CreateGameObject(name, transform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(80, 80, 80) },
            new UIText("Fonts/PixelFont", text, Color.White, 1.2f, TextAlignment.Center, VerticalAlignment.Center) { SortingLayer = 1 },
            new UIButton()
        );

        var btn = go.GetComponent<UIButton>();
        var spr = go.GetComponent<SpriteRenderer>();
        btn.OnClick += onClick;
        btn.OnFocusEnter += () => spr.Color = new Color(120, 120, 120);
        btn.OnFocusExit += () => spr.Color = new Color(80, 80, 80);

        return go;
    }

    // Обновление интерфейса настройки протокола
    private static void RefreshProtocolUI()
    {
        var robot = _protocolSetupMgr?.CurrentRobot;
        if (robot == null) return;

        _robotNameText.Text = $"{robot.Name} ({robot.CurrentHealth}/{robot.MaxHealth})";

        // Обновить подписи на кнопках букв: показать привязанное действие
        for (int i = 0; i < AllLetters.Length; i++)
        {
            var letter = AllLetters[i];
            var go = _letterButtons[i];
            var uiText = go.GetComponent<UIText>();
            var spr = go.GetComponent<SpriteRenderer>();

            bool hasBinding = robot.LetterBindings.TryGetValue(letter, out var action);
            string displayText = hasBinding ? $"{letter}\n{action.Name[0]}" : letter.ToString(); // первая буква действия
            uiText.Text = displayText;

            // Сброс цвета (кроме выделенной)
            if (_selectedLetterIndex != i)
                spr.Color = new Color(50, 50, 50);
        }

        // Обновить список навыков
        ClearSkillButtons();
        float yOffset = 10;
        foreach (var skill in robot.SkillSlots)
        {
            var skillTransform = new Transform
            {
                Anchor = new Vector2(0, 0),
                Position = new Vector2(10, yOffset),
                Size = new Vector2(230, 40),
                Origin = Vector2.Zero
            };
            var skillBtn = CreateButton($"Skill_{skill.Name}", skillTransform, skill.Name, () => OnSkillClicked(skill));
            skillBtn.Transform.SetParent(_skillListPanel.GameObject.Transform);
            _skillButtons.Add(skillBtn);
            yOffset += 45;
        }

        // Сброс выделения буквы
        ClearLetterSelection();
    }

    private static void ClearSkillButtons()
    {
        foreach (var btn in _skillButtons)
            btn.Destroy();
        _skillButtons.Clear();
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