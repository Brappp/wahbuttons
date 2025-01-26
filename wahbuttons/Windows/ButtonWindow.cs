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
            ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        Plugin = plugin;
        Config = config;
        IsOpen = config.IsVisible;

        // Ensure RespectCloseHotkey is always false
        RespectCloseHotkey = false;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Set window flags dynamically
        Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

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

        // Synchronize IsOpen state with Config.IsVisible
        IsOpen = Config.IsVisible;

        // Set position and size
        ImGui.SetNextWindowPos(Config.Position, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(Config.Size, ImGuiCond.FirstUseEver);
    }

    public override void Draw()
    {
        if (!Config.IsLocked)
        {
            var currentPosition = ImGui.GetWindowPos();
            var currentSize = ImGui.GetWindowSize();

            // Save position if it has changed
            if (IsPositionDifferent(currentPosition, Config.Position))
            {
                Config.Position = currentPosition;
                Plugin.Configuration.Save();
            }

            // Save size if it has changed
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

        float buttonWidth = Config.Size.X / columns - 10; // Adjust for spacing
        float buttonHeight = Config.Size.Y / rows - 10;   // Adjust for spacing

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int buttonIndex = row * columns + col;
                if (buttonIndex >= Config.Buttons.Count)
                    break;

                var button = Config.Buttons[buttonIndex];

                // Use explicit width and height if set, otherwise fallback to grid-calculated sizes
                float finalWidth = button.Width > 0 ? button.Width : buttonWidth;
                float finalHeight = button.Height > 0 ? button.Height : buttonHeight;

                ImGui.PushStyleColor(ImGuiCol.Button, button.Color);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, button.Color * 1.2f);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, button.Color * 1.5f);

                if (ImGui.Button(button.Label, new Vector2(finalWidth, finalHeight)))
                {
                    Plugin.ChatGui.Print($"Executing command: {button.Command}");
                    Plugin.CommandManager.ProcessCommand(button.Command);
                }

                ImGui.PopStyleColor(3);

                if (col < columns - 1)
                {
                    ImGui.SameLine(); // Align buttons in the same row
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

            // Use explicit width and height if set, otherwise fallback to reasonable defaults
            float finalWidth = button.Width > 0 ? button.Width : Config.Size.X - 20; // Full width minus padding
            float finalHeight = button.Height > 0 ? button.Height : 30; // Default height

            if (ImGui.Button(button.Label, new Vector2(finalWidth, finalHeight)))
            {
                Plugin.ChatGui.Print($"Executing command: {button.Command}");
                Plugin.CommandManager.ProcessCommand(button.Command);
            }

            ImGui.PopStyleColor(3);

            currentPos.Y += finalHeight + 10; // Adjust for vertical spacing
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

            // Use explicit width and height if set, otherwise fallback to reasonable defaults
            float finalWidth = button.Width > 0 ? button.Width : 100; // Default width
            float finalHeight = button.Height > 0 ? button.Height : Config.Size.Y - 20; // Full height minus padding

            if (ImGui.Button(button.Label, new Vector2(finalWidth, finalHeight)))
            {
                Plugin.ChatGui.Print($"Executing command: {button.Command}");
                Plugin.CommandManager.ProcessCommand(button.Command);
            }

            ImGui.PopStyleColor(3);

            currentPos.X += finalWidth + 10; // Adjust for horizontal spacing
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
