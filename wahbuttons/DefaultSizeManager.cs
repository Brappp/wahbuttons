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

        // Initialize selectedWindows from saved configuration if available
        if (Configuration is not null && Configuration.SelectedWindowsForSize is not null)
        {
            foreach (var winName in Configuration.SelectedWindowsForSize)
            {
                selectedWindows.Add(winName);
            }
        }
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
        // Apply to current window
        if (ImGui.Button("Current Window"))
        {
            ApplySizeToWindow(window);
        }

        // Apply to selected windows
        ImGui.SameLine();
        if (ImGui.Button("Selected Windows..."))
        {
            windowSizePopupOpen = true;
            ImGui.OpenPopup("Apply to Windows");
        }

        // Add a tooltip to indicate selection state
        if (ImGui.IsItemHovered() && selectedWindows.Count > 0)
        {
            ImGui.BeginTooltip();
            ImGui.Text($"{selectedWindows.Count} windows currently selected");
            ImGui.EndTooltip();
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

            // Select/Deselect all buttons
            bool allSelected = selectedWindows.Count == ButtonWindows.Count;
            if (ImGui.Checkbox("Select All", ref allSelected))
            {
                if (allSelected)
                {
                    // Select all windows
                    foreach (var win in ButtonWindows)
                    {
                        selectedWindows.Add(win.Config.Name);
                    }
                }
                else
                {
                    // Deselect all windows
                    selectedWindows.Clear();
                }

                // Save selection to configuration
                SaveSelectedWindows();
            }

            ImGui.Separator();

            // Window checkboxes
            foreach (var win in ButtonWindows)
            {
                bool selected = selectedWindows.Contains(win.Config.Name);
                if (ImGui.Checkbox($"{win.Config.Name} ({win.Config.Buttons.Count} buttons)", ref selected))
                {
                    if (selected)
                        selectedWindows.Add(win.Config.Name);
                    else
                        selectedWindows.Remove(win.Config.Name);

                    // Save selection to configuration
                    SaveSelectedWindows();
                }
            }

            ImGui.Separator();

            // Action buttons
            if (ImGui.Button("Apply", new Vector2(120, 0)))
            {
                foreach (var win in ButtonWindows)
                {
                    if (selectedWindows.Contains(win.Config.Name))
                    {
                        ApplySizeToWindow(win);
                    }
                }
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
            }

            // Keep the popup open until it's explicitly closed
            if (!windowSizePopupOpen)
            {
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

    private void SaveSelectedWindows()
    {
        // Save the selected windows to configuration
        Configuration.SelectedWindowsForSize = new List<string>(selectedWindows);
        Configuration.Save();
    }
}
