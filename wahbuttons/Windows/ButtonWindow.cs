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
        : base(config.Name + "##" + Guid.NewGuid(),
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

        // Apply locking logic
        if (Config.IsLocked)
        {
            Flags |= ImGuiWindowFlags.NoMove;
            Flags |= ImGuiWindowFlags.NoResize;
        }

        // Apply transparency logic
        if (Config.TransparentBackground)
        {
            Flags |= ImGuiWindowFlags.NoBackground;
        }

        ImGui.SetNextWindowPos(Config.Position, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(Config.Size, ImGuiCond.FirstUseEver);
    }

    public override void Draw()
    {
        if (!Config.IsLocked)
        {
            var currentPosition = ImGui.GetWindowPos();
            var currentSize = ImGui.GetWindowSize();

            if (IsPositionDifferent(currentPosition, Config.Position))
            {
                Config.Position = currentPosition;
                Plugin.Configuration.Save();
            }

            if (IsSizeDifferent(currentSize, Config.Size))
            {
                Config.Size = currentSize;
                Plugin.Configuration.Save();
            }
        }

        // Render buttons based on layout
        switch (Config.Layout)
        {
            case Configuration.ButtonLayout.Grid:
                DrawGridLayout();
                break;
            case Configuration.ButtonLayout.Vertical:
                DrawVerticalLayout();
                break;
            case Configuration.ButtonLayout.Horizontal:
                DrawHorizontalLayout();
                break;
        }
    }

    private void DrawGridLayout()
    {
        int rows = Config.GridRows;
        int columns = Config.GridColumns;

        float buttonWidth = Config.Size.X / columns - 10; 
        float buttonHeight = Config.Size.Y / rows - 10;  

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int buttonIndex = row * columns + col;
                if (buttonIndex >= Config.Buttons.Count)
                    break;

                var button = Config.Buttons[buttonIndex];

                ImGui.PushStyleColor(ImGuiCol.Button, button.Color);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, button.Color * 1.2f);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, button.Color * 1.5f);

                if (ImGui.Button(button.Label, new Vector2(buttonWidth, buttonHeight)))
                {
                    Plugin.ChatGui.Print($"Executing command: {button.Command}");
                    Plugin.CommandManager.ProcessCommand(button.Command);
                }

                ImGui.PopStyleColor(3);

                if (col < columns - 1)
                {
                    ImGui.SameLine(); 
                }
            }
        }
    }

    private void DrawVerticalLayout()
    {
        Vector2 currentPos = new(10, 10);
        foreach (var button in Config.Buttons)
        {
            ImGui.SetCursorPos(currentPos);

            ImGui.PushStyleColor(ImGuiCol.Button, button.Color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, button.Color * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, button.Color * 1.5f);

            if (ImGui.Button(button.Label, new Vector2(button.Width, button.Height)))
            {
                Plugin.ChatGui.Print($"Executing command: {button.Command}");
                Plugin.CommandManager.ProcessCommand(button.Command);
            }

            ImGui.PopStyleColor(3);

            currentPos.Y += button.Height + 10; // Adjust for vertical spacing
        }
    }

    private void DrawHorizontalLayout()
    {
        Vector2 currentPos = new(10, 10);
        foreach (var button in Config.Buttons)
        {
            ImGui.SetCursorPos(currentPos);

            ImGui.PushStyleColor(ImGuiCol.Button, button.Color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, button.Color * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, button.Color * 1.5f);

            if (ImGui.Button(button.Label, new Vector2(button.Width, button.Height)))
            {
                Plugin.ChatGui.Print($"Executing command: {button.Command}");
                Plugin.CommandManager.ProcessCommand(button.Command);
            }

            ImGui.PopStyleColor(3);

            currentPos.X += button.Width + 10; // Adjust for horizontal spacing
        }
    }

    private bool IsPositionDifferent(Vector2 a, Vector2 b, float tolerance = 0.1f)
    {
        return Math.Abs(a.X - b.X) > tolerance || Math.Abs(a.Y - b.Y) > tolerance;
    }

    private bool IsSizeDifferent(Vector2 a, Vector2 b, float tolerance = 0.1f)
    {
        return Math.Abs(a.X - b.X) > tolerance || Math.Abs(a.Y - b.Y) > tolerance;
    }
}
