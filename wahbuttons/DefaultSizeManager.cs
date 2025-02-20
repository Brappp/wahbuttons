using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using WahButtons.Windows;

namespace WahButtons.UI.Components;

public class DefaultSizeManager
{
    private readonly Configuration Configuration;
    private readonly List<ButtonWindow> ButtonWindows;
    private readonly HashSet<string> selectedWindows = new();
    private bool windowSizePopupOpen;
    private Vector2 defaultSize = new(75, 30);

    public DefaultSizeManager(Configuration configuration, List<ButtonWindow> buttonWindows)
    {
        Configuration = configuration;
        ButtonWindows = buttonWindows;
    }

    public void DrawDefaultButtonSize(ButtonWindow window)
    {
        ImGui.Text("Default Button Size");
        ImGui.Indent(10);

        DrawSizeInput();
        DrawSizeButtons(window);

        if (windowSizePopupOpen)
        {
            DrawSizePopup();
        }

        ImGui.Unindent(10);
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }

    private void DrawSizeInput()
    {
        ImGui.SetNextItemWidth(200);
        if (ImGui.DragFloat2("Size (W × H)", ref defaultSize, 0.5f, 20, 500, "%.1f"))
        {
            // Size is changed but not applied until a button is clicked
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Width × Height in pixels");
        }

        ImGui.Spacing();
        ImGui.Text("Apply to:");
    }

    private void DrawSizeButtons(ButtonWindow window)
    {
        if (ImGui.Button("Current Window"))
        {
            ApplySizeToWindow(window);
        }

        ImGui.SameLine();
        if (ImGui.Button("All Windows"))
        {
            foreach (var win in ButtonWindows)
            {
                ApplySizeToWindow(win);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Selected Windows..."))
        {
            windowSizePopupOpen = true;
            ImGui.OpenPopup("Apply to Windows");
        }
    }

    private void DrawSizePopup()
    {
        ImGui.SetNextWindowSize(new Vector2(300, 0));
        if (ImGui.BeginPopupModal("Apply to Windows", ref windowSizePopupOpen,
            ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Apply size {defaultSize.X:F1} × {defaultSize.Y:F1} to:");
            ImGui.Separator();

            foreach (var win in ButtonWindows)
            {
                bool selected = selectedWindows.Contains(win.Config.Name);
                if (ImGui.Checkbox($"{win.Config.Name} ({win.Config.Buttons.Count} buttons)", ref selected))
                {
                    if (selected)
                        selectedWindows.Add(win.Config.Name);
                    else
                        selectedWindows.Remove(win.Config.Name);
                }
            }

            ImGui.Separator();

            if (ImGui.Button("Apply", new Vector2(120, 0)))
            {
                foreach (var win in ButtonWindows)
                {
                    if (selectedWindows.Contains(win.Config.Name))
                    {
                        ApplySizeToWindow(win);
                    }
                }
                selectedWindows.Clear();
                windowSizePopupOpen = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                selectedWindows.Clear();
                windowSizePopupOpen = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void ApplySizeToWindow(ButtonWindow window)
    {
        foreach (var btn in window.Config.Buttons)
        {
            btn.Width = defaultSize.X;
            btn.Height = defaultSize.Y;
        }
        Configuration.Save();
    }
}
