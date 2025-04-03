using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;
using WahButtons.Helpers;

namespace WahButtons.Windows;

public class ButtonWindow : Window, IDisposable
{
    private Plugin Plugin;
    public Configuration.ButtonWindowConfig Config { get; }

    public ButtonWindow(Plugin plugin, Configuration.ButtonWindowConfig config)
        : base(config.Name + "##" + Guid.NewGuid(),
            WindowHelper.GetWindowFlags(config))
    {
        Plugin = plugin;
        Config = config;
        IsOpen = config.IsVisible;
        RespectCloseHotkey = false;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        Flags = WindowHelper.GetWindowFlags(Config);
        IsOpen = Config.IsVisible;

        ImGui.SetNextWindowPos(Config.Position, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(Config.Size, ImGuiCond.FirstUseEver);
    }

    public override void Draw()
    {
        UpdatePositionAndSizeIfNeeded();
        DrawLayout();
    }

    private void UpdatePositionAndSizeIfNeeded()
    {
        if (Config.IsLocked)
            return;

        var currentPosition = ImGui.GetWindowPos();
        var currentSize = ImGui.GetWindowSize();

        if (WindowHelper.IsPositionDifferent(currentPosition, Config.Position))
        {
            Config.Position = currentPosition;
            Plugin.Configuration.Save();
        }

        if (WindowHelper.IsSizeDifferent(currentSize, Config.Size))
        {
            Config.Size = currentSize;
            Plugin.Configuration.Save();
        }
    }

    private void DrawLayout()
    {
        switch (Config.Layout)
        {
            case Configuration.ButtonLayout.Grid:
                LayoutHelper.HandleGridLayout(Config.Buttons, Config.Size, Config.GridRows, Config.GridColumns, RenderButton);
                break;
            case Configuration.ButtonLayout.Vertical:
                LayoutHelper.HandleVerticalLayout(Config.Buttons, Config.Size, RenderButton);
                break;
            case Configuration.ButtonLayout.Horizontal:
                LayoutHelper.HandleHorizontalLayout(Config.Buttons, Config.Size, RenderButton);
                break;
        }
    }

    private void RenderButton(Configuration.ButtonData button, float width, float height)
    {
        // Check if the button should be rendered based on conditions
        if (!ButtonHelper.ShouldRenderButton(button))
            return;

        // Apply button styles
        ButtonHelper.ApplyButtonStyles(button);

        // Check if the button should be enabled
        bool isEnabled = ButtonHelper.IsButtonEnabled(button);
        if (!isEnabled)
        {
            ImGui.BeginDisabled();
        }

        // Get the current label (may be changed by rules for smart buttons)
        string displayLabel = button.IsSmartButton ? ButtonHelper.GetButtonLabel(button) : button.Label;

        // Render the button
        if (ImGui.Button(displayLabel, new Vector2(width, height)))
        {
            ExecuteButtonCommand(button);
        }

        if (!isEnabled)
        {
            ImGui.EndDisabled();
        }

        ImGui.PopStyleColor(4); // Pop all pushed styles
    }

    private void ExecuteButtonCommand(Configuration.ButtonData button)
    {
        // Get the current command (may be changed by rules for smart buttons)
        string command = button.IsSmartButton ? ButtonHelper.GetButtonCommand(button) : button.Command;
        
        Plugin.ChatGui.Print($"Executing command: {command}");
        Plugin.CommandManager.ProcessCommand(command);
    }
}
