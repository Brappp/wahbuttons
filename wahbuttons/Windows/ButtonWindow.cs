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
        RespectCloseHotkey = false;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

        if (Config.IsLocked)
        {
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;
        }

        if (Config.TransparentBackground)
        {
            Flags |= ImGuiWindowFlags.NoBackground;
        }

        IsOpen = Config.IsVisible;

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

                float finalWidth = button.Width > 0 ? button.Width : buttonWidth;
                float finalHeight = button.Height > 0 ? button.Height : buttonHeight;

                RenderButton(button, finalWidth, finalHeight);

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

            float finalWidth = button.Width > 0 ? button.Width : Config.Size.X - 20;
            float finalHeight = button.Height > 0 ? button.Height : 30;

            RenderButton(button, finalWidth, finalHeight);

            currentPos.Y += finalHeight + 10;
        }
    }

    private void DrawHorizontalLayout()
    {
        Vector2 currentPos = new(10, 10);
        foreach (var button in Config.Buttons)
        {
            ImGui.SetCursorPos(currentPos);

            float finalWidth = button.Width > 0 ? button.Width : 100;
            float finalHeight = button.Height > 0 ? button.Height : Config.Size.Y - 20;

            RenderButton(button, finalWidth, finalHeight);

            currentPos.X += finalWidth + 10;
        }
    }

    private void RenderButton(Configuration.ButtonData button, float width, float height)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, button.Color);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, button.Color * 1.2f);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, button.Color * 1.5f);
        ImGui.PushStyleColor(ImGuiCol.Text, button.LabelColor); // Apply label color

        if (ImGui.Button(button.Label, new Vector2(width, height)))
        {
            Plugin.ChatGui.Print($"Executing command: {button.Command}");
            Plugin.CommandManager.ProcessCommand(button.Command);
        }

        ImGui.PopStyleColor(4); // Pop all pushed styles
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