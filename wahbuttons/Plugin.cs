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

        public Configuration Configuration { get; private set; }
        private MainWindow MainWindow;
        private WindowSystem WindowSystem = new("Wah Buttons");

        private bool isInitialized = false;

        public Plugin()
        {
            Framework.Update += OnFrameworkUpdate;

            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;
            ClientState.TerritoryChanged += OnTerritoryChanged;

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
                IsOpen = ClientState.IsLoggedIn
            };

            WindowSystem.AddWindow(MainWindow);

            foreach (var buttonConfig in Configuration.Windows)
            {
                var buttonWindow = new ButtonWindow(this, buttonConfig);
                WindowSystem.AddWindow(buttonWindow);
            }

            // Register commands
            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = @"Opens the main window.

/wahbuttons <window_name> - Toggles the visibility of a specific window.
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
            if (MainWindow != null)
            {
                MainWindow.IsOpen = true;
                PluginLog.Debug("Main window shown due to login.");
            }

            // Show all button windows on login
            foreach (var window in WindowSystem.Windows.OfType<ButtonWindow>())
            {
                window.IsOpen = true;
            }
        }

        private void OnLogout(int type, int code)
        {
            PluginLog.Debug($"Logout detected. Type: {type}, Code: {code}");

            // Hide the main window
            if (MainWindow != null)
            {
                MainWindow.IsOpen = false;
                PluginLog.Debug("Main window hidden due to logout.");
            }

            // Hide all button windows on logout
            foreach (var window in WindowSystem.Windows.OfType<ButtonWindow>())
            {
                window.IsOpen = false;
            }
        }

        private void OnTerritoryChanged(ushort territoryId)
        {
            PluginLog.Debug($"Territory changed to: {territoryId}");
            if (MainWindow != null && !MainWindow.IsOpen)
            {
                PluginLog.Debug("Reopening main window due to territory change.");
                MainWindow.IsOpen = true;
            }
        }

        private void OnCommand(string command, string args)
        {
            string windowName = args.Trim();
            if (string.IsNullOrEmpty(windowName))
            {
                MainWindow.IsOpen = true;
                ChatGui.Print("Main window opened.");
            }
            else
            {
                var window = WindowSystem.Windows
                    .OfType<ButtonWindow>()
                    .FirstOrDefault(bw => bw.Config.Name.Equals(windowName, StringComparison.OrdinalIgnoreCase));

                if (window != null)
                {
                    window.Config.IsVisible = !window.Config.IsVisible;
                    window.IsOpen = window.Config.IsVisible;
                    Configuration.Save();
                    ChatGui.Print($"{windowName} visibility toggled.");
                }
                else
                {
                    ChatGui.PrintError($"No window found with the name: {windowName}");
                }
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
            ClientState.TerritoryChanged -= OnTerritoryChanged;

            Framework.Update -= OnFrameworkUpdate;

            WindowSystem.RemoveAllWindows();

            PluginLog.Information($"{Name} disposed.");
        }
    }
}
