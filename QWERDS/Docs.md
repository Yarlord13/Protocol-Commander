# Документация по использованию игрового движка (версия 2.1)

## Оглавление
1. [Обзор системы](#обзор-системы)
2. [Базовые классы движка](#базовые-классы-движка)
   - [Behaviour](#behaviour)
   - [GameObject](#gameobject)
   - [Transform](#transform)
   - [Scene](#scene)
3. [Система координат и масштабирование](#система-координат-и-масштабирование)
   - [ResolutionManager](#resolutionmanager)
4. [Создание сцены](#создание-сцены)
   - [Конфигурирование Transform](#конфигурирование-transform)
   - [Примеры создания объектов](#примеры-создания-объектов)
5. [Иерархия объектов](#иерархия-объектов)
6. [Системные компоненты](#системные-компоненты)
   - [SpriteRenderer](#spriterenderer)
   - [UIText](#uitext)
   - [UIButton](#uibutton)
   - [Animation](#animation)
7. [Расширение функционала](#расширение-функционала)
   - [Создание нового поведения](#создание-нового-поведения)
   - [Создание нового системного компонента](#создание-нового-системного-компонента)
8. [Интеграция с MonoGame](#интеграция-с-monogame)
9. [Советы и лучшие практики](#советы-и-лучшие-практики)
10. [Правила оформления кода](#правила-оформления-кода)

---

## Обзор системы

Данный игровой движок предоставляет архитектуру, подобную Unity, для проектов на MonoGame. Он позволяет декларативно описывать игровые сцены, отделяя этап конструирования объектов от загрузки ресурсов. Основной код игры (`Game1.cs`) остаётся неизменным при добавлении новых объектов или поведений.

**Ключевые концепции:**

- **GameObject** – контейнер для компонентов.
- **Transform** – обязательный компонент, задающий положение, поворот, размер, привязку и иерархию.
- **Behaviour** – базовый класс для всех компонентов, содержит методы `Start()` и `Update()`.
- **Scene** – статический менеджер, управляющий всеми объектами, их обновлением и отрисовкой.
- **ResolutionManager** – утилита для адаптации координат под любое разрешение экрана с сохранением пропорций.

**Новое в версии 2.1:**
- Компонент **UIText** для отрисовки текста с поддержкой выравнивания и масштабирования.
- Компонент **UIButton** для создания интерактивных кнопок с событиями мыши.
- Компонент **Animation** для анимации свойств объектов с поддержкой именованных клипов и нескольких анимируемых полей.
- В **SpriteRenderer** добавлено поле `SortingLayer` (целое число) для управления порядком отрисовки совместно с `LayerDepth`.

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

| Метод | Описание |
|-------|----------|
| `Initialize()` | Инициализирует сцену (вызывать перед созданием объектов). |
| `CreateGameObject(string name, Transform transform, params Behaviour[] components)` | Создаёт GameObject с заданным Transform и добавляет его в сцену. |
| `AddGameObject(go)` | Добавляет существующий GameObject. |
| `RemoveGameObject(go)` | Удаляет GameObject из сцены. |
| `LoadContent(ContentManager)` | Загружает текстуры для всех SpriteRenderer и шрифты для UIText. |
| `Update(GameTime)` | Обновляет все активные объекты и компоненты. |
| `Draw(SpriteBatch)` | Отрисовывает все SpriteRenderer и UIText с учётом сортировки по слоям. |
| `FindComponentsOfType<T>()` | Возвращает все компоненты заданного типа. |

---

## Система координат и масштабирование

### ResolutionManager
Статический класс, обеспечивающий преобразование координат из **эталонного разрешения 1920×1080** в фактическое разрешение экрана с сохранением пропорций.

| Член | Описание |
|------|----------|
| `ReferenceWidth` (1920) | Эталонная ширина. |
| `ReferenceHeight` (1080) | Эталонная высота. |
| `ActualWidth` / `ActualHeight` | Текущие размеры окна/экрана. |
| `Scale` | Коэффициент масштабирования (`ActualHeight / ReferenceHeight`). |
| `Initialize(GraphicsDevice)` | Инициализация (вызывается после создания графического устройства). |
| `ToScreen(Vector2 referencePos)` | Переводит эталонные координаты в экранные пиксели. |
| `ToScreenSize(Vector2 referenceSize)` | Переводит эталонный размер в экранные пиксели. |
| `ToReference(Vector2 screenPos)` | Обратное преобразование (например, для мыши). |

**Принцип работы:**  
Все координаты и размеры в `Transform` задаются в **эталонных единицах** (как если бы экран всегда был 1920×1080). При отрисовке `ResolutionManager` масштабирует их пропорционально высоте экрана. Это гарантирует, что интерфейс и объекты сохранят свои относительные размеры и позиции на любом мониторе.

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

**4. Дочерний элемент, растянутый по высоте родителя с отступами**

```csharp
var childTransform = new Transform
{
    Anchor = new Vector2(0f, 0f),        // левый верхний угол родителя
    Position = new Vector2(20, 20),
    SizeModeX = SizeMode.Fixed,
    Size = new Vector2(120, 0),
    SizeModeY = SizeMode.Stretch,
    StretchTop = 20,
    StretchBottom = 20,
    Origin = Vector2.Zero
};
childTransform.SetParent(panel.Transform);
var child = Scene.CreateGameObject("PanelChild", childTransform,
    new SpriteRenderer("Sprites/ChildElement"));
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

Отвечает за отрисовку спрайта. Использует `Transform.GetScreenPosition()` и `Transform.GetScreenSize()` для получения финальных координат на экране.

| Свойство | Описание |
|----------|----------|
| `TexturePath` | Путь к текстуре относительно `Content` (без расширения). |
| `Texture` | Загруженная текстура (заполняется автоматически). |
| `Color` | Цветовой оттенок (по умолчанию `White`). |
| `SourceRectangle` | Область текстуры для отрисовки (`null` – вся). |
| `Effects` | `SpriteEffects` (отражение). |
| `LayerDepth` | Глубина для сортировки (0.0 – передний план, 1.0 – задний). |
| `SortingLayer` | **Новое:** целочисленный слой отрисовки. Чем больше число, тем позже рисуется объект (поверх других). По умолчанию 0. |

**Пример:**

```csharp
var renderer = new SpriteRenderer("enemy")
{
    Color = Color.Red,
    Effects = SpriteEffects.FlipHorizontally,
    SortingLayer = 10,   // будет отрисован поверх объектов с меньшим слоем
    LayerDepth = 0.2f
};
```

> **Сортировка:** При отрисовке все компоненты `SpriteRenderer` и `UIText` сортируются сначала по `SortingLayer` (по возрастанию), затем по `LayerDepth` (по возрастанию). Это позволяет гибко управлять порядком отображения.

---

### UIText

Компонент для отрисовки текста с использованием `SpriteFont`. Аналогичен `SpriteRenderer`, но для текста.

| Свойство | Описание |
|----------|----------|
| `FontPath` | Путь к файлу шрифта (`.spritefont`) относительно `Content`. |
| `Font` | Загруженный шрифт (заполняется автоматически). |
| `Text` | Отображаемая строка. |
| `Color` | Цвет текста. |
| `Alignment` | Выравнивание текста внутри прямоугольной области (`Left`, `Center`, `Right`). |
| `Scale` | Масштаб текста (умножается на размер шрифта). |
| `LayerDepth` | Глубина слоя (аналогично `SpriteRenderer`). |
| `SortingLayer` | Слой отрисовки (целое число). |

**Конструкторы:**

```csharp
public UIText() { }
public UIText(string fontPath, string text = "", Color? color = null, float scale = 1f, TextAlignment alignment = TextAlignment.Left)
```

**Пример:**

```csharp
var textObj = Scene.CreateGameObject("ScoreLabel", textTransform,
    new UIText("Fonts/Arial", "Score: 0", Color.Gold, 2.0f, TextAlignment.Center)
);
```

---

### UIButton

Компонент для создания интерактивных кнопок, реагирующих на мышь. Отслеживает попадание курсора в границы объекта и состояние левой кнопки мыши.

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
| `OnEnable` | Вызывается при установке `Interactable = true`. |
| `OnDisable` | Вызывается при установке `Interactable = false`. |

**Конструкторы:**

```csharp
public UIButton() { }
public UIButton(
    Action onClick = null,
    Action onFocusEnter = null,
    Action onFocusExit = null,
    Action onPointerDown = null,
    Action onPointerUp = null,
    bool interactable = true)
```

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

> **Важно:** Координаты мыши автоматически преобразуются в эталонные с помощью `ResolutionManager.ToReference`, поэтому попадания корректно работают при любом разрешении экрана.

---

### Animation

Компонент для анимации значений произвольных типов (`float`, `Vector2`, `Color` и т.д.). Позволяет создавать именованные анимационные клипы, каждый из которых может содержать несколько дорожек, изменяющих различные свойства объектов.

**Основные классы:**

- **`AnimationClip`** – именованный набор анимационных дорожек с общей длительностью и флагом зацикливания.
- **`AnimationTrack<T>`** – дорожка, связывающая конкретное свойство объекта (через делегаты `setter`/`getter`) с ключевыми кадрами типа `T`.
- **`Keyframe<T>`** – пара (время, значение).

**Класс `Animation` (компонент):**

| Член | Описание |
|------|----------|
| `State` | Текущее состояние (`Stopped`, `Playing`, `Paused`). |
| `AddClip(AnimationClip clip)` | Добавляет именованный клип. |
| `RemoveClip(string name)` | Удаляет клип по имени. |
| `HasClip(string name)` | Проверяет наличие клипа. |
| `Play(string clipName)` | Запускает воспроизведение клипа с начала. |
| `Pause()` | Приостанавливает анимацию. |
| `Resume()` | Возобновляет после паузы. |
| `Stop()` | Останавливает и сбрасывает текущий клип. |
| `OnFinished` | Событие, вызываемое при завершении клипа (если не зациклен). |

**Создание анимационного клипа:**

```csharp
var clip = new AnimationClip("Move", loop: true);

// Добавляем дорожку, изменяющую позицию объекта
var posTrack = clip.AddTrack<Vector2>(
    setter: v => myObject.Transform.Position = v,
    getter: () => myObject.Transform.Position
);
posTrack.SetKeyframes(new[] {
    new Keyframe<Vector2>(0f, new Vector2(0, 0)),
    new Keyframe<Vector2>(1f, new Vector2(500, 0)),
    new Keyframe<Vector2>(2f, new Vector2(0, 0))
});

// Можно добавить вторую дорожку для изменения цвета спрайта
var colorTrack = clip.AddTrack<Color>(
    setter: c => myObject.GetComponent<SpriteRenderer>().Color = c,
    getter: () => myObject.GetComponent<SpriteRenderer>().Color
);
colorTrack.SetKeyframes(new[] {
    new Keyframe<Color>(0f, Color.White),
    new Keyframe<Color>(1f, Color.Red),
    new Keyframe<Color>(2f, Color.White)
});

// Добавляем клип в компонент анимации
var anim = myObject.AddComponent<Animation>();
anim.AddClip(clip);

// Запускаем
anim.Play("Move");
```

**Поддерживаемые типы для интерполяции:**
`float`, `Vector2`, `Vector3`, `Color`, `int` (линейная интерполяция с округлением). Для других типов потребуется расширение класса.

> **Примечание:** Анимация автоматически вызывает `Sample()` в методе `Update()`, поэтому явно применять значения не нужно.

---

## Расширение функционала

### Создание нового поведения

1. Создайте класс в папке `Gameplay/Behaviours/`.
2. Унаследуйте его от `MyGameEngine.Behaviour`.
3. Переопределите `Start()` и/или `Update()`.

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
}
```

4. Используйте его при создании объекта:

```csharp
var enemy = Scene.CreateGameObject("Enemy", enemyTransform,
    new SpriteRenderer("enemy"),
    new Health { MaxHealth = 150 }
);
```

### Создание нового системного компонента

Если вам нужен компонент, встроенный в движок (например, `Collider`, `AudioSource`), создайте его в папке `Engine/` и унаследуйте от `Behaviour`. Для загрузки ресурсов добавьте логику в `Scene.LoadContent`.

**Пример AudioSource:**

```csharp
// Engine/Audio/AudioSource.cs
using Microsoft.Xna.Framework.Audio;
using MyGameEngine;

public class AudioSource : Behaviour
{
    public string SoundPath { get; set; }
    public SoundEffect Sound { get; internal set; }

    public AudioSource(string soundPath)
    {
        SoundPath = soundPath;
    }

    public void Play()
    {
        Sound?.Play();
    }
}
```

---

## Интеграция с MonoGame

`Game1.cs` должен выглядеть так:

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

1. **Всегда вызывайте `ResolutionManager.Initialize()` после создания графического устройства**, иначе методы `ToScreen` выбросят исключение.
2. **Используйте эталонные единицы (1920×1080) для всех координат и размеров** – это обеспечит корректное отображение на любых экранах.
3. **Для объектов интерфейса активно применяйте режим `Stretch`**, чтобы они адаптировались под разные соотношения сторон.
4. **При создании иерархии сначала создайте родительский объект, затем дочерние и установите `SetParent()`.** Это упрощает настройку.
5. **Не изменяйте `Transform` после добавления объекта в сцену без необходимости** – изменения позиции/размера во время игры допустимы, но следует учитывать, что `Start()` уже был вызван.
6. **Для производительности избегайте частого вызова `GetComponent<T>()` в `Update()`** – кэшируйте ссылки в `Start()`.
7. **Используйте `ActiveSelf` для временного отключения объектов** вместо их уничтожения.
8. **При работе с анимациями** создавайте клипы один раз в `Start()` или при построении сцены, чтобы избежать лишних аллокаций.
9. **Для UIButton** подписывайтесь на события в `Start()` или через конструктор для читаемости.

---

## Правила оформления кода

Для поддержания единообразия и читаемости кода придерживайтесь следующих правил:

### Комментарии
- **Все публичные члены классов должны иметь XML-комментарии `<summary>`**, описывающие назначение.
- Внутри методов добавляйте поясняющие комментарии для сложных участков.
- Используйте `//` для однострочных комментариев и `/* */` для многострочных.
- Комментарии должны отвечать на вопрос «почему?», а не «что?» (код сам говорит «что»).

### Именование
- **Классы и методы:** `PascalCase` (например, `GameObject`, `GetWorldPosition()`).
- **Поля (private):** `_camelCase` с подчёркиванием (например, `_sizeModeX`).
- **Свойства (public):** `PascalCase` (например, `SizeModeX`).
- **Локальные переменные:** `camelCase` (например, `screenPos`).
- **Константы:** `PascalCase` (например, `ReferenceWidth`).

### Организация файлов
- Классы движка располагаются в папке `Engine/` с соответствующей структурой подпапок (`Engine/Core`, `Engine/Components`, `Engine/Graphics` и т.д.).
- Игровые поведения – в `Gameplay/Behaviours/`.
- Строители сцен – в корне проекта или в `Scenes/`.
- Ресурсы (спрайты, звуки) добавляются через `Content.mgcb`.

### Структура класса
1. Поля (private).
2. Свойства (public).
3. Конструкторы.
4. Публичные методы.
5. Приватные методы.

### Пример оформления

```csharp
/// <summary>
/// Компонент, задающий пространственные свойства объекта.
/// Поддерживает иерархию и гибкие режимы размера.
/// </summary>
public class Transform : Behaviour
{
    // Приватные поля
    private Vector2 _position;
    private float _rotation;
    private Vector2 _size;
    // ...

    // Публичные свойства
    /// <summary>
    /// Эталонное смещение от точки привязки Anchor.
    /// </summary>
    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }
    // ...
}
```

Следуя этим правилам, код останется понятным как для человека, так и для нейросетей, что упростит дальнейшую разработку и поддержку проекта.