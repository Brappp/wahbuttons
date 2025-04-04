using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Linq;
using Dalamud.Plugin.Services;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.IoC;
using WahButtons.Windows;
using ImGuiNET;
using Dalamud.Game.ClientState.Conditions;

namespace WahButtons
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Wah Buttons";
        private const string CommandName = "/wahbuttons";

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] public static ICondition Condition { get; private set; } = null!;
        [PluginService] public static IGameGui GameGui { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static IClientState ClientStatePlugin { get; private set; } = null!;

        public Configuration Configuration { get; private set; }
        private MainWindow MainWindow;
        private ConditionWindow ConditionWindow;
        private AetheryteWindow AetheryteWindow;
        private AdvancedWindow AdvancedWindow;
        private HelpWindow HelpWindow;
        public WindowSystem WindowSystem { get; private set; } = new("Wah Buttons");

        private bool isInitialized = false;

        public Plugin()
        {
            Framework.Update += OnFrameworkUpdate;
            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;
            PluginInterface.UiBuilder.OpenConfigUi += OpenConfigWindow;
        }

        private void OnFrameworkUpdate(IFramework framework)
        {
            if (ClientState.IsLoggedIn && !isInitialized)
            {
                Initialize();
                isInitialized = true;
                Framework.Update -= OnFrameworkUpdate;
            }
        }

        private void Initialize()
        {
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Save();

            MainWindow = new MainWindow(this, Configuration, WindowSystem)
            {
                IsOpen = false // Ensure MainWindow starts closed
            };

            ConditionWindow = new ConditionWindow(this)
            {
                IsOpen = false
            };

            AetheryteWindow = new AetheryteWindow(this)
            {
                IsOpen = false
            };
            
            AdvancedWindow = new AdvancedWindow(this, ConditionWindow, AetheryteWindow)
            {
                IsOpen = Configuration.ShowConditionWindow
            };
            
            HelpWindow = new HelpWindow(this)
            {
                IsOpen = false
            };

            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(ConditionWindow);
            WindowSystem.AddWindow(AetheryteWindow);
            WindowSystem.AddWindow(AdvancedWindow);
            WindowSystem.AddWindow(HelpWindow);

            foreach (var buttonConfig in Configuration.Windows)
            {
                var buttonWindow = new ButtonWindow(this, buttonConfig);
                WindowSystem.AddWindow(buttonWindow);
            }

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = @"Opens the main window.

/wahbuttons <window_name> - Toggles the visibility of a specific window.
/wahbuttons help - Shows the help window with documentation.
/wahbuttons advanced - Opens the advanced features window.
/wahbuttons conditions - Opens the condition tracker tab (legacy).
/wahbuttons aetherytes - Opens the aetheryte teleport tab (legacy).
Example: /wahbuttons Window 1"
            });

            PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
            PluginInterface.UiBuilder.OpenMainUi += () => MainWindow.IsOpen = true;

            PluginLog.Information($"{Name} initialized.");
        }

        private void OpenConfigWindow()
        {
            if (!MainWindow.IsOpen)
            {
                MainWindow.IsOpen = true;
                PluginLog.Debug("Main window opened via Config UI callback.");
            }
        }

        private void OnLogin()
        {
            PluginLog.Debug("Login detected.");
            // Ensure the MainWindow does not open on login
            if (MainWindow != null)
            {
                MainWindow.IsOpen = false; // Explicitly ensure MainWindow remains closed
                PluginLog.Debug("Main window kept closed due to login.");
            }

            // Open all ButtonWindows
            foreach (var window in WindowSystem.Windows.OfType<ButtonWindow>())
            {
                window.IsOpen = true;
            }
        }

        private void OnLogout(int type, int code)
        {
            PluginLog.Debug($"Logout detected. Type: {type}, Code: {code}");

            if (MainWindow != null)
            {
                MainWindow.IsOpen = false;
                PluginLog.Debug("Main window hidden due to logout.");
            }

            foreach (var window in WindowSystem.Windows.OfType<ButtonWindow>())
            {
                window.IsOpen = false;
            }
        }

        private void OnCommand(string command, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                MainWindow.IsOpen = !MainWindow.IsOpen;
                return;
            }

            if (args.Trim().Equals("advanced", StringComparison.OrdinalIgnoreCase))
            {
                AdvancedWindow.IsOpen = !AdvancedWindow.IsOpen;
                return;
            }
            
            if (args.Trim().Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                HelpWindow.IsOpen = !HelpWindow.IsOpen;
                return;
            }

            if (args.Trim().Equals("conditions", StringComparison.OrdinalIgnoreCase))
            {
                // Legacy command, open advanced window with conditions tab
                AdvancedWindow.IsOpen = true;
                ConditionWindow.IsOpen = true;
                return;
            }

            if (args.Trim().Equals("aetherytes", StringComparison.OrdinalIgnoreCase))
            {
                // Legacy command, open advanced window with aetherytes tab
                AdvancedWindow.IsOpen = true;
                AetheryteWindow.IsOpen = true;
                return;
            }

            // Handle window toggling by name
            var windowName = args.Trim();
            var window = WindowSystem.Windows.OfType<ButtonWindow>()
                .FirstOrDefault(w => w.Config.Name.Equals(windowName, StringComparison.OrdinalIgnoreCase));

            if (window != null)
            {
                window.IsOpen = !window.IsOpen;
                window.Config.IsVisible = window.IsOpen;
                Configuration.Save();
            }
            else
            {
                ChatGui.PrintError($"Window '{windowName}' not found.");
            }
        }

        public void Dispose()
        {
            PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
            PluginInterface.UiBuilder.OpenMainUi -= () => MainWindow.IsOpen = true;
            PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigWindow;

            CommandManager.RemoveHandler(CommandName);

            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;

            Framework.Update -= OnFrameworkUpdate;

            WindowSystem.RemoveAllWindows();

            PluginLog.Information($"{Name} disposed.");
        }
        
        public void AddDefaultWindow()
        {
            var defaultConfig = new Configuration.ButtonWindowConfig
            {
                Name = "Default Window",
                IsVisible = true,
                IsLocked = false,
                Position = new System.Numerics.Vector2(100, 100),
                Size = new System.Numerics.Vector2(300, 200),
                Layout = Configuration.ButtonLayout.Grid,
                GridRows = 3,
                GridColumns = 3,
                Buttons = new System.Collections.Generic.List<Configuration.ButtonData>
                {
                    new Configuration.ButtonData
                    {
                        Label = "Teleport",
                        Command = "/tp",
                        Width = 80,
                        Height = 30,
                        Color = new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 1.0f)
                    },
                    new Configuration.ButtonData
                    {
                        Label = "Return",
                        Command = "/return",
                        Width = 80,
                        Height = 30,
                        Color = new System.Numerics.Vector4(0.0f, 0.6f, 0.4f, 1.0f)
                    }
                }
            };
            
            Configuration.Windows.Add(defaultConfig);
            var buttonWindow = new ButtonWindow(this, defaultConfig);
            WindowSystem.AddWindow(buttonWindow);
            Configuration.Save();
            
            PluginLog.Information("Created default window");
        }
    }
}
