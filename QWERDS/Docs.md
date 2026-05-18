# Документация проекта «Protocol Commander» (актуальная версия)

## Оглавление
1. [Обзор проекта](#обзор-проекта)
2. [Архитектура движка](#архитектура-движка)
   - [GameObject и компоненты](#gameobject-и-компоненты)
   - [Transform и система координат](#transform-и-система-координат)
   - [Сцена (Scene)](#сцена-scene)
   - [Менеджер разрешения (ResolutionManager)](#менеджер-разрешения-resolutionmanager)
   - [Ввод (InputManager)](#ввод-inputmanager)
   - [Компоненты отрисовки и UI](#компоненты-отрисовки-и-ui)
   - [Анимации](#анимации)
3. [Игровые механики](#игровые-механики)
   - [Сущности: Robot и Enemy](#сущности-robot-и-enemy)
   - [Действия (ActionBase и EnemyAction)](#действия-actionbase-и-enemyaction)
   - [BattleManager – управление боем](#battlemanager--управление-боем)
   - [ProtocolSetupManager – настройка протокола](#protocolsetupmanager--настройка-протокола)
   - [Валидация слов (WordValidator)](#валидация-слов-wordvalidator)
   - [Глобальное состояние (GameState)](#глобальное-состояние-gamestate)
   - [Статистика (RunStatistics)](#статистика-runstatistics)
   - [Сложность и адаптация (DifficultyManager, LetterFrequency)](#сложность-и-адаптация-difficultymanager-letterfrequency)
   - [Вспомогательные компоненты](#вспомогательные-компоненты)
4. [Построение сцен и интерфейс (MySceneBuilder)](#построение-сцен-и-интерфейс-myscenebuilder)
   - [Главное меню](#главное-меню)
   - [Режим битвы](#режим-битвы)
   - [Настройка протокола](#настройка-протокола)
   - [Меню паузы](#меню-паузы)
   - [Переключение режимов](#переключение-режимов)
5. [Расширение функциональности](#расширение-функциональности)
   - [Добавление нового действия робота](#добавление-нового-действия-робота)
   - [Добавление нового врага или вражеского действия](#добавление-нового-врага-или-вражеского-действия)
   - [Добавление спецэффектов слов](#добавление-спецэффектов-слов)
   - [Добавление тегов (магазин)](#добавление-тегов-магазин)
   - [Создание нового UI-компонента](#создание-нового-ui-компонента)
6. [Интеграция с MonoGame](#интеграция-с-monogame)
7. [Правила оформления кода](#правила-оформления-кода)

---

## Обзор проекта

**Protocol Commander** – тактический рогалик с лингвистической механикой. Игрок управляет тремя роботами, отправляя слова, где каждая буква активирует действия. Роботы выполняют действия первыми на каждой букве, затем враги. Между боями игрок настраивает протокол: назначает действия роботов на буквы алфавита. Проект написан на C# + MonoGame, использует самописный компонентный движок, вдохновлённый Unity.

---

## Архитектура движка

### GameObject и компоненты

`GameObject` – контейнер для компонентов (`Behaviour`). Всегда содержит `Transform`. Создаётся через `Scene.CreateGameObject`.

**Ключевые члены `GameObject`:**
- `Name` – имя объекта (для отладки).
- `Transform` – компонент трансформации.
- `ActiveSelf` – локальная активность.
- `ActiveInHierarchy` – активность с учётом родителей.
- `AddComponent<T>()` – добавляет компонент типа `T`.
- `GetComponent<T>()`, `GetComponentInChildren<T>()`, `GetComponents<T>()` – поиск компонентов.
- `Destroy()` – уничтожает объект.

**Базовый класс `Behaviour`:**
- `Start()` – вызывается один раз при активации объекта.
- `Update(GameTime)` – вызывается каждый кадр (если `Enabled == true`).
- `OnDestroy()` – при уничтожении.
- `GameObject`, `Transform` – ссылки на владельца.

### Transform и система координат

`Transform` задаёт положение, размер, поворот, привязку и иерархию. Все координаты задаются в **эталонных единицах** (1920×1080). При отрисовке они преобразуются в экранные пиксели через `ResolutionManager`.

**Основные свойства:**
- `Position` – смещение от точки привязки (`Anchor`) в эталонных пикселях.
- `Anchor` – нормализованная точка (0..1) на родительском прямоугольнике.
- `Origin` – точка поворота внутри объекта (0..1 относительно его размера).
- `Size` – фиксированный размер (используется при `SizeMode = Fixed`).
- `SizeModeX` / `SizeModeY` – `Fixed` или `Stretch`.
- `StretchLeft`, `StretchRight`, `StretchTop`, `StretchBottom` – отступы при `Stretch`.
- `Parent` / `Children` – иерархия.
- `Rotation` – угол в радианах.

**Важные методы:**
- `GetWorldPosition()` – возвращает мировую позицию верхнего левого угла в эталонных единицах.
- `GetWorldSize()` – возвращает мировой размер в эталонных единицах.
- `GetScreenPosition()`, `GetScreenSize()` – аналоги в пикселях.

**Примечание:** Для корневых объектов (без родителя) `Anchor` вычисляется относительно реального окна (с учётом letterbox), а `Position` масштабируется пропорционально текущему разрешению. Это даёт поведение, аналогичное Unity: элементы с `Anchor = (1,0)` прилипают к правому краю экрана, а отступы в эталонных пикселях сохраняют относительный размер.

### Сцена (Scene)

Статический класс, управляющий всеми `GameObject`. Хранит списки активных объектов, обрабатывает добавление/удаление, вызывает `Update` и `Draw` в правильном порядке. `Draw` сортирует спрайты и текст по `SortingLayer` и `LayerDepth`.

**Основные методы:**
- `Initialize()` – вызвать перед созданием любых объектов.
- `CreateGameObject(...)` – создаёт объект и добавляет в сцену.
- `AddGameObject()`, `RemoveGameObject()`.
- `LoadContent(ContentManager)` – загружает текстуры и шрифты для всех `SpriteRenderer` и `UIText`.
- `Update(GameTime)`, `Draw(SpriteBatch)`.
- `FindComponentsOfType<T>()` – возвращает все компоненты указанного типа.

### Менеджер разрешения (ResolutionManager)

Статический класс, обеспечивающий масштабирование эталонной области 1920×1080 на реальный экран с центрированием. Вычисляет коэффициент `Scale` и смещение `Offset`.

**Поля:**
- `ReferenceWidth` / `ReferenceHeight` – эталонное разрешение (1920×1080).
- `ActualWidth` / `ActualHeight` – текущее разрешение окна.
- `Scale` – множитель масштабирования.
- `Offset` – смещение для центрирования.

**Методы:**
- `Initialize(GraphicsDevice)` – вызывается после создания устройства.
- `ToScreen(Vector2 refPos)` – переводит эталонные координаты в экранные.
- `ToScreenSize(Vector2 refSize)` – переводит эталонный размер в экранный.
- `ToReference(Vector2 screenPos)` – обратное преобразование (например, для мыши).

### Ввод (InputManager)

Статический класс, который **один раз за кадр** считывает состояния клавиатуры и мыши и предоставляет их через методы. Все компоненты должны использовать `InputManager`, а не прямо вызывать `Keyboard.GetState()`.

**Методы:**
- `Update()` – вызывать в начале `Game1.Update()`.
- `FeedTextInput(char c)` – передаёт символ от `Window.TextInput`.
- `TryGetTextInput(out char c)` – извлекает символ из очереди.
- `IsKeyDown`, `IsKeyUp`, `IsKeyPressed`, `IsKeyReleased`.
- `MouseScreenPosition`, `MouseReferencePosition` (в эталонных координатах).
- `IsLeftButtonDown`, `IsLeftButtonPressed`, `IsLeftButtonReleased`.

### Компоненты отрисовки и UI

**`SpriteRenderer`** – отображает текстуру. Свойства: `TexturePath`, `Color`, `SourceRectangle`, `Effects`, `LayerDepth`, `SortingLayer`.

**`UIText`** – отображает текст с помощью `SpriteFont`. Свойства: `FontPath`, `Text`, `Color`, `Scale`, `Alignment` (Left/Center/Right), `VerticalAlignment` (Top/Center/Bottom), `LayerDepth`, `SortingLayer`. Метод `GetTextLocalPositionAtIndex(int index)` возвращает локальную позицию левого края символа – используется для курсора в `UIInputField`.

**`UIButton`** – интерактивная кнопка. События: `OnFocusEnter`, `OnFocusExit`, `OnPointerDown`, `OnPointerUp`, `OnClick`, `OnPressed`, `OnEnable`, `OnDisable`. Кнопка автоматически обрабатывает наведение и клики через `InputManager`.

**`UIInputField`** – текстовое поле с поддержкой русского языка, мигающим курсором, автоповтором. Свойства: `Text`, `MaxLength`, `Placeholder`, `ReadOnly`, `IsFocused`. События: `OnTextChanged(string)`, `OnSubmit(string)`. Полностью полагается на `InputManager.TryGetTextInput()` для ввода символов.

**`Animation`** – компонент для анимации свойств (float, Vector2, Color). Использует `AnimationClip` и `AnimationTrack<T>`. Подробнее см. исходный код `Animation.cs`.

### Анимации

Анимации создаются через `AnimationClip` и `AnimationTrack`. Пример:

```csharp
var anim = gameObject.AddComponent<Animation>();
var clip = new AnimationClip("FadeIn");
var track = clip.AddTrack<float>(
    setter: (value) => sprite.Color = new Color(1,1,1,value),
    getter: () => sprite.Color.A / 255f
);
track.AddKeyframe(new Keyframe<float>(0, 0));
track.AddKeyframe(new Keyframe<float>(1, 1));
anim.AddClip(clip);
anim.Play("FadeIn");
```

---

## Игровые механики

### Сущности: Robot и Enemy

**`Robot`** – игровой юнит. Хранит `Name`, `CurrentHealth`, `MaxHealth`, список `SkillSlots` (доступные действия), словарь `LetterBindings` (буква → действие). Методы: `Heal`, `TakeDamage`, `ActOnLetter`.

**`Enemy`** – противник. Хранит `Name`, `CurrentHealth`, словарь `Actions` (буква → `EnemyAction`). Метод `ActOnLetter` выполняет действие.

**`EnemyAction`** – простое действие врага. Содержит `Damage` и `Description`, метод `Execute(Enemy source, BattleContext context)`.

### Действия (ActionBase и EnemyAction)

Базовый класс `ActionBase`:

```csharp
public abstract class ActionBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public abstract void Execute(BattleContext context);
}
```

`BattleContext` содержит `BattleManager` и `Robot User` (исполнитель).

Пример готового действия – `AttackAction`, `HealAction`. Для создания нового действия достаточно унаследоваться и реализовать `Execute`.

### BattleManager – управление боем

Компонент, прикрепляемый к корневому объекту боя. Выполняет:
- Инициализацию копии роботов из `GameState.Robots` и генерацию врагов через `DifficultyManager.GenerateEnemies()`.
- Подписку на `UIInputField.OnSubmit`.
- Обработку слова: проверка через `WordValidator.IsRealWord`, посимвольное выполнение `ActOnLetter` для всех живых роботов и врагов.
- Логирование событий.
- Проверку победы/поражения. При победе вызывает `VictoryDelayHandler.StartVictoryDelay()` для задержки 2 секунды и перехода в режим настройки протокола.

**Основные события:**
- `OnLogMessage(string)` – для отображения лога.
- `OnWordAccepted(string)` – при успешной отправке слова.
- `OnBattleEnd(bool victory)` – при завершении боя.

**Вспомогательные методы:**
- `LogMessage(string)`
- `GetRandomAliveRobot()`, `GetRandomAliveEnemy()`

### ProtocolSetupManager – настройка протокола

Компонент, управляющий привязкой букв к действиям. Хранит текущего робота (`_currentRobot`), список доступных навыков (`_availableSkills`), лимит назначений за сессию (`MaxBindingsPerSession = 3`). Методы:

- `StartNewSession()` – сбрасывает счётчик привязок.
- `RefreshAvailableSkills()` – генерирует случайный набор из 3 навыков (например, `AttackAction`, `HealAction`).
- `BindAction(char letter, ActionBase action)` – назначает действие на букву (если лимит не превышен).
- `UnbindAction(char letter)` – удаляет привязку.
- `GetSkillPowerForLetter(char letter)` – возвращает множитель силы для буквы (использует `DifficultyManager.GetLetterPowerModifier`).

### Валидация слов (WordValidator)

Статический класс. Загружает словарь из `Content/words.txt` и спецэффекты из `Content/special_words.csv`. Методы:

- `Initialize()` – загружает файлы (если файлы отсутствуют, игра не падает, валидация всегда возвращает `true`).
- `IsRealWord(string word)` – проверяет наличие слова в словаре.
- `GetSpecialEffect(string word)` – возвращает строку-идентификатор эффекта или `null`.

Файл `special_words.csv` имеет формат: `слово,идентификатор`. Обработка эффектов добавляется в `BattleManager.OnWordSubmitted`.

### Глобальное состояние (GameState)

Статический класс для данных, сохраняющихся между боями:

- `Robots` – список роботов (их состояние здоровья, привязки).
- `UsedWords` – хэш-сет использованных слов (для запрета повторов).

Метод `Reset()` очищает состояние для нового забега.

### Статистика (RunStatistics)

Собирает статистику текущего забега: использованные буквы, навыки, побеждённых врагов, время ходов. Все методы статические. Используется `DifficultyManager` для адаптации монстров. Основные методы:

- `RecordLetterUsed(char)`, `GetLetterUsage(char)`, `GetTotalLettersUsed()`.
- `RecordSkillUsed(ActionBase)`, `GetSkillUsage()`.
- `RecordEnemyDefeated(string)`, `GetEnemyDefeats()`.
- `RecordTurnTime(float)`, `GetAverageTurnTime()`.
- `RecordWordUsed(string)`, `TotalWordsUsed`.

### Сложность и адаптация (DifficultyManager, LetterFrequency)

**`DifficultyManager`** – управляет сложностью забега. Генерирует врагов через `GenerateEnemies()`: количество и сила зависят от `CurrentBattleIndex` (индекс боя). При генерации каждому врагу на случайные буквы назначается `EnemyAction` с уроном, растущим от сложности. Также предоставляет `GetLetterPowerModifier(char)`, который возвращает множитель силы действия робота в зависимости от частоты буквы (редкие буквы дают бонус).

**`LetterFrequency`** – статический класс, содержащий частоты букв русского алфавита (в процентах). Используется для расчёта модификатора силы.

### Вспомогательные компоненты

- **`BattleInfoDisplay`** – подписывается на события `BattleManager`, собирает информацию о состоянии боя (здоровье роботов, врагов, лог). Может использоваться для отображения UI.
- **`ProtocolKeyboardHandler`** – компонент, который в режиме настройки протокола обрабатывает нажатия клавиш на физической клавиатуре и вызывает `MySceneBuilder.OnLetterClicked` для соответствующей буквы.
- **`VictoryDelayHandler`** – добавляет паузу в 2 секунды после победы в бою, затем переключает на экран настройки протокола (через `MySceneBuilder.SwitchToProtocolSetup`).

---

## Построение сцен и интерфейс (MySceneBuilder)

Класс `MySceneBuilder` статический, отвечает за создание всех корневых объектов сцен и их переключение. Метод `Build()` вызывается из `Game1.Initialize()`. Ниже описаны основные создаваемые режимы.

### Главное меню

Корневой объект `_mainMenuRoot` с растяжением на весь экран. Содержит заголовок и кнопки «Играть», «О проекте», «Выход». Кнопка «Играть» переключает на режим настройки протокола.

### Режим битвы

Корневой объект `_battleRoot` – растянут на левую часть (справа резервируется 600 пикселей под панель информации). Содержит:
- Поле ввода `UIInputField`.
- Лог битвы `UIText` (обновляется через `BattleManager.OnLogMessage`).
- Заголовок.
- Кнопку меню (пауза) в правом верхнем углу с текстом «Меню».
- `BattleManager` и `BattleInfoDisplay` как компоненты.
- Панель информации (пока не реализована полноценно).

### Настройка протокола

Корневой объект `_protocolSetupRoot`. Содержит:
- Верхняя панель с именем текущего робота.
- Сетка кнопок для всех букв (33 буквы русского алфавита). При клике на букву она выделяется.
- Справа панель со списком доступных навыков (кнопки, динамически создаваемые при каждом открытии). При клике на навык он назначается на выделенную букву (вызов `ProtocolSetupManager.BindAction`).
- Кнопка «Удалить привязку».
- Кнопка «В бой» (переключает в режим битвы).
- Кнопка «Меню» (пауза).

### Меню паузы

Корневой объект `_pauseMenuRoot`, создаётся поверх всех остальных. Содержит:
- Полупрозрачный фон, который динамически растягивается на весь **реальный** экран (с учётом letterbox). Размер фона вычисляется через `ResolutionManager.ToReference(new Vector2(ActualWidth, ActualHeight))` и обновляется при изменении размера окна.
- Центральная панель с кнопками: «Выйти в меню», «Настройки», «Журнал», «Выход из игры». Функциональны только «Выйти в меню» и «Выход из игры».

Меню паузы активируется через `TogglePauseMenu()`, вызываемое по нажатию Escape или кнопке «Меню». В главном меню пауза не работает.

### Переключение режимов

- `SwitchToMainMenu()` – активирует главное меню, деактивирует остальные, скрывает паузу.
- `SwitchToBattle()` – активирует режим битвы, очищает поле ввода.
- `SwitchToProtocolSetup()` – активирует настройку протокола, сбрасывает счётчик привязок, обновляет доступные навыки и UI.

---

## Расширение функциональности

### Добавление нового действия робота

1. Создайте класс, наследующий `ActionBase`. Реализуйте `Execute(BattleContext context)`. Используйте `context.Battle` для доступа к `BattleManager` (например, `GetRandomAliveEnemy()`).
2. Если действие должно выбирать цель, можете добавить параметр в `BattleContext` или реализовать собственный механизм.
3. Чтобы действие появилось в настройке протокола, его нужно добавить в пул навыков. В `ProtocolSetupManager.RefreshAvailableSkills()` вручную добавьте экземпляр вашего действия в список `allPossibleSkills`. Например:

```csharp
var allPossibleSkills = new List<ActionBase>
{
    new AttackAction(),
    new HealAction(),
    new ScanAction(), // ваше новое действие
};
```

Или можно сделать систему, загружающую все действия через рефлексию.

### Добавление нового врага или вражеского действия

Враг создаётся через `new Enemy(name, maxHealth)`. Его действия на буквы – словарь `Dictionary<char, EnemyAction>`. Чтобы изменить генерацию врагов, модифицируйте `DifficultyManager.GenerateEnemies()`. Там можно создавать разные типы врагов (например, с разными именами, HP, и разными наборами действий). Для сложного поведения создайте наследника `EnemyAction` или добавьте дополнительные поля в существующий класс.

### Добавление спецэффектов слов

1. Отредактируйте файл `Content/special_words.csv`, добавив строку: `слово,идентификатор`.
2. В `BattleManager.OnWordSubmitted` после вызова `WordValidator.GetSpecialEffect(word)` обработайте полученный идентификатор. Лучше использовать словарь делегатов или switch:

```csharp
if (specialEffect != null)
{
    switch (specialEffect)
    {
        case "double_damage":
            // установить флаг, что следующий ход наносит двойной урон
            break;
    }
}
```

3. Примените эффект при выполнении действий (например, модифицируйте `AttackAction`).

### Добавление тегов (магазин)

Теги – это временные модификаторы, которые можно купить в фазе настройки. В текущей версии теги не реализованы, но структура заложена в дизайн-документе. Для добавления:
- Создайте класс тега с методами применения к букве/слову.
- Добавьте UI в `MySceneBuilder` (панель магазина).
- Храните активные теги в `GameState` или `RunStatistics`.
- Модифицируйте `BattleManager.ExecuteWord` для применения эффектов тегов (например, глушитель отключает действия врагов на определённую букву).

### Создание нового UI-компонента

Наследуйте от `Behaviour`. В методе `Update` используйте `InputManager` для обработки ввода. Для отрисовки добавьте свой `Draw`-метод и зарегистрируйте его в `Scene` (лучше через `SpriteRenderer` и `UIText`, но можно и напрямую в `Scene.Draw`). Если компонент должен получать уведомления об активации/деактивации, реализуйте `IActivatable`.

---

## Интеграция с MonoGame

Класс `Game1` наследуется от `Core` (из `MonoGameLibrary`). Обязательные действия:

- В конструкторе подписаться на `Window.TextInput` и передавать символы в `InputManager.FeedTextInput`.
- В `Initialize()` вызвать `MySceneBuilder.Build()`, затем `ResolutionManager.Initialize()`.
- В `LoadContent()` вызвать `Scene.LoadContent(Content)`.
- В `Update()` сначала `InputManager.Update()`, затем обработать глобальные клавиши (Escape для паузы, F11 для полноэкранного режима), потом `Scene.Update()`.
- В `Draw()` очистить экран, начать `SpriteBatch`, вызвать `Scene.Draw()`, закончить `SpriteBatch`.
- При изменении размера окна (событие `ClientSizeChanged`) повторно инициализировать `ResolutionManager` и вызвать `MySceneBuilder.OnWindowResized()` (чтобы обновить размер фона паузы).

Пример минимального `Game1.Update()`:

```csharp
protected override void Update(GameTime gameTime)
{
    InputManager.Update();
    if (InputManager.IsKeyPressed(Keys.Escape))
        MySceneBuilder.TogglePauseMenu();
    if (InputManager.IsKeyPressed(Keys.F11))
        ToggleFullScreen();
    Scene.Update(gameTime);
    base.Update(gameTime);
}
```

---

## Правила оформления кода

- Все публичные члены должны иметь XML-комментарии `<summary>`.
- Комментарии поясняют «почему», а не «что».
- Именование: классы и методы – `PascalCase`, приватные поля – `_camelCase`, публичные свойства – `PascalCase`, локальные переменные – `camelCase`.
- Файлы движка – в папке `Engine/`, игровая логика – в `Gameplay/`, построитель сцен – в `Scenes/`.
- Избегайте прямых вызовов `Keyboard.GetState()` и `Mouse.GetState()` – используйте `InputManager`.
- Все позиции и размеры задавайте в эталонных единицах (1920×1080). Доверьте преобразование экранных координат `Transform` и `ResolutionManager`.

--- 

Документация актуальна для версии проекта, описанной в приложенных файлах. При расширении функциональности следуйте описанным паттернам.