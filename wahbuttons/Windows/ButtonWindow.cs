using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace WahButtons.Windows;

public class ButtonWindow : Window, IDisposable
{
    private Plugin Plugin;
    public Configuration.ButtonWindowConfig Config { get; }

    public ButtonWindow(Plugin plugin, Configuration.ButtonWindowConfig config)
        : base($"{config.Name}##{Guid.NewGuid()}", // Ensure unique ID for every instance
            (config.IsLocked ? ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove : ImGuiWindowFlags.None) |
            (config.TransparentBackground ? ImGuiWindowFlags.NoBackground : ImGuiWindowFlags.None) |
            ImGuiWindowFlags.NoTitleBar)
    {
        Plugin = plugin;
        Config = config;
        IsOpen = config.IsVisible;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        Flags = ImGuiWindowFlags.NoTitleBar;

        if (Config.IsLocked)
        {
            Flags |= ImGuiWindowFlags.NoMove;
            Flags |= ImGuiWindowFlags.NoResize;
        }

        if (Config.TransparentBackground)
        {
            Flags |= ImGuiWindowFlags.NoBackground;
        }
    }

    public override void Draw()
    {
        ImGui.Text($"This is {Config.Name}.");
    }
}
