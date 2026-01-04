using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using WahButtons.Windows;

namespace WahButtons;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration Configuration;
    public List<ButtonWindow> ButtonWindows = new();
    private WindowSystem WindowSystem;
    private ButtonWindow? SelectedWindowToDelete = null;
    private bool ShowDeleteConfirmation = false;

    public MainWindow(Plugin plugin, Configuration configuration, WindowSystem windowSystem)
        : base("Wah Buttons##Main")
    {
        Plugin = plugin;
        Configuration = configuration;
        WindowSystem = windowSystem;

        // Clear any existing ButtonWindows to prevent duplicates
        ButtonWindows.Clear();

        // Load ButtonWindows from configuration
        foreach (var config in Configuration.Windows)
        {
            // Check if a window with this config already exists in the WindowSystem
            var existingWindow = WindowSystem.Windows
                .OfType<ButtonWindow>()
                .FirstOrDefault(w => w.Config == config);

            if (existingWindow == null)
            {
                AddButtonWindowFromConfig(config);
            }
            else
            {
                // If it already exists, just add it to our tracking list
                ButtonWindows.Add(existingWindow);
            }
        }
    }

    public void Dispose()
    {
        SaveAllButtonWindows();
    }

    public override void Draw()
    {
        ImGui.Text("Manage Wah Buttons");
        ImGui.Separator();

        // Add New Window
        if (ImGui.Button("Add Window"))
        {
            var newConfig = new Configuration.ButtonWindowConfig
            {
                Name = "Window " + (ButtonWindows.Count + 1),
                IsVisible = true,
                Layout = Configuration.ButtonLayout.Grid,
                GridRows = 2,
                GridColumns = 2
            };

            Configuration.Windows.Add(newConfig);
            AddButtonWindowFromConfig(newConfig);
            Configuration.Save();
        }

        ImGui.Separator();

        // Remove Specific Window Dropdown and Button
        ImGui.Text("Select a window to delete:");
        ImGui.SetNextItemWidth(150); // Adjust dropdown width
        if (ImGui.BeginCombo("##SelectWindowToDelete", SelectedWindowToDelete?.Config.Name ?? "None"))
        {
            foreach (var window in ButtonWindows)
            {
                if (ImGui.Selectable(window.Config.Name, SelectedWindowToDelete == window))
                {
                    SelectedWindowToDelete = window;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.1f, 1.0f)); // Red color for delete button
        if (ImGui.Button("Delete Window") && SelectedWindowToDelete != null)
        {
            ShowDeleteConfirmation = true;
        }
        ImGui.PopStyleColor();

        if (ShowDeleteConfirmation && SelectedWindowToDelete != null)
        {
            ImGui.OpenPopup("Delete Confirmation");
        }

        if (ImGui.BeginPopupModal("Delete Confirmation", ref ShowDeleteConfirmation, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Are you sure you want to delete {SelectedWindowToDelete?.Config.Name}?");
            ImGui.Separator();

            if (ImGui.Button("Yes", new Vector2(120, 0)))
            {
                RemoveButtonWindow(SelectedWindowToDelete!);
                SelectedWindowToDelete = null;
                ShowDeleteConfirmation = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("No", new Vector2(120, 0)))
            {
                SelectedWindowToDelete = null;
                ShowDeleteConfirmation = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }

        ImGui.Separator();

        // Tab bar for each ButtonWindow
        if (ImGui.BeginTabBar("DynamicWindowsTabBar"))
        {
            foreach (var window in ButtonWindows)
            {
                if (ImGui.BeginTabItem(window.Config.Name))
                {
                    DrawWindowControls(window);
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }

    private void DrawWindowControls(ButtonWindow window)
    {
        ImGui.Text($"Editing: {window.Config.Name}");
        ImGui.Separator();

        // Toggle Visibility
        bool isVisible = window.Config.IsVisible;
        if (ImGui.Checkbox("Show", ref isVisible))
        {
            window.Config.IsVisible = isVisible;
            window.IsOpen = isVisible;
            Configuration.Save();
        }

        // Lock Position
        bool isLocked = window.Config.IsLocked;
        if (ImGui.Checkbox("Lock Position", ref isLocked))
        {
            window.Config.IsLocked = isLocked;
            Configuration.Save();
        }

        // Transparent Background
        bool transparent = window.Config.TransparentBackground;
        if (ImGui.Checkbox("Transparent Background", ref transparent))
        {
            window.Config.TransparentBackground = transparent;
            Configuration.Save();
        }

        // Layout Options
        ImGui.Text("Layout:");
        if (ImGui.RadioButton("Vertical", window.Config.Layout == Configuration.ButtonLayout.Vertical))
        {
            window.Config.Layout = Configuration.ButtonLayout.Vertical;
            Configuration.Save();
        }
        if (ImGui.RadioButton("Horizontal", window.Config.Layout == Configuration.ButtonLayout.Horizontal))
        {
            window.Config.Layout = Configuration.ButtonLayout.Horizontal;
            Configuration.Save();
        }
        if (ImGui.RadioButton("Grid", window.Config.Layout == Configuration.ButtonLayout.Grid))
        {
            window.Config.Layout = Configuration.ButtonLayout.Grid;
            Configuration.Save();
        }

        if (window.Config.Layout == Configuration.ButtonLayout.Grid)
        {
            int rows = window.Config.GridRows;
            if (ImGui.InputInt("Rows", ref rows))
            {
                window.Config.GridRows = Math.Max(1, rows);
                Configuration.Save();
            }

            int columns = window.Config.GridColumns;
            if (ImGui.InputInt("Columns", ref columns))
            {
                window.Config.GridColumns = Math.Max(1, columns);
                Configuration.Save();
            }
        }

        ImGui.Separator();

        // Add and Manage Buttons
        if (ImGui.Button($"Add Button##{window.Config.Name}"))
        {
            window.Config.Buttons.Add(new Configuration.ButtonData("New Button", "/command", 75));
            Configuration.Save();
        }

        for (int i = 0; i < window.Config.Buttons.Count; i++)
        {
            var button = window.Config.Buttons[i];
            ImGui.PushID($"Button{i}");

            ImGui.Text($"Button {i + 1}");

            // Move Buttons
            ImGui.SameLine();
            if (ImGui.Button("▲") && i > 0)
            {
                SwapButtons(window.Config.Buttons, i, i - 1);
                Configuration.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("▼") && i < window.Config.Buttons.Count - 1)
            {
                SwapButtons(window.Config.Buttons, i, i + 1);
                Configuration.Save();
            }

            // Button Attributes
            string label = button.Label;
            if (ImGui.InputText("Label", ref label, 256))
            {
                button.Label = label;
                Configuration.Save();
            }

            string command = button.Command;
            if (ImGui.InputText("Command", ref command, 256))
            {
                button.Command = command;
                Configuration.Save();
            }

            float width = button.Width;
            if (ImGui.InputFloat("Width", ref width))
            {
                button.Width = width;
                Configuration.Save();
            }

            float height = button.Height;
            if (ImGui.InputFloat("Height", ref height))
            {
                button.Height = height;
                Configuration.Save();
            }

            Vector4 color = button.Color;
            if (ImGui.ColorEdit4("Button Color", ref color))
            {
                button.Color = color;
                Configuration.Save();
            }

            Vector4 labelColor = button.LabelColor;
            if (ImGui.ColorEdit4("Label Color", ref labelColor))
            {
                button.LabelColor = labelColor;
                Configuration.Save();
            }

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.1f, 1.0f));
            if (ImGui.Button("Remove Button"))
            {
                window.Config.Buttons.RemoveAt(i);
                Configuration.Save();
                ImGui.PopStyleColor();
                ImGui.PopID();
                break;
            }
            ImGui.PopStyleColor();

            ImGui.PopID();
        }
    }

    private void AddButtonWindowFromConfig(Configuration.ButtonWindowConfig config)
    {
        var window = new ButtonWindow(Plugin, config)
        {
            IsOpen = config.IsVisible
        };
        ButtonWindows.Add(window);
        WindowSystem.AddWindow(window);
    }

    private void RemoveButtonWindow(ButtonWindow window)
    {
        ButtonWindows.Remove(window);
        WindowSystem.RemoveWindow(window);
        Configuration.Windows.Remove(window.Config);
        Configuration.Save();
    }

    private void SwapButtons(List<Configuration.ButtonData> buttons, int indexA, int indexB)
    {
        (buttons[indexA], buttons[indexB]) = (buttons[indexB], buttons[indexA]);
    }

    public void SaveAllButtonWindows()
    {
        foreach (var window in ButtonWindows)
        {
            if (!Configuration.Windows.Contains(window.Config))
            {
                Configuration.Windows.Add(window.Config);
            }
        }
        Configuration.Save();
    }
}