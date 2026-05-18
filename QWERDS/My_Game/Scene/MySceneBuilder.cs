using System;
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
    public static readonly char[] AllLetters = "йцукенгшщзхъфывапролджэячсмитьбю".ToCharArray();

    private static GameObject _mainMenuRoot;
    private static GameObject _battleRoot;
    private static GameObject _protocolSetupRoot;
    private static GameObject _pauseMenuRoot;

    private static ProtocolSetupManager _protocolSetupMgr;
    private static BattleManager _battleManager;
    private static GameObject[] _letterButtons;
    private static UIText[] _multiplierLabels;
    private static int _selectedLetterIndex = -1;
    private static Transform _skillListPanel;
    private static readonly List<GameObject> _skillButtons = new List<GameObject>();
    private static UIText _selectedLetterInfoText;
    private static UIText _selectedLetterDesc;
    private static UIText _selectedLetterMultiplier;

    private static bool _isPauseMenuVisible = false;
    private static GameMode _modeBeforePause = GameMode.None;
    private static UIText _robotNameText;

    public static GameMode CurrentMode { get; private set; } = GameMode.None;

    #region Построение всех сцен

    public static void Build()
    {
        Scene.Initialize();
        ResetRun(); // начальный сброс
        CreateMainMenu();
        CreateBattleMode();
        CreateProtocolSetupMode();
        CreatePauseMenu();
        SwitchToMainMenu();
    }

    /// <summary>Полный сброс забега (очистка статистики, прогресса, привязок).</summary>
    public static void ResetRun()
    {
        GameState.ResetRun();
        RunStatistics.Reset();
        DifficultyManager.Reset();
        WordValidator.Initialize(); // перезагружает словарь, если нужно
        if (_protocolSetupMgr != null)
            _protocolSetupMgr.ClearAllBindings();
    }

    private static Transform CreateRootTransform() => new Transform()
    {
        SizeModeX = SizeMode.Stretch,
        SizeModeY = SizeMode.Stretch,
        StretchBottom = 0,
        StretchLeft = 0,
        StretchRight = 0,
        StretchTop = 0
    };

    #endregion

    #region Главное меню

    private static void CreateMainMenu()
    {
        if (_mainMenuRoot != null) return;
        _mainMenuRoot = Scene.CreateGameObject("MainMenuRoot", CreateRootTransform());
        _mainMenuRoot.ActiveSelf = false;

        var baseColor = new Color(106, 138, 57);
        var titleTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),
            Position = new Vector2(0, 100),
            Size = new Vector2(900, 120),
            Origin = new Vector2(0.5f, 0f)
        };
        var title = Scene.CreateGameObject("Title", titleTransform,
            new UIText("Fonts/PixelFont", "Protocol Commander", baseColor, 3.5f, TextAlignment.Center)
        );
        title.Transform.SetParent(_mainMenuRoot.Transform);

        string[] buttonTexts = { "Играть", "О проекте", "Выход" };
        Vector2[] offsets = { new Vector2(0, -100), new Vector2(0, 0), new Vector2(0, 100) };

        for (int i = 0; i < buttonTexts.Length; i++)
        {
            var btnTransform = new Transform
            {
                Anchor = new Vector2(0.5f, 0.5f),
                Position = offsets[i],
                Size = new Vector2(350, 70),
                Origin = new Vector2(0.5f, 0.5f)
            };
            var btnObj = Scene.CreateGameObject($"Button_{buttonTexts[i]}", btnTransform,
                new SpriteRenderer("Sprites/Panel") { Color = new Color(80, 80, 80) },
                new UIText("Fonts/PixelFont", buttonTexts[i], baseColor, 1.8f, TextAlignment.Center),
                new UIButton()
            );
            btnObj.Transform.SetParent(_mainMenuRoot.Transform);

            if (buttonTexts[i] == "Играть")
                btnObj.GetComponent<UIButton>().OnClick += () => { ResetRun(); SwitchToProtocolSetup(); };
            else if (buttonTexts[i] == "Выход")
                btnObj.GetComponent<UIButton>().OnClick += () => Game1.InstanceGame?.Exit();

            var spr = btnObj.GetComponent<SpriteRenderer>();
            var uiBtn = btnObj.GetComponent<UIButton>();
            if (spr != null && uiBtn != null)
            {
                uiBtn.OnFocusEnter += () => spr.Color = new Color(120, 120, 120);
                uiBtn.OnFocusExit += () => spr.Color = new Color(80, 80, 80);
            }
        }
    }

    #endregion

    #region Режим битвы

    private static void CreateBattleMode()
    {
        if (_battleRoot != null) return;

        var battleRootTransform = new Transform()
        {
            SizeModeX = SizeMode.Stretch,
            SizeModeY = SizeMode.Stretch,
            StretchLeft = 0,
            StretchRight = 500,
            StretchTop = 0,
            StretchBottom = 0
        };
        _battleRoot = Scene.CreateGameObject("BattleRoot", battleRootTransform);
        _battleRoot.ActiveSelf = false;

        var inputTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Position = new Vector2(0, 250),
            Size = new Vector2(700, 80),
            Origin = new Vector2(0.5f, 0.5f)
        };
        var inputObj = Scene.CreateGameObject("WordInput", inputTransform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(40, 40, 40) },
            new UIText("Fonts/PixelFont", "", Color.White, 2.0f, TextAlignment.Left, VerticalAlignment.Center) { LayerDepth = 0.1f },
            new UIInputField { Placeholder = "Введите слово...", MaxLength = 20 }
        );
        inputObj.Transform.SetParent(_battleRoot.Transform);

        var logTransform = new Transform
        {
            Anchor = new Vector2(0, 0),
            Position = new Vector2(30, 30),
            Size = new Vector2(550, 250),
            Origin = Vector2.Zero
        };
        var logObj = Scene.CreateGameObject("BattleLog", logTransform,
            new UIText("Fonts/PixelFont", "", Color.White, 1.3f, TextAlignment.Left, VerticalAlignment.Top)
        );
        logObj.Transform.SetParent(_battleRoot.Transform);

        _battleManager = _battleRoot.AddComponent<BattleManager>();
        var logText = logObj.GetComponent<UIText>();
        _battleManager.OnLogMessage += (msg) => logText.Text = msg;
        _battleManager.OnBattleEnd += OnBattleEnd;

        var battleLabelTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),
            Position = new Vector2(0, 40),
            Size = new Vector2(500, 50),
            Origin = new Vector2(0.5f, 0f)
        };
        var battleLabel = Scene.CreateGameObject("BattleLabel", battleLabelTransform,
            new UIText("Fonts/PixelFont", "Режим боя", Color.Red, 2.5f, TextAlignment.Center)
        );
        battleLabel.Transform.SetParent(_battleRoot.Transform);

        var pauseBtnTransform = new Transform
        {
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-20, 20),
            Size = new Vector2(70, 70),
            Origin = new Vector2(1, 0)
        };
        var pauseBtn = CreateButton("PauseButton_Battle", pauseBtnTransform, "Меню", TogglePauseMenu);
        pauseBtn.Transform.SetParent(_battleRoot.Transform);

        var infoPanelTransform = new Transform
        {
            SizeModeY = SizeMode.Stretch,
            StretchTop = 10,
            StretchBottom = 10,
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-20, 10),
            Size = new Vector2(470, 0),
            Origin = new Vector2(1, 0)
        };
        var infoPanel = Scene.CreateGameObject("BattleInfoPanel", infoPanelTransform);
        infoPanel.Transform.SetParent(_battleRoot.Transform);
        infoPanel.AddComponent<BattleInfoDisplay>();
    }

    private static void OnBattleEnd(bool victory)
{
    if (victory)
    {
        // победа – через VictoryDelayHandler переключится на настройку
        if (_battleRoot != null)
        {
            var inputField = _battleRoot.GetComponentInChildren<UIInputField>();
            if (inputField != null) inputField.IsFocused = false;
        }
    }
    else
    {
        // поражение – сразу в меню (можно с паузой через корутину, но пока без)
        if (_battleManager != null)
            _battleManager.LogMessage("Поражение! Возврат в главное меню...");
        SwitchToMainMenu();
    }
}

    #endregion

    #region Настройка протокола (клавиатура + навыки)

    private static void CreateProtocolSetupMode()
    {
        if (_protocolSetupRoot != null) return;

        _protocolSetupRoot = Scene.CreateGameObject("ProtocolSetupRoot", CreateRootTransform());
        _protocolSetupRoot.ActiveSelf = false;
        _protocolSetupMgr = _protocolSetupRoot.AddComponent<ProtocolSetupManager>();
        _protocolSetupMgr.Initialize();
        _protocolSetupMgr.OnDataChanged += OnProtocolDataChanged;

        // Верхняя панель
        var topPanel = new Transform
        {
            Anchor = new Vector2(0.5f, 0f),
            Position = new Vector2(0, 20),
            Size = new Vector2(1200, 80),
            Origin = new Vector2(0.5f, 0f)
        };
        var topObj = Scene.CreateGameObject("TopPanel", topPanel);
        topObj.Transform.SetParent(_protocolSetupRoot.Transform);
        _robotNameText = new UIText("Fonts/PixelFont", "", Color.White, 2.0f, TextAlignment.Center);
        var nameObj = Scene.CreateGameObject("RobotName", new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Size = new Vector2(600, 60),
            Origin = new Vector2(0.5f, 0.5f)
        }, _robotNameText);
        nameObj.Transform.SetParent(topObj.Transform);

        // Создаём клавиатуру
        CreateKeyboardLayout();
        ProtocolKeyboardHandler.LetterButtons = _letterButtons;

        // Панель навыков
        _skillListPanel = new Transform
        {
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-380, 130),
            Size = new Vector2(350, 420),
            Origin = new Vector2(0, 0)
        };
        var panelGo = Scene.CreateGameObject("SkillListPanel", _skillListPanel,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(30, 30, 30), SortingLayer = 2 }
        );
        panelGo.Transform.SetParent(_protocolSetupRoot.Transform);

        // Информационная панель
        var infoPanelTransform = new Transform
        {
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-380, 570),
            Size = new Vector2(350, 200),
            Origin = new Vector2(0, 0)
        };
        var infoPanel = Scene.CreateGameObject("SelectedLetterInfo", infoPanelTransform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(20, 20, 40), SortingLayer = 2 }
        );
        infoPanel.Transform.SetParent(_protocolSetupRoot.Transform);

        _selectedLetterInfoText = new UIText("Fonts/PixelFont", "", Color.Cyan, 1.5f, TextAlignment.Center) { SortingLayer = 10 };
        var skillNameGo = Scene.CreateGameObject("SkillName", new Transform
        {
            Anchor = new Vector2(0.5f, 0),
            Position = new Vector2(0, 10),
            Size = new Vector2(330, 40),
            Origin = new Vector2(0.5f, 0)
        }, _selectedLetterInfoText);
        skillNameGo.Transform.SetParent(infoPanel.Transform);

        _selectedLetterDesc = new UIText("Fonts/PixelFont", "", Color.White, 1.2f, TextAlignment.Left, VerticalAlignment.Top) { SortingLayer = 10 };
        var descGo = Scene.CreateGameObject("SkillDesc", new Transform
        {
            Anchor = new Vector2(0, 0),
            Position = new Vector2(10, 60),
            Size = new Vector2(330, 70),
            Origin = Vector2.Zero
        }, _selectedLetterDesc);
        descGo.Transform.SetParent(infoPanel.Transform);

        _selectedLetterMultiplier = new UIText("Fonts/PixelFont", "", Color.Yellow, 1.2f, TextAlignment.Left) { SortingLayer = 10 };
        var multGo = Scene.CreateGameObject("Multiplier", new Transform
        {
            Anchor = new Vector2(0, 0),
            Position = new Vector2(10, 140),
            Size = new Vector2(330, 30),
            Origin = Vector2.Zero
        }, _selectedLetterMultiplier);
        multGo.Transform.SetParent(infoPanel.Transform);

        var removeBtn = CreateButton("RemoveBinding", new Transform
        {
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-380, 790),
            Size = new Vector2(350, 50),
            Origin = new Vector2(0, 0)
        }, "Удалить привязку", RemoveSelectedBinding);
        removeBtn.Transform.SetParent(_protocolSetupRoot.Transform);

        var toBattleBtn = CreateButton("ToBattle", new Transform
        {
            Anchor = new Vector2(1, 1),
            Position = new Vector2(-380, -90),
            Size = new Vector2(350, 60),
            Origin = new Vector2(0, 1)
        }, "В бой", () => SwitchToBattle());
        toBattleBtn.Transform.SetParent(_protocolSetupRoot.Transform);

        var pauseBtn = CreateButton("PauseButton_Setup", new Transform
        {
            Anchor = new Vector2(1, 0),
            Position = new Vector2(-20, 20),
            Size = new Vector2(70, 70),
            Origin = new Vector2(1, 0)
        }, "Меню", TogglePauseMenu);
        pauseBtn.Transform.SetParent(_protocolSetupRoot.Transform);

        RefreshLetterButtons();
        RefreshSkillList();
    }

    private static void CreateKeyboardLayout()
    {
        string[] rows = { "йцукенгшщзхъ", "фывапролджэ", "ячсмитьбю" };
        float[] rowXOffsets = { 0, 45, 90 };
        float startY = 130;
        float keyWidth = 85;
        float keyHeight = 85;
        float spacing = 8;

        _letterButtons = new GameObject[AllLetters.Length];
        _multiplierLabels = new UIText[AllLetters.Length];

        int globalIndex = 0;
        for (int row = 0; row < rows.Length; row++)
        {
            string rowLetters = rows[row];
            float startX = 100 + rowXOffsets[row];
            for (int col = 0; col < rowLetters.Length; col++)
            {
                char letter = rowLetters[col];
                float x = startX + col * (keyWidth + spacing);
                float y = startY + row * (keyHeight + spacing);
                var btnGo = CreateLetterButton(letter, globalIndex, x, y, keyWidth, keyHeight);
                btnGo.Transform.SetParent(_protocolSetupRoot.Transform);
                _letterButtons[globalIndex] = btnGo;
                globalIndex++;
            }
        }
    }

    private static GameObject CreateLetterButton(char letter, int index, float x, float y, float width, float height)
    {
        var transform = new Transform
        {
            Anchor = new Vector2(0, 0),
            Position = new Vector2(x, y),
            Size = new Vector2(width, height),
            Origin = new Vector2(0.5f, 0.5f)
        };
        var go = Scene.CreateGameObject($"Letter_{letter}", transform,
            new SpriteRenderer("Sprites/Panel") { Color = Color.Gray },
            new UIButton()
        );
        // Буква
        var text = go.AddComponent<UIText>();
        text.FontPath = "Fonts/PixelFont";
        text.Text = letter.ToString();
        text.Color = Color.White;
        text.Scale = 2.2f;
        text.Alignment = TextAlignment.Center;
        text.VerticalAlignment = VerticalAlignment.Center;
        text.SortingLayer = 10;

        // Множитель
        var multText = go.AddComponent<UIText>();
        multText.FontPath = "Fonts/PixelFont";
        multText.Text = "";
        multText.Scale = 0.9f;
        multText.Alignment = TextAlignment.Right;
        multText.VerticalAlignment = VerticalAlignment.Bottom;
        multText.SortingLayer = 10;
        _multiplierLabels[index] = multText;

        var btn = go.GetComponent<UIButton>();
        btn.OnClick += () => OnLetterClicked(index);
        return go;
    }

    public static void OnLetterClicked(int index)
    {
        if (index < 0 || index >= _letterButtons.Length) return;
        if (_selectedLetterIndex == index)
        {
            ClearLetterSelection();
            return;
        }
        ClearLetterSelection();
        _selectedLetterIndex = index;
        UpdateSelectedLetterInfo();
        RefreshLetterButtons();
    }

    private static void ClearLetterSelection()
    {
        _selectedLetterIndex = -1;
        UpdateSelectedLetterInfo();
        RefreshLetterButtons();
    }

    private static void UpdateSelectedLetterInfo()
    {
        if (_selectedLetterIndex < 0)
        {
            _selectedLetterInfoText.Text = "";
            _selectedLetterDesc.Text = "Выберите букву";
            _selectedLetterMultiplier.Text = "";
            return;
        }
        char letter = AllLetters[_selectedLetterIndex];
        var skill = _protocolSetupMgr.GetBoundSkill(letter);
        float mult = _protocolSetupMgr.GetSkillPowerForLetter(letter);
        _selectedLetterMultiplier.Text = $"Множитель силы: {mult:F2}";
        if (skill == null)
        {
            _selectedLetterInfoText.Text = "Нет навыка";
            _selectedLetterDesc.Text = "Нажмите на любой навык справа, чтобы привязать его к этой букве.";
        }
        else
        {
            _selectedLetterInfoText.Text = skill.Name;
            _selectedLetterDesc.Text = skill.Description;
        }
    }

    private static void OnSkillClicked(ActionBase skill)
    {
        if (_selectedLetterIndex < 0) return;
        char letter = AllLetters[_selectedLetterIndex];
        if (_protocolSetupMgr.BindAction(letter, skill))
        {
            RefreshSkillList();
            RefreshLetterButtons();
            UpdateSelectedLetterInfo();
        }
    }

    private static void RemoveSelectedBinding()
    {
        if (_selectedLetterIndex < 0) return;
        char letter = AllLetters[_selectedLetterIndex];
        _protocolSetupMgr.UnbindAction(letter);
        RefreshSkillList();
        RefreshLetterButtons();
        UpdateSelectedLetterInfo();
    }

    private static void RefreshLetterButtons()
    {
        var robot = _protocolSetupMgr?.CurrentRobot;
        if (robot == null) return;

        for (int i = 0; i < AllLetters.Length; i++)
        {
            char letter = AllLetters[i];
            var go = _letterButtons[i];
            if (go == null) continue;

            var spr = go.GetComponent<SpriteRenderer>();
            bool hasBinding = robot.LetterBindings.ContainsKey(letter);
            bool isSelected = (i == _selectedLetterIndex);

            if (isSelected)
                spr.Color = new Color(70, 130, 200);
            else if (hasBinding)
                spr.Color = new Color(60, 120, 60);
            else
                spr.Color = new Color(80, 80, 80);

            float mult = _protocolSetupMgr.GetSkillPowerForLetter(letter);
            if (_multiplierLabels[i] != null)
            {
                _multiplierLabels[i].Text = $"{mult:F1}";
                float t = (mult - 0.7f) / 0.8f;
                t = Math.Clamp(t, 0f, 1f);
                _multiplierLabels[i].Color = Color.Lerp(Color.Red, Color.Green, t);
            }
        }
    }

    private static void RefreshSkillList()
    {
        foreach (var btn in _skillButtons)
            btn.Destroy();
        _skillButtons.Clear();

        var skills = _protocolSetupMgr.UnboundSkills;
        float yOffset = 10;
        foreach (var skill in skills)
        {
            var skillTransform = new Transform
            {
                Anchor = new Vector2(0, 0),
                Position = new Vector2(10, yOffset),
                Size = new Vector2(330, 65),
                Origin = Vector2.Zero
            };
            var skillBtn = Scene.CreateGameObject($"Skill_{skill.Name}", skillTransform,
                new SpriteRenderer("Sprites/Panel") { Color = new Color(60, 60, 60) }
            );
            var text = skillBtn.AddComponent<UIText>();
            text.FontPath = "Fonts/PixelFont";
            text.Text = $"{skill.Name}\n{skill.Description}";
            text.Color = Color.White;
            text.Scale = 1.0f;
            text.Alignment = TextAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.SortingLayer = 10;

            var button = skillBtn.AddComponent<UIButton>();
            var capturedSkill = skill;
            button.OnClick += () => OnSkillClicked(capturedSkill);
            button.OnFocusEnter += () => text.Color = Color.Yellow;
            button.OnFocusExit += () => text.Color = Color.White;

            skillBtn.Transform.SetParent(_skillListPanel.GameObject.Transform);
            _skillButtons.Add(skillBtn);
            yOffset += 75;
        }
    }

    private static void OnProtocolDataChanged()
    {
        RefreshLetterButtons();
        RefreshSkillList();
        UpdateSelectedLetterInfo();
    }

    #endregion

    #region Меню паузы и вспомогательные методы

    private static void CreatePauseMenu()
    {
        if (_pauseMenuRoot != null) return;

        _pauseMenuRoot = Scene.CreateGameObject("PauseMenuRoot", CreateRootTransform());
        _pauseMenuRoot.ActiveSelf = false;

        var bgTransform = new Transform
        {
            SizeModeX = SizeMode.Stretch,
            SizeModeY = SizeMode.Stretch,
            StretchLeft = 0, StretchRight = 0, StretchTop = 0, StretchBottom = 0
        };
        var bg = Scene.CreateGameObject("PauseBackground", bgTransform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(0, 0, 0, 180), SortingLayer = 100 }
        );
        bg.Transform.SetParent(_pauseMenuRoot.Transform);

        var panelTransform = new Transform
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Position = Vector2.Zero,
            Size = new Vector2(500, 380),
            Origin = new Vector2(0.5f, 0.5f)
        };
        var panel = Scene.CreateGameObject("PausePanel", panelTransform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(40, 40, 40), SortingLayer = 101 }
        );
        panel.Transform.SetParent(_pauseMenuRoot.Transform);

        string[] btnTexts = { "Выйти в меню", "Настройки", "Журнал", "Выход из игры" };
        float[] yOffsets = { -120, -40, 40, 120 };
        for (int i = 0; i < btnTexts.Length; i++)
        {
            var btnTransform = new Transform
            {
                Anchor = new Vector2(0.5f, 0.5f),
                Position = new Vector2(0, yOffsets[i]),
                Size = new Vector2(300, 55),
                Origin = new Vector2(0.5f, 0.5f)
            };
            var btnGo = Scene.CreateGameObject($"PauseButton_{btnTexts[i]}", btnTransform,
                new SpriteRenderer("Sprites/Panel") { Color = new Color(80, 80, 80), SortingLayer = 102 },
                new UIText("Fonts/PixelFont", btnTexts[i], Color.White, 1.4f, TextAlignment.Center, VerticalAlignment.Center) { SortingLayer = 103 },
                new UIButton()
            );
            btnGo.Transform.SetParent(panel.Transform);

            var btn = btnGo.GetComponent<UIButton>();
            var spr = btnGo.GetComponent<SpriteRenderer>();
            btn.OnFocusEnter += () => spr.Color = new Color(120, 120, 120);
            btn.OnFocusExit += () => spr.Color = new Color(80, 80, 80);

            if (btnTexts[i] == "Выйти в меню")
                btn.OnClick += () => { HidePauseMenu(); SwitchToMainMenu(); };
            else if (btnTexts[i] == "Выход из игры")
                btn.OnClick += () => Game1.InstanceGame?.Exit();
        }
    }

    public static void TogglePauseMenu()
    {
        if (CurrentMode == GameMode.MainMenu) return;
        if (!_isPauseMenuVisible) ShowPauseMenu();
        else HidePauseMenu();
    }

    private static void ShowPauseMenu()
    {
        _modeBeforePause = CurrentMode;
        _pauseMenuRoot.ActiveSelf = true;
        _isPauseMenuVisible = true;
        if (CurrentMode == GameMode.Battle)
        {
            var inputField = _battleRoot?.GetComponentInChildren<UIInputField>();
            if (inputField != null) inputField.IsFocused = false;
        }
    }

    private static void HidePauseMenu()
    {
        _pauseMenuRoot.ActiveSelf = false;
        _isPauseMenuVisible = false;
    }

    private static GameObject CreateButton(string name, Transform transform, string text, Action onClick)
    {
        var go = Scene.CreateGameObject(name, transform,
            new SpriteRenderer("Sprites/Panel") { Color = new Color(80, 80, 80) },
            new UIText("Fonts/PixelFont", text, Color.White, 1.5f, TextAlignment.Center, VerticalAlignment.Center) { SortingLayer = 10 },
            new UIButton()
        );
        var btn = go.GetComponent<UIButton>();
        var spr = go.GetComponent<SpriteRenderer>();
        btn.OnClick += onClick;
        btn.OnFocusEnter += () => spr.Color = new Color(120, 120, 120);
        btn.OnFocusExit += () => spr.Color = new Color(80, 80, 80);
        return go;
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
        HidePauseMenu();
        // полный сброс забега
        ResetRun();
        // также нужно пересоздать боевой менеджер? Нет, он переинициализируется при следующем входе в бой
        if (_battleManager != null)
        {
            // отпишемся от событий, чтобы не вызывались повторно
            _battleManager.OnBattleEnd -= OnBattleEnd;
            // и пересоздадим? Проще оставить, при следующем SwitchToBattle вызовется RestartBattle()
        }
    }

    public static void SwitchToBattle()
    {
        if (_battleRoot != null)
        {
            var inputField = _battleRoot.GetComponentInChildren<UIInputField>();
            inputField?.Clear();
            // переинициализируем бой (новые враги, свежие роботы из GameState)
            if (_battleManager != null)
            {
                _battleManager.RestartBattle();
                // подпишемся заново (если отписывались)
                _battleManager.OnBattleEnd -= OnBattleEnd;
                _battleManager.OnBattleEnd += OnBattleEnd;
            }
        }
        SetActiveRoot(_battleRoot);
        CurrentMode = GameMode.Battle;
    }

    public static void SwitchToProtocolSetup()
    {
        _protocolSetupMgr.StartNewSession();
        _protocolSetupMgr.RefreshAvailableSkills();
        RefreshLetterButtons();
        RefreshSkillList();
        ClearLetterSelection();
        SetActiveRoot(_protocolSetupRoot);
        CurrentMode = GameMode.ProtocolSetup;
    }

    #endregion
}