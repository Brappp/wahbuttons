using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.IoC;
using WahButtons.Windows;
using ImGuiNET;

namespace WahButtons
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Wah Buttons";
        private const string CommandName = "/wahbuttons";

        public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] private static IFramework Framework { get; set; } = null!;
        [PluginService] private static IClientState ClientState { get; set; } = null!;
        [PluginService] private static IPluginLog PluginLog { get; set; } = null!;

        public Configuration Configuration { get; init; }
        private MainWindow MainWindow { get; init; }
        private WindowSystem WindowSystem = new("Wah Buttons");

        // Make ButtonWindows public so MainWindow can access it
        public Dictionary<string, ButtonWindow> ButtonWindows { get; private set; } = new();

        private bool isInitialized = false;

        public Plugin(IDalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            MainWindow = new MainWindow(this, Configuration, WindowSystem);
            WindowSystem.AddWindow(MainWindow);

            // Clear existing windows from the UI system
            foreach (var window in WindowSystem.Windows.ToArray())
            {
                if (window is ButtonWindow)
                {
                    WindowSystem.RemoveWindow(window);
                }
            }

            // Create new windows from configuration
            ButtonWindows.Clear();
            foreach (var buttonConfig in Configuration.Windows)
            {
                CreateButtonWindow(buttonConfig);
            }

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            PluginInterface.UiBuilder.OpenMainUi += DrawMainUI;

            Framework.Update += OnFrameworkUpdate;
            ClientState.Login += OnLogin;
            ClientState.Logout += OnLogout;

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the configuration window."
            });
        }

        // Method to create a button window and avoid duplicates
        public ButtonWindow CreateButtonWindow(Configuration.ButtonWindowConfig config)
        {
            // Remove existing window with the same config if any
            if (ButtonWindows.TryGetValue(config.Name, out var existingWindow))
            {
                WindowSystem.RemoveWindow(existingWindow);
                ButtonWindows.Remove(config.Name);
            }

            // Create and add the new window
            var buttonWindow = new ButtonWindow(this, config);
            ButtonWindows[config.Name] = buttonWindow;
            WindowSystem.AddWindow(buttonWindow);

            return buttonWindow;
        }

        // Method to remove a button window
        public void RemoveButtonWindow(ButtonWindow window)
        {
            if (ButtonWindows.ContainsValue(window))
            {
                string? keyToRemove = ButtonWindows.FirstOrDefault(x => x.Value == window).Key;
                if (keyToRemove != null)
                {
                    ButtonWindows.Remove(keyToRemove);
                    WindowSystem.RemoveWindow(window);
                }
            }
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
            Configuration.Save();
            PluginLog.Information($"{Name} initialized.");
        }

        private void OnLogin()
        {
            if (MainWindow != null)
            {
                MainWindow.IsOpen = false;
            }

            foreach (var window in ButtonWindows.Values)
            {
                window.IsOpen = window.Config.IsVisible;
            }
        }

        private void OnLogout(int type, int code)
        {
            if (MainWindow != null)
            {
                MainWindow.IsOpen = false;
            }

            foreach (var window in ButtonWindows.Values)
            {
                window.IsOpen = false;
            }
        }

        private void OnCommand(string command, string args)
        {
            MainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        private void DrawConfigUI()
        {
            MainWindow.IsOpen = true;
        }

        private void DrawMainUI()
        {
            MainWindow.IsOpen = true;
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            ButtonWindows.Clear();

            CommandManager.RemoveHandler(CommandName);

            PluginInterface.UiBuilder.Draw -= DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            PluginInterface.UiBuilder.OpenMainUi -= DrawMainUI;

            Framework.Update -= OnFrameworkUpdate;
            ClientState.Login -= OnLogin;
            ClientState.Logout -= OnLogout;
        }
    }
}
