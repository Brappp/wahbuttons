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

        private bool isInitialized = false;

        public Plugin(IDalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            MainWindow = new MainWindow(this, Configuration, WindowSystem);
            WindowSystem.AddWindow(MainWindow);

            foreach (var buttonConfig in Configuration.Windows)
            {
                var buttonWindow = new ButtonWindow(this, buttonConfig);
                WindowSystem.AddWindow(buttonWindow);
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

            foreach (var window in WindowSystem.Windows.OfType<ButtonWindow>())
            {
                window.IsOpen = true;
            }
        }

        private void OnLogout(int type, int code)
        {
            if (MainWindow != null)
            {
                MainWindow.IsOpen = false;
            }

            foreach (var window in WindowSystem.Windows.OfType<ButtonWindow>())
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
