# Документация по использованию игрового движка (версия 2.3)

## Оглавление
1. [Обзор системы](#обзор-системы)
2. [Базовые классы движка](#базовые-классы-движка)
   - [Behaviour](#behaviour)
   - [GameObject](#gameobject)
   - [Transform](#transform)
   - [Scene](#scene)
3. [Система координат и масштабирование](#система-координат-и-масштабирование)
   - [ResolutionManager](#resolutionmanager)
4. [Система ввода](#система-ввода)
   - [InputManager](#inputmanager)
5. [Создание сцены](#создание-сцены)
   - [Конфигурирование Transform](#конфигурирование-transform)
   - [Примеры создания объектов](#примеры-создания-объектов)
6. [Иерархия объектов](#иерархия-объектов)
7. [Системные компоненты](#системные-компоненты)
   - [SpriteRenderer](#spriterenderer)
   - [UIText](#uitext)
   - [UIButton](#uibutton)
   - [Animation](#animation)
   - [UIInputField](#uiinputfield)
8. [Расширение функционала](#расширение-функционала)
   - [Создание нового поведения](#создание-нового-поведения)
   - [Создание нового системного компонента](#создание-нового-системного-компонента)
9. [Интеграция с MonoGame](#интеграция-с-monogame)
10. [Советы и лучшие практики](#советы-и-лучшие-практики)
11. [Правила оформления кода](#правила-оформления-кода)

---

## Обзор системы

Данный игровой движок предоставляет архитектуру, подобную Unity, для проектов на MonoGame. Он позволяет декларативно описывать игровые сцены, отделяя этап конструирования объектов от загрузки ресурсов. Основной код игры (`Game1.cs`) остаётся неизменным при добавлении новых объектов или поведений.

**Ключевые концепции:**

- **GameObject** – контейнер для компонентов.
- **Transform** – обязательный компонент, задающий положение, поворот, размер, привязку и иерархию.
- **Behaviour** – базовый класс для всех компонентов, содержит методы `Start()` и `Update()`.
- **Scene** – статический менеджер, управляющий всеми объектами, их обновлением и отрисовкой.
- **ResolutionManager** – утилита для адаптации координат под любое разрешение экрана с сохранением пропорций и центрированием.
- **InputManager** – централизованный менеджер ввода, считывающий состояния клавиатуры и мыши один раз за кадр.

**Новое в версии 2.3:**
- Добавлен статический класс **InputManager**, централизующий обработку ввода. Все компоненты, работающие с клавиатурой и мышью (`UIButton`, `UIInputField`, `Game1`), теперь используют его методы. Ввод считывается единожды за кадр в `Game1.Update()`.
- Методы `UIButton` и `UIInputField` больше не хранят предыдущие состояния устройств ввода — они полностью опираются на `InputManager`.

**Новое в версии 2.2:**
- Компонент **UIText** получил поддержку вертикального выравнивания `VerticalAlignment` и метод `GetTextLocalPositionAtIndex` для позиционирования курсора.
- Компонент **UIButton** теперь корректно обрабатывает интерактивную область с учётом свойства `Origin`.
- Компонент **UIInputField** – полноценное текстовое поле ввода с курсором, навигацией, автоповтором и поддержкой русского языка.
- В **Scene** добавлено статическое свойство `Content`, позволяющее компонентам дозагружать ресурсы после `LoadContent`.
- **ResolutionManager** теперь центрирует эталонную область (1920×1080) на любых экранах, включая сверхширокие, вычисляя масштаб и смещение (`Offset`).
- Безопасная работа с коллекциями в `Scene.Update` и `Scene.Draw` через копирование списков.

---

## Базовые классы движка

### `Behaviour`
Абстрактный класс, от которого наследуются все компоненты.

| Член | Описание |
|------|----------|
| `GameObject` | Ссылка на GameObject, к которому прикреплён компонент. |
| `Transform` | Удобное свойство, эквивалентное `gameObject.Transform`. |
| `Enabled` | Включён ли компонент (если `false`, `Update()` не вызывается). |
| `Start()` | Вызывается один раз перед первым `Update()`. |
| `Update(GameTime gameTime)` | Вызывается каждый кадр. |
| `OnDestroy()` | Вызывается при уничтожении компонента или объекта. |

### `GameObject`
Контейнер компонентов. Всегда содержит `Transform`.

| Член | Описание |
|------|----------|
| `Name` | Имя объекта (для отладки). |
| `Transform` | Ссылка на компонент Transform. |
| `ActiveSelf` | Локальная активность объекта. |
| `ActiveInHierarchy` | Активность с учётом родителей. |
| **Конструктор** | `GameObject(string name = "GameObject", Transform transform = null, params Behaviour[] components)` |
| `AddComponent<T>()` | Создаёт и добавляет новый компонент. |
| `GetComponent<T>()` | Возвращает первый компонент указанного типа. |
| `GetComponents<T>()` | Возвращает все компоненты указанного типа. |
| `Destroy()` | Уничтожает объект и все его компоненты. |

> **Важно:** Для создания объекта следует использовать `Scene.CreateGameObject()`, передавая сконфигурированный `Transform`.

### `Transform`
Компонент, задающий пространственные характеристики и иерархию. Поддерживает два режима размера по каждой оси: фиксированный (`Fixed`) и растянутый (`Stretch`).

| Свойство / Метод | Описание |
|------------------|----------|
| `Position` | Эталонное смещение от точки привязки `Anchor` (в пикселях эталонного разрешения 1920×1080). |
| `Rotation` | Поворот в радианах. |
| `Anchor` | Нормализованная точка (0..1) на родительском прямоугольнике, к которой привязывается объект. (0,0) – левый верхний угол, (1,1) – правый нижний. |
| `Origin` | Нормализованная точка (0..1) внутри самого объекта, которая совмещается с точкой привязки. Используется также как центр поворота. |
| `Size` | Фиксированный размер объекта (используется, когда `SizeModeX` / `SizeModeY` = `Fixed`). |
| `SizeModeX` | Режим размера по оси X: `Fixed` или `Stretch`. |
| `SizeModeY` | Режим размера по оси Y: `Fixed` или `Stretch`. |
| `StretchLeft` | Отступ слева от родительского края (при `SizeModeX = Stretch`). |
| `StretchRight` | Отступ справа от родительского края (при `SizeModeX = Stretch`). |
| `StretchTop` | Отступ сверху от родительского края (при `SizeModeY = Stretch`). |
| `StretchBottom` | Отступ снизу от родительского края (при `SizeModeY = Stretch`). |
| `Parent` | Родительский Transform (или `null`). |
| `Children` | Список дочерних Transform'ов. |
| `SetParent(Transform parent, bool worldPositionStays = true)` | Устанавливает родителя. |
| `GetWorldPosition()` | Возвращает мировую позицию (верхний левый угол) в эталонных единицах. |
| `GetWorldSize()` | Возвращает мировой размер в эталонных единицах с учётом режимов `SizeMode`. |
| `GetScreenPosition()` | Возвращает позицию в пикселях текущего разрешения экрана. |
| `GetScreenSize()` | Возвращает размер в пикселях текущего разрешения экрана. |

### `Scene`
Статический класс-менеджер.

| Метод / Свойство | Описание |
|------------------|----------|
| `Initialize()` | Инициализирует сцену (вызывать перед созданием объектов). |
| `CreateGameObject(...)` | Создаёт GameObject с заданным Transform и добавляет его в сцену. |
| `AddGameObject(go)` | Добавляет существующий GameObject. |
| `RemoveGameObject(go)` | Удаляет GameObject из сцены. |
| `LoadContent(ContentManager)` | Загружает текстуры и шрифты для всех компонентов, сохраняет ContentManager. |
| `Content` | Статическое свойство, хранящее ContentManager для дозагрузки ресурсов в компонентах после инициализации. |
| `Update(GameTime)` | Обновляет все активные объекты и компоненты (потокобезопасная версия). |
| `Draw(SpriteBatch)` | Отрисовывает все SpriteRenderer и UIText с учётом сортировки по слоям. |
| `FindComponentsOfType<T>()` | Возвращает все компоненты заданного типа. |

---

## Система координат и масштабирование

### ResolutionManager
Статический класс, обеспечивающий преобразование координат из **эталонного разрешения 1920×1080** в фактическое разрешение экрана с сохранением пропорций и центрированием.

| Член | Описание |
|------|----------|
| `ReferenceWidth` (1920) | Эталонная ширина. |
| `ReferenceHeight` (1080) | Эталонная высота. |
| `ActualWidth` / `ActualHeight` | Текущие размеры окна/экрана. |
| `Scale` | Масштабный коэффициент (минимум из отношений ширин/высот), чтобы игровая область полностью помещалась. |
| `Offset` | Смещение в пикселях для центрирования игровой области на экране. |
| `Initialize(GraphicsDevice)` | Инициализация (вызывается после создания графического устройства). |
| `ToScreen(Vector2 referencePos)` | Переводит эталонные координаты в экранные пиксели (с учётом Offset). |
| `ToScreenSize(Vector2 referenceSize)` | Переводит эталонный размер в экранные пиксели. |
| `ToReference(Vector2 screenPos)` | Обратное преобразование (например, для мыши). |

**Принцип работы:**  
Все координаты и размеры задаются в эталонных единицах 1920×1080. Масштаб вычисляется так, чтобы игровая область вписалась в экран, а `Offset` центрирует её. Это гарантирует корректное отображение на любых мониторах, включая ультраширокие.

---

## Система ввода

### InputManager

Статический класс, который **однократно за кадр** считывает состояния клавиатуры и мыши и предоставляет удобные методы для проверки нажатий, удержаний и отпусканий. Все компоненты, зависящие от ввода, используют `InputManager` вместо прямого опроса `Keyboard.GetState()` / `Mouse.GetState()`.

**Главное правило:** вызывайте `InputManager.Update()` **строго один раз в начале** метода `Game1.Update()`, до обновления сцены и любой другой логики, работающей с вводом.

| Член | Описание |
|------|----------|
| `Update()` | Считывает текущие состояния клавиатуры и мыши, сохраняет предыдущие. |
| `Initialized` | Был ли менеджер обновлён хотя бы один раз. |

**Клавиатура:**

| Метод | Описание |
|-------|----------|
| `IsKeyDown(Keys key)` | Клавиша зажата в текущем кадре. |
| `IsKeyUp(Keys key)` | Клавиша отпущена в текущем кадре. |
| `IsKeyPressed(Keys key)` | Клавиша была нажата в этом кадре (переход из Up в Down). |
| `IsKeyReleased(Keys key)` | Клавиша была отпущена в этом кадре (переход из Down в Up). |

**Мышь:**

| Член | Описание |
|------|----------|
| `MouseScreenPosition` | Позиция мыши в экранных координатах. |
| `MouseReferencePosition` | Позиция мыши в эталонных координатах игры (с учётом масштабирования). |
| `IsLeftButtonDown` | Левая кнопка мыши зажата в текущем кадре. |
| `IsLeftButtonPressed` | Левая кнопка была нажата в этом кадре. |
| `IsLeftButtonReleased` | Левая кнопка была отпущена в этом кадре. |

**Пример использования в пользовательском компоненте:**

```csharp
public override void Update(GameTime gameTime)
{
    // Проверка однократного нажатия пробела
    if (InputManager.IsKeyPressed(Keys.Space))
    {
        Jump();
    }

    // Позиция мыши в эталонных координатах
    Vector2 mouseRef = InputManager.MouseReferencePosition;
    // ...
}
```

---

## Создание сцены

Сцена описывается в отдельном классе-строителе, например `MySceneBuilder`:

```csharp
public static class MySceneBuilder
{
    public static void Build()
    {
        Scene.Initialize();

        // Создание объектов...
    }
}
```

Вызов `Build()` должен происходить в `Game1.Initialize()`.

### Конфигурирование Transform

Перед созданием объекта необходимо создать экземпляр `Transform` и настроить его свойства. Затем передать в `Scene.CreateGameObject()`.

```csharp
var myTransform = new Transform
{
    Anchor = new Vector2(0.5f, 0.5f), // центр родителя (экрана)
    Position = new Vector2(0, -50),   // смещение вверх на 50 эталонных пикселей
    Size = new Vector2(200, 100),     // фиксированный размер 200×100
    Origin = new Vector2(0.5f, 0.5f)  // центр объекта
};
var obj = Scene.CreateGameObject("MyObject", myTransform, 
    new SpriteRenderer("Sprites/MySprite"));
```

### Примеры создания объектов

**1. Объект в центре экрана с фиксированным размером**

```csharp
var centerTransform = new Transform
{
    Anchor = new Vector2(0.5f, 0.5f),
    Position = Vector2.Zero,
    Size = new Vector2(300, 300),
    Origin = new Vector2(0.5f, 0.5f)
};
var centeredObj = Scene.CreateGameObject("Centered", centerTransform);
```

**2. Кнопка в правом нижнем углу с отступом 20 пикселей от краёв**

```csharp
var buttonTransform = new Transform
{
    Anchor = new Vector2(1f, 1f),       // правый нижний угол экрана
    Position = new Vector2(-20, -20),   // смещение влево и вверх
    Size = new Vector2(150, 50),
    Origin = new Vector2(1f, 1f)        // правый нижний угол объекта
};
var button = Scene.CreateGameObject("Button", buttonTransform, 
    new SpriteRenderer("Sprites/Button"));
```

**3. Панель, растянутая по ширине с отступами и фиксированной высотой**

```csharp
var panelTransform = new Transform
{
    Anchor = new Vector2(0.5f, 0f),      // центр верхней границы экрана
    Position = new Vector2(0, 30),       // отступ сверху 30
    SizeModeX = SizeMode.Stretch,
    StretchLeft = 50,
    StretchRight = 50,
    SizeModeY = SizeMode.Fixed,
    Size = new Vector2(0, 100),          // ширина игнорируется, высота 100
    Origin = new Vector2(0.5f, 0f)
};
var panel = Scene.CreateGameObject("TopPanel", panelTransform,
    new SpriteRenderer("Sprites/Panel"));
```

**4. Поле ввода текста (с автоматическим курсором)**

```csharp
var inputTransform = new Transform
{
    Anchor = new Vector2(0.5f, 0.5f),
    Position = new Vector2(0, 200),
    Size = new Vector2(600, 60),
    Origin = new Vector2(0.5f, 0.5f)
};

var inputObj = Scene.CreateGameObject("WordInput", inputTransform,
    new SpriteRenderer("Sprites/Panel") { Color = new Color(40, 40, 40) },
    new UIText("Fonts/MyFont", "", Color.White, 1.8f,
        TextAlignment.Left, VerticalAlignment.Center),
    new UIInputField
    {
        Placeholder = "Введите слово...",
        MaxLength = 20
    }
);

// Подписка на события
var inputField = inputObj.GetComponent<UIInputField>();
inputField.OnTextChanged += (text) => Console.WriteLine($"Текст: {text}");
inputField.OnSubmit += (text) => Console.WriteLine($"Отправлено: {text}");
```

---

## Иерархия объектов

Каждый `Transform` может иметь родителя и дочерние элементы. При установке родителя через `SetParent()` позиция и размер дочернего объекта начинают рассчитываться относительно родительского прямоугольника.

**Особенности:**
- Свойство `Anchor` дочернего объекта указывает точку **на родительском прямоугольнике**.
- Отступы `StretchLeft`, `StretchRight`, `StretchTop`, `StretchBottom` отсчитываются от границ родителя.
- Мировые координаты вычисляются рекурсивно с учётом всей цепочки родителей.

**Пример создания иерархии:**

```csharp
var parent = Scene.CreateGameObject("Parent", parentTransform);
var child = Scene.CreateGameObject("Child", childTransform);
child.Transform.SetParent(parent.Transform);
```

> **Примечание:** При изменении размеров родителя (например, из-за растяжения по экрану) все дочерние элементы, использующие режим `Stretch`, автоматически пересчитывают свои размеры.

---

## Системные компоненты

### SpriteRenderer

Отвечает за отрисовку спрайта.

| Свойство | Описание |
|----------|----------|
| `TexturePath` | Путь к текстуре относительно `Content` (без расширения). |
| `Texture` | Загруженная текстура (заполняется автоматически). |
| `Color` | Цветовой оттенок (по умолчанию `White`). |
| `SourceRectangle` | Область текстуры для отрисовки (`null` – вся). |
| `Effects` | `SpriteEffects` (отражение). |
| `LayerDepth` | Глубина для сортировки (0.0 – передний план, 1.0 – задний). |
| `SortingLayer` | Целочисленный слой отрисовки. Чем больше число, тем позже рисуется объект (поверх других). По умолчанию 0. |

### UIText

Компонент для отрисовки текста с использованием `SpriteFont`. Поддерживает горизонтальное и вертикальное выравнивание.

| Свойство | Описание |
|----------|----------|
| `FontPath` | Путь к файлу шрифта (`.spritefont`) относительно `Content`. |
| `Font` | Загруженный шрифт (заполняется автоматически). |
| `Text` | Отображаемая строка. |
| `Color` | Цвет текста. |
| `Alignment` | Горизонтальное выравнивание: `Left`, `Center`, `Right`. |
| `VerticalAlignment` | Вертикальное выравнивание: `Top`, `Center`, `Bottom`. |
| `Scale` | Масштаб текста (умножается на размер шрифта). |
| `LayerDepth` | Глубина слоя. |
| `SortingLayer` | Слой отрисовки (целое число). |

**Методы:**
- `GetTextLocalPositionAtIndex(int index)` – возвращает локальную позицию левого края символа по индексу (0 .. Text.Length). Используется для позиционирования курсора ввода.

**Конструкторы:**

```csharp
public UIText() { }
public UIText(string fontPath, string text = "", Color? color = null, float scale = 1f,
              TextAlignment alignment = TextAlignment.Left,
              VerticalAlignment verticalAlignment = VerticalAlignment.Center)
```

### UIButton

Компонент для создания интерактивных кнопок, реагирующих на мышь. **Для работы использует `InputManager`** — необходимо вызывать `InputManager.Update()` каждый кадр. Интерактивная область корректно рассчитывается с учётом `Origin`.

| Свойство | Описание |
|----------|----------|
| `Interactable` | Включена ли кнопка (если `false`, события не вызываются). |

**События:**

| Событие | Описание |
|---------|----------|
| `OnFocusEnter` | Курсор вошёл в границы кнопки. |
| `OnFocusExit` | Курсор покинул границы кнопки. |
| `OnPointerDown` | Нажата левая кнопка мыши над кнопкой. |
| `OnPointerUp` | Отпущена левая кнопка мыши над кнопкой (после нажатия). |
| `OnClick` | Успешный клик (нажатие и отпускание над кнопкой). |
| `OnPressed` | Вызывается каждый кадр, пока кнопка нажата и курсор над ней. |

**Пример:**

```csharp
var button = Scene.CreateGameObject("MyButton", btnTransform,
    new SpriteRenderer("Sprites/button"),
    new UIButton(
        onClick: () => Console.WriteLine("Clicked!"),
        onFocusEnter: () => btnGo.GetComponent<SpriteRenderer>().Color = Color.Yellow,
        onFocusExit: () => btnGo.GetComponent<SpriteRenderer>().Color = Color.White
    )
);
```

### Animation

Компонент для анимации значений произвольных типов (`float`, `Vector2`, `Color` и т.д.) с поддержкой клипов и дорожек. Описание осталось без изменений по сравнению с версией 2.1.

### UIInputField

Компонент текстового поля ввода с мигающим курсором, навигацией, автоповтором и поддержкой русской раскладки. **Полностью полагается на `InputManager`** (не хранит собственные предыдущие состояния устройств). Курсор создаётся автоматически при вызове `Start()`; текстура дозагружается через `Scene.Content`.

| Свойство | Описание |
|----------|----------|
| `Text` | Текущий текст в поле. Изменение обновляет отображение и вызывает `OnTextChanged`. |
| `MaxLength` | Максимальная длина строки (по умолчанию 20). |
| `Placeholder` | Текст-подсказка, когда поле пустое и не в фокусе. |
| `ReadOnly` | Если `true`, ввод запрещён. |
| `IsFocused` | Находится ли поле в фокусе ввода (управляется кликом мыши). |

**События:**
- `OnTextChanged(string newText)` – вызывается при любом изменении текста.
- `OnSubmit(string text)` – вызывается при нажатии Enter (после чего поле автоматически очищается).

**Методы:**
- `Clear()` – очищает поле и сбрасывает курсор.

**Особенности:**
- Маппинг клавиш основан на стандартной русской раскладке ЙЦУКЕН.
- Навигация стрелками, Home/End.
- Удержание Backspace и Delete удаляет символы непрерывно с автоповтором.
- Курсор создаётся как дочерний `GameObject` с собственным `SpriteRenderer`. Для его работы необходимо, чтобы текстура `Sprites/Panel` присутствовала в контенте.
- Позиция курсора вычисляется методом `UIText.GetTextLocalPositionAtIndex()`, поэтому компонент должен находиться на том же объекте, что и `UIText`.
- Для корректной работы необходимо вызывать `InputManager.Update()` в `Game1.Update()` перед обновлением сцены.

**Пример создания см. в разделе «Примеры создания объектов».**

---

## Расширение функционала

### Создание нового поведения

1. Создайте класс в папке `Gameplay/Behaviours/`.
2. Унаследуйте его от `MyGameEngine.Behaviour`.
3. Переопределите `Start()` и/или `Update()`.
4. Для обработки ввода используйте методы `InputManager`.

```csharp
using Microsoft.Xna.Framework;
using MyGameEngine;

public class Health : Behaviour
{
    public int MaxHealth { get; set; } = 100;
    public int CurrentHealth { get; private set; }

    public override void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
            gameObject.Destroy();
    }

    public override void Update(GameTime gameTime)
    {
        // Пример: лечение по нажатию H
        if (InputManager.IsKeyPressed(Keys.H))
        {
            CurrentHealth = Math.Min(CurrentHealth + 10, MaxHealth);
        }
    }
}
```

### Создание нового системного компонента

При необходимости дозагрузки ресурсов используйте `Scene.Content.Load<T>()` в методах `Start()` или `Update()`. Если компонент обрабатывает ввод, используйте `InputManager` вместо прямых вызовов `Keyboard.GetState()` / `Mouse.GetState()`.

---

## Интеграция с MonoGame

`Game1.cs` должен выглядеть так (с вызовом `InputManager.Update()`):

```csharp
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public Game1() { /* ... */ }

    protected override void Initialize()
    {
        MySceneBuilder.Build();
        base.Initialize();
        ResolutionManager.Initialize(GraphicsDevice);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Scene.LoadContent(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        // Считываем ввод один раз за кадр
        InputManager.Update();

        // Обработка ввода уровня игры (если нужна)
        if (InputManager.IsKeyPressed(Keys.Escape))
            Exit();

        Scene.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        Scene.Draw(_spriteBatch);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
```

---

## Советы и лучшие практики

1. **Всегда вызывайте `InputManager.Update()`** в начале `Game1.Update()` перед обновлением сцены.
2. **Всегда вызывайте `ResolutionManager.Initialize()` после создания графического устройства.**
3. **Используйте эталонные единицы (1920×1080)** для координат и размеров.
4. **Для адаптивных интерфейсов применяйте режим `Stretch`** с отступами.
5. **При создании иерархии** сначала создавайте родителя, затем дочерний объект и устанавливайте `SetParent()`.
6. **Не модифицируйте коллекции объектов во время итерации** – Scene автоматически обезопасит Update и Draw, но в собственных циклах избегайте удаления элементов из `_gameObjects`.
7. **Кэшируйте ссылки на компоненты** в `Start()`, чтобы не вызывать `GetComponent<T>()` каждый кадр.
8. **Используйте `ActiveSelf`** для временного отключения объектов вместо уничтожения.
9. **При работе с анимациями** создавайте клипы один раз в `Start()` или при построении сцены.
10. **Для кастомных шрифтов** убедитесь, что `.spritefont` включает кириллические символы и при необходимости отключите фильтрацию текстур выбрав `Point` в настройках Pipeline Tool.
11. **Для дозагрузки ресурсов** используйте `Scene.Content.Load<T>()` – это безопасно в любое время после `LoadContent`.
12. **Если курсор в UIInputField не отображается** убедитесь, что текстура `Sprites/Panel` присутствует в контенте и что сам объект поля ввода добавлен в сцену до `LoadContent` (или дозагружает текстуру, как это делает `UIInputField`).
13. **В обработчиках ввода** всегда предпочитайте методы `InputManager` (например, `IsKeyPressed`, `IsLeftButtonReleased`) вместо ручного хранения предыдущих состояний.

---

## Правила оформления кода

*(данный раздел остаётся без изменений из версии 2.1)*

- **Все публичные члены классов должны иметь XML-комментарии `<summary>`**.
- Комментарии должны отвечать на вопрос «почему?», а не «что?».
- **Именование:** классы и методы – `PascalCase`, приватные поля – `_camelCase`, свойства – `PascalCase`, локальные переменные – `camelCase`.
- **Организация файлов:** движок в `Engine/`, поведения в `Gameplay/Behaviours/`, строители сцен в `Scenes/`.
- **Структура класса:** поля → свойства → конструкторы → публичные методы → приватные методы.