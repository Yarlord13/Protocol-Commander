using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MyGameEngine;
using QWERDS;
using System;

public enum GameMode
{
    None,
    MainMenu,
    Battle,
    ProtocolSetup
}

public static class MySceneBuilder
{
    public static readonly char[] AllLetters = "йцукенгшщзхъфывапролджэячсмитьбю".ToCharArray();

    private static Transform CreateRootTransform() => new Transform()
    {
        SizeModeX = SizeMode.Stretch,
        SizeModeY = SizeMode.Stretch,
        StretchBottom = 0,
        StretchLeft = 0,
        StretchRight = 0,
        StretchTop = 0
    };

    private static GameObject _mainMenuRoot;
    private static GameObject _battleRoot;
    private static GameObject _protocolSetupRoot;

    private static ProtocolSetupManager _protocolSetupMgr;
    private static GameObject[] _letterButtons;
    private static GameObject _selectedLetterButton;
    private static int _selectedLetterIndex = -1;
    private static Transform _skillListPanel;
    private static readonly List<GameObject> _skillButtons = new List<GameObject>();

    public static GameMode CurrentMode { get; private set; } = GameMode.None;

    public static void Build()
    {
        Scene.Initialize();
        GameState.Reset();
        RunStatistics.Reset();
        DifficultyManager.Reset();
        WordValidator.Initialize();

        // Создаём одного робота (Героя)
        var hero = new Robot("Герой", 120);
        hero.SkillSlots.Add(new AttackAction());
        hero.SkillSlots.Add(new HealAction());
        GameState.Robots.Add(hero);

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

            if (buttonTexts[i] == "Играть")
            {
                var btn = btnObj.GetComponent<UIButton>();
                btn.OnClick += () => SwitchToProtocolSetup();
            }
            else if (buttonTexts[i] == "Выход")
            {
                var btn = btnObj.GetComponent<UIButton>();
                btn.OnClick += () => Game1.InstanceGame?.Exit();
            }

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

        // Корень боя – растянут на всю левую половину (оставим место для правой панели)
        var battleRootTransform = new Transform()
        {
            SizeModeX = SizeMode.Stretch,
            SizeModeY = SizeMode.Stretch,
            StretchLeft = 0,
            StretchRight = 600,  // резервируем 600 пикселей справа под панель информации
            StretchTop = 0,
            StretchBottom = 0
        };
        _battleRoot = Scene.CreateGameObject("BattleRoot", battleRootTransform);
        _battleRoot.ActiveSelf = false;

        // Поле ввода
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

        // Лог битвы (левая верхняя область)
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

        // Правая панель информации о бое
        var infoPanelTransform = new Transform
        {
            SizeModeY = SizeMode.Stretch,
            StretchTop = 10,
            StretchBottom = 10,
            Anchor = new Vector2(1, 0),  // привязана к правому краю BattleRoot
            Position = new Vector2(-10, 10),
            Size = new Vector2(580, 0),
            Origin = new Vector2(1, 0)   // правый верхний угол панели
        };
        var infoPanel = Scene.CreateGameObject("BattleInfoPanel", infoPanelTransform
            //new SpriteRenderer("Sprites/Panel") { Color = new Color(20, 20, 40) }
        );
        infoPanel.Transform.SetParent(_battleRoot.Transform);
        // Добавляем компонент отображения информации (пока без UI вывода, только сбор данных)
        infoPanel.AddComponent<BattleInfoDisplay>();
    }

    // ==================== Настройка протокола ====================
    private static void CreateProtocolSetupMode()
    {
        if (_protocolSetupRoot != null) return;

        _protocolSetupRoot = Scene.CreateGameObject("ProtocolSetupRoot", CreateRootTransform());
        _protocolSetupRoot.ActiveSelf = false;

        _protocolSetupMgr = _protocolSetupRoot.AddComponent<ProtocolSetupManager>();
        //_protocolSetupMgr.OnDataChanged += RefreshProtocolUI;

        // === Верхняя панель: имя робота ===
        var topPanel = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),
            Position = new Vector2(0, 20),
            Size = new Vector2(1000, 60),
            Origin = new Vector2(0.5f, 0f)
        };
        var topObj = Scene.CreateGameObject("TopPanel", topPanel);
        topObj.Transform.SetParent(_protocolSetupRoot.Transform);

        // Имя робота
        var robotNameText = new UIText("Fonts/PixelFont", "", Color.White, 1.8f, TextAlignment.Center);
        var nameObj = Scene.CreateGameObject("RobotName", new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Size = new Vector2(400, 40),
            Origin = new Vector2(0.5f, 0.5f)
        }, robotNameText);
        nameObj.Transform.SetParent(topObj.Transform);
        // Сохраняем ссылку для обновления в RefreshProtocolUI
        // Для простоты будем обновлять через глобальную переменную, но лучше через поле класса.
        // Добавим статическое поле:
        _robotNameText = robotNameText;  // нужно добавить статическое поле private static UIText _robotNameText;

        // === Сетка букв ===
        var keyboardHandlerGo = Scene.CreateGameObject("ProtocolKeyboardHandler");
        keyboardHandlerGo.Transform.SetParent(_protocolSetupRoot.Transform);
        var handler = keyboardHandlerGo.AddComponent<ProtocolKeyboardHandler>();
        ProtocolKeyboardHandler.LetterButtons = _letterButtons;

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

        // Первое обновление UI
        RefreshProtocolUI();
    }

    // Вспомогательные методы (CreateLetterButton, OnLetterClicked, ClearLetterSelection, RemoveSelectedBinding, OnSkillClicked, CreateButton, RefreshProtocolUI, ClearSkillButtons, SetActiveRoot, SwitchToMainMenu, SwitchToBattle, SwitchToProtocolSetup) остаются те же, но с исправлениями.
    // Привожу их изменённые версии:

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

        btn.OnFocusEnter += () => { if (_selectedLetterIndex != index) spr.Color = new Color(80, 80, 80); };
        btn.OnFocusExit += () => { if (_selectedLetterIndex != index) spr.Color = new Color(50, 50, 50); };

        return go;
    }

    public static void OnLetterClicked(int index)
    {
        if (_selectedLetterIndex == index)
        {
            ClearLetterSelection();
            return;
        }

        ClearLetterSelection();
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
        if (_selectedLetterIndex < 0 || _selectedLetterIndex >= AllLetters.Length) return;
        char letter = AllLetters[_selectedLetterIndex];
        _protocolSetupMgr.UnbindAction(letter);
        RefreshProtocolUI();
    }

    private static void OnSkillClicked(ActionBase skill)
    {
        if (_selectedLetterIndex < 0) return;
        char letter = AllLetters[_selectedLetterIndex];
        _protocolSetupMgr.BindAction(letter, skill);
        RefreshProtocolUI();
    }

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

    private static void RefreshProtocolUI()
    {
        var robot = _protocolSetupMgr?.CurrentRobot;
        if (robot == null) return;

        _robotNameText.Text = $"{robot.Name} ({robot.CurrentHealth}/{robot.MaxHealth})";

        // Обновляем подписи на кнопках букв
        for (int i = 0; i < AllLetters.Length; i++)
        {
            var letter = AllLetters[i];
            var go = _letterButtons[i];
            var uiText = go.GetComponent<UIText>();
            var spr = go.GetComponent<SpriteRenderer>();

            bool hasBinding = robot.LetterBindings.TryGetValue(letter, out var action);
            float powerMod = _protocolSetupMgr.GetSkillPowerForLetter(letter);
            string displayText = hasBinding ? $"{letter}\n{action.Name}\nx{powerMod:F1}" : letter.ToString();
            uiText.Text = displayText;

            if (_selectedLetterIndex != i)
                spr.Color = new Color(50, 50, 50);
        }

        // Обновляем список навыков
        ClearSkillButtons();
        var skills = _protocolSetupMgr.GetAvailableSkills();
        float yOffset = 10;
        foreach (var skill in skills)
        {
            var skillTransform = new Transform
            {
                Anchor = new Vector2(0, 0),
                Position = new Vector2(10, yOffset),
                Size = new Vector2(300, 40),
                Origin = Vector2.Zero
            };
            var skillBtn = Scene.CreateGameObject($"Skill_{skill.Name}", skillTransform);
            // Текст без описания
            var text = skillBtn.AddComponent<UIText>();
            text.FontPath = "Fonts/PixelFont";
            text.Text = skill.Name;
            text.Color = Color.White;
            text.Scale = 1.2f;
            text.Alignment = TextAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.SortingLayer = 5;
            
            var button = skillBtn.AddComponent<UIButton>();
            var capturedSkill = skill; // для замыкания
            button.OnClick += () => OnSkillClicked(capturedSkill);
            button.OnFocusEnter += () => text.Color = Color.Yellow;
            button.OnFocusExit += () => text.Color = Color.White;
            
            skillBtn.Transform.SetParent(_skillListPanel.GameObject.Transform);
            _skillButtons.Add(skillBtn);
            yOffset += 45;
        }
        
        ClearLetterSelection();
    }

    private static void ClearSkillButtons()
    {
        foreach (var btn in _skillButtons)
            btn.Destroy();
        _skillButtons.Clear();
    }

    private static void SetActiveRoot(GameObject activeRoot)
    {
        if (_mainMenuRoot != null) _mainMenuRoot.ActiveSelf = false;
        if (_battleRoot != null) _battleRoot.ActiveSelf = false;
        if (_protocolSetupRoot != null) _protocolSetupRoot.ActiveSelf = false;

        if (activeRoot != null)
            activeRoot.ActiveSelf = true;
    }

    public static void SwitchToMainMenu()
    {
        SetActiveRoot(_mainMenuRoot);
        CurrentMode = GameMode.MainMenu;
    }

    public static void SwitchToBattle()
    {
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
        // Обновляем список доступных навыков при входе
        _protocolSetupMgr.StartNewSession();
        _protocolSetupMgr?.RefreshAvailableSkills();
        RefreshProtocolUI();
        SetActiveRoot(_protocolSetupRoot);
        CurrentMode = GameMode.ProtocolSetup;
    }

    // Добавляем недостающее статическое поле
    private static UIText _robotNameText;
}