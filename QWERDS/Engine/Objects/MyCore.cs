#region Assembly MonoGameLibrary, Version=1.0.26.0, Culture=neutral, PublicKeyToken=null
// C:\Users\Kirill\.nuget\packages\monogamelibrary\1.0.26\lib\net9.0\MonoGameLibrary.dll
// Decompiled with ICSharpCode.Decompiler 9.1.0.7988
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Graphics.Camera;
using MonoGameLibrary.Graphics.Collision;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Utilities;
using MonoGameLibrary;

//namespace MyGameEngine;

public class MyCore : Game
{
    internal static MyCore s_instance;

    private static SceneManager s_sceneManager = new SceneManager();

    private static FpsDisplayPosition s_fpsDisplayPosition = FpsDisplayPosition.UpperLeft;

    private static double[] s_frameTimeBuffer = new double[5];

    private static int s_frameTimeIndex = 0;

    private static double s_currentFps = 60.0;

    private static bool s_frameBufferFilled = false;

    public static MyCore Instance => s_instance;

    public static SceneManager SceneManager => s_sceneManager;

    public static GraphicsDeviceManager Graphics { get; private set; }

    public new static GraphicsDevice GraphicsDevice { get; private set; }

    public static SpriteBatch SpriteBatch { get; private set; }

    public static Scene Scene => s_sceneManager.CurrentScene;

    public static Point VirtualResolution { get; set; } = new Point(1920, 1080);

    public static Vector2 ContentScale
    {
        get
        {
            if (Graphics == null)
            {
                return Vector2.One;
            }

            float val = (float)Graphics.PreferredBackBufferWidth / (float)VirtualResolution.X;
            float val2 = (float)Graphics.PreferredBackBufferHeight / (float)VirtualResolution.Y;
            return new Vector2(Math.Min(val, val2));
        }
    }

    public static Matrix ScaleMatrix
    {
        get
        {
            Vector2 contentScale = ContentScale;
            float num = (float)VirtualResolution.X * contentScale.X;
            float num2 = (float)VirtualResolution.Y * contentScale.Y;
            float xPosition = ((float)Graphics.PreferredBackBufferWidth - num) / 2f;
            float yPosition = ((float)Graphics.PreferredBackBufferHeight - num2) / 2f;
            return Matrix.CreateScale(contentScale.X, contentScale.Y, 1f) * Matrix.CreateTranslation(xPosition, yPosition, 0f);
        }
    }

    public static Matrix CameraMatrix
    {
        get
        {
            if (Camera == null)
            {
                return ScaleMatrix;
            }

            return Camera.ViewMatrix * ScaleMatrix;
        }
    }

    public static Rectangle ScaledViewport
    {
        get
        {
            Vector2 contentScale = ContentScale;
            float num = (float)VirtualResolution.X * contentScale.X;
            float num2 = (float)VirtualResolution.Y * contentScale.Y;
            float num3 = ((float)Graphics.PreferredBackBufferWidth - num) / 2f;
            float num4 = ((float)Graphics.PreferredBackBufferHeight - num2) / 2f;
            return new Rectangle((int)num3, (int)num4, (int)num, (int)num2);
        }
    }

    public static bool IsSteamDeck
    {
        get
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string? environmentVariable = Environment.GetEnvironmentVariable("SteamDeck");
                    string environmentVariable2 = Environment.GetEnvironmentVariable("SteamAppId");
                    string environmentVariable3 = Environment.GetEnvironmentVariable("SteamGameId");
                    if (!string.IsNullOrEmpty(environmentVariable) || !string.IsNullOrEmpty(environmentVariable2) || !string.IsNullOrEmpty(environmentVariable3))
                    {
                        return true;
                    }

                    if (File.Exists("/sys/devices/virtual/dmi/id/product_name"))
                    {
                        string text = File.ReadAllText("/sys/devices/virtual/dmi/id/product_name").Trim();
                        if (text.Contains("Jupiter", StringComparison.OrdinalIgnoreCase) || text.Contains("Steam Deck", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    if (File.Exists("/proc/cpuinfo"))
                    {
                        string text2 = File.ReadAllText("/proc/cpuinfo");
                        if (text2.Contains("AuthenticAMD", StringComparison.OrdinalIgnoreCase) && text2.Contains("AMD Custom APU", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public new static ContentManager Content { get; private set; }

    public static InputManager Input { get; private set; }

    public static bool ExitOnEscape { get; set; }

    public static AudioController Audio { get; private set; }

    public static Camera2D Camera { get; private set; }

    public static CameraController CameraController { get; private set; }

    public static DisplayMode PrimaryDisplayMode => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

    public static int MonitorWidth => PrimaryDisplayMode.Width;

    public static int MonitorHeight => PrimaryDisplayMode.Height;

    public static Point RecommendedWindowSize
    {
        get
        {
            int val = (int)((float)MonitorWidth * 0.8f);
            int val2 = (int)((float)MonitorHeight * 0.8f);
            val = Math.Max(800, val);
            val2 = Math.Max(600, val2);
            return new Point(val, val2);
        }
    }

    public static SpriteFont DebugFont
    {
        get
        {
            return DebugSystem.DebugFont;
        }
        set
        {
            DebugSystem.DebugFont = value;
        }
    }

    public static float DebugFontScale
    {
        get
        {
            return DebugSystem.DebugFontScale;
        }
        set
        {
            DebugSystem.DebugFontScale = value;
        }
    }

    public static bool DeveloperMode
    {
        get
        {
            return DebugSystem.DeveloperMode;
        }
        set
        {
            DebugSystem.DeveloperMode = value;
        }
    }

    public static bool ShowCollisionBoxes
    {
        get
        {
            return DebugSystem.ShowCollisionBoxes;
        }
        set
        {
            DebugSystem.ShowCollisionBoxes = value;
        }
    }

    public static bool ShowDebugMessages
    {
        get
        {
            return DebugSystem.ShowDebugMessages;
        }
        set
        {
            DebugSystem.ShowDebugMessages = value;
        }
    }

    public static IReadOnlyList<DebugSystem.DebugMessage> DebugMessages => DebugSystem.DebugMessages;

    public static FpsDisplayPosition FpsDisplayPosition
    {
        get
        {
            return s_fpsDisplayPosition;
        }
        set
        {
            s_fpsDisplayPosition = value;
        }
    }

    public static double CurrentFps => s_currentFps;

    public static void ToggleDeveloperMode()
    {
        DebugSystem.ToggleDeveloperMode();
    }

    public static void ToggleCollisionBoxes()
    {
        DebugSystem.ToggleCollisionBoxes();
    }

    public static void AddDebugMessage(string message)
    {
        DebugSystem.AddDebugMessage(message);
    }

    public static Vector2 GetFpsDisplayPosition()
    {
        return s_fpsDisplayPosition switch
        {
            FpsDisplayPosition.UpperLeft => new Vector2(10f, 10f),
            FpsDisplayPosition.Top => new Vector2(950f, 10f),
            FpsDisplayPosition.UpperRight => new Vector2(1790f, 10f),
            FpsDisplayPosition.Right => new Vector2(1790f, 530f),
            FpsDisplayPosition.BottomRight => new Vector2(1790f, 1050f),
            FpsDisplayPosition.Bottom => new Vector2(950f, 1050f),
            FpsDisplayPosition.BottomLeft => new Vector2(10f, 1050f),
            FpsDisplayPosition.Left => new Vector2(10f, 530f),
            _ => new Vector2(10f, 10f),
        };
    }

    public static Vector2 ScreenToVirtual(Vector2 screenPosition)
    {
        Vector2 contentScale = ContentScale;
        Rectangle scaledViewport = ScaledViewport;
        return (screenPosition - new Vector2(scaledViewport.X, scaledViewport.Y)) / contentScale;
    }

    public static Vector2 VirtualToScreen(Vector2 virtualPosition)
    {
        Vector2 contentScale = ContentScale;
        Rectangle scaledViewport = ScaledViewport;
        return virtualPosition * contentScale + new Vector2(scaledViewport.X, scaledViewport.Y);
    }

    public static Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        Vector2 vector = ScreenToVirtual(screenPosition);
        if (Camera == null)
        {
            return vector;
        }

        return Camera.ScreenToWorld(vector);
    }

    public static Vector2 WorldToScreen(Vector2 worldPosition)
    {
        if (Camera == null)
        {
            return VirtualToScreen(worldPosition);
        }

        return VirtualToScreen(Camera.WorldToScreen(worldPosition));
    }

    public static void SetVirtualResolution(int width, int height)
    {
        VirtualResolution = new Point(width, height);
    }

    public MyCore(string title, int width, int height, bool fullScreen)
    {
        Initialize(title, width, height, fullScreen);
    }

    public MyCore(string title, bool fullScreen = false, float windowSizePercent = 0.8f)
    {
        if (IsSteamDeck)
        {
            fullScreen = true;
        }

        Point monitorAwareSize = GetMonitorAwareSize(windowSizePercent);
        Initialize(title, monitorAwareSize.X, monitorAwareSize.Y, fullScreen);
    }

    public MyCore(string title)
    {
        if (IsSteamDeck)
        {
            Initialize(title, 1280, 800, fullScreen: true);
            return;
        }

        Point monitorAwareSize = GetMonitorAwareSize();
        Initialize(title, monitorAwareSize.X, monitorAwareSize.Y, fullScreen: false);
    }

    public static Point GetMonitorAwareSize(float sizePercent = 0.8f)
    {
        if (IsSteamDeck)
        {
            return new Point(1280, 800);
        }

        sizePercent = Math.Clamp(sizePercent, 0.1f, 1f);
        int val = (int)((float)MonitorWidth * sizePercent);
        int val2 = (int)((float)MonitorHeight * sizePercent);
        val = Math.Max(800, val);
        val2 = Math.Max(600, val2);
        val = Math.Min(val, MonitorWidth - 100);
        val2 = Math.Min(val2, MonitorHeight - 100);
        return new Point(val, val2);
    }

    private void Initialize(string title, int width, int height, bool fullScreen)
    {
        if (s_instance != null)
        {
            throw new InvalidOperationException("Only a single Core instance can be created");
        }

        s_instance = this;
        Graphics = new GraphicsDeviceManager(this);
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;
        Graphics.SynchronizeWithVerticalRetrace = true;
        base.IsFixedTimeStep = false;
        base.TargetElapsedTime = TimeSpan.FromMilliseconds(8.333333333333334);
        Graphics.ApplyChanges();
        base.Window.Title = title;
        Content = base.Content;
        Content.RootDirectory = "Content";
        base.IsMouseVisible = true;
        ExitOnEscape = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        GraphicsDevice = base.GraphicsDevice;
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        CollisionDraw.Initialize(GraphicsDevice);
        Input = new InputManager();
        Audio = new AudioController();
        Camera = new Camera2D();
        Camera.ResetToDefaultZoom();
        Camera.SetZoomLimitsForCharacter(64f);
        CameraController = new CameraController(Camera);
    }

    public static void ResizeWindow(int width, int height)
    {
        if (Graphics != null)
        {
            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;
            Graphics.ApplyChanges();
        }
    }

    public static void ResizeWindowToMonitorPercent(float sizePercent = 0.8f)
    {
        Point monitorAwareSize = GetMonitorAwareSize(sizePercent);
        ResizeWindow(monitorAwareSize.X, monitorAwareSize.Y);
    }

    public static void CenterWindow()
    {
        if (Graphics != null && !Graphics.IsFullScreen && s_instance?.Window != null)
        {
            GameWindow window = s_instance.Window;
            int val = (MonitorWidth - Graphics.PreferredBackBufferWidth) / 2;
            int val2 = (MonitorHeight - Graphics.PreferredBackBufferHeight) / 2;
            val = Math.Max(0, val);
            val2 = Math.Max(0, val2);
            window.Position = new Point(val, val2);
        }
    }

    protected override void UnloadContent()
    {
        s_sceneManager.ClearCache();
        Audio.Dispose();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update(gameTime);
        Audio.Update();

        UpdateFpsTracking(gameTime);

        s_sceneManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        s_sceneManager.Draw(gameTime);
        base.Draw(gameTime);
    }

    private static void UpdateFpsTracking(GameTime gameTime)
    {
        s_frameTimeBuffer[s_frameTimeIndex] = gameTime.ElapsedGameTime.TotalSeconds;
        s_frameTimeIndex = (s_frameTimeIndex + 1) % s_frameTimeBuffer.Length;
        if (s_frameTimeIndex == 0)
        {
            s_frameBufferFilled = true;
        }

        int num = (s_frameBufferFilled ? s_frameTimeBuffer.Length : s_frameTimeIndex);
        if (num > 0)
        {
            double num2 = 0.0;
            for (int i = 0; i < num; i++)
            {
                num2 += s_frameTimeBuffer[i];
            }

            double num3 = num2 / (double)num;
            s_currentFps = ((num3 > 0.0) ? (1.0 / num3) : 60.0);
            if (s_frameTimeIndex % 30 == 0)
            {
                _ = 5;
            }
        }
    }

    public static void EnableSceneManager()
    {
    }

    public static void TransitionTo<T>() where T : Scene, new()
    {
        s_sceneManager.TransitionTo<T>();
    }

    public static void ChangeScene(Scene next)
    {
        if (next != null)
        {
            bool cacheScene = next.GetType().Name == "GameScene";
            s_sceneManager.TransitionTo(next, cacheScene);
        }
    }
}
#if false // Decompilation log
'170' items in cache
------------------
Resolve: 'System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Runtime.dll'
------------------
Resolve: 'MonoGame.Framework, Version=3.8.4.1, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'MonoGame.Framework, Version=3.8.4.1, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\Kirill\.nuget\packages\monogame.framework.desktopgl\3.8.4.1\lib\net8.0\MonoGame.Framework.dll'
------------------
Resolve: 'System.Collections, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Collections.dll'
------------------
Resolve: 'System.Xml.ReaderWriter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.ReaderWriter, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Xml.ReaderWriter.dll'
------------------
Resolve: 'System.Xml.XDocument, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Xml.XDocument, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Xml.XDocument.dll'
------------------
Resolve: 'System.Text.Json, Version=10.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Text.Json, Version=10.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Text.Json.dll'
------------------
Resolve: 'System.Console, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Console, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Console.dll'
------------------
Resolve: 'System.Linq, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Linq.dll'
------------------
Resolve: 'System.Threading, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Threading, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Threading.dll'
------------------
Resolve: 'System.ComponentModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.ComponentModel, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.ComponentModel.dll'
------------------
Resolve: 'System.Runtime, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Runtime.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.InteropServices, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '9.0.0.0', Got: '10.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.4\ref\net10.0\System.Runtime.CompilerServices.Unsafe.dll'
#endif
