using System;
using System.Numerics;
using System.Linq;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using WahButtons.Windows;
using WahButtons.Helpers;

namespace WahButtons.UI.Components;

public class WindowManager
{
    private readonly MainWindow MainWindow;
    private readonly Configuration Configuration;
    private readonly WindowSystem WindowSystem;
    private readonly Plugin Plugin;
    private ButtonWindow? SelectedWindowToDelete;
    private bool ShowDeleteConfirmation;
    private string NewWindowName = "New Window";
    private string? editingWindowName = null;
    private bool needToApplyNameChange = false;

    public WindowManager(MainWindow mainWindow, Configuration configuration, WindowSystem windowSystem)
    {
        MainWindow = mainWindow;
        Configuration = configuration;
        WindowSystem = windowSystem;
        Plugin = mainWindow.Plugin;
    }

    public void DrawWindowManagement()
    {
        ImGui.Text("Wah Buttons Management");
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text("Window Management");

        DrawWindowCreation();

        if (ShowDeleteConfirmation)
        {
            DrawDeleteConfirmationPopup();
        }
    }

    private void DrawWindowCreation()
    {
        ImGui.BeginGroup();
        {
            // Left side - Add window with name
            ImGui.BeginGroup();
            {
                ImGui.SetNextItemWidth(200);
                if (ImGui.InputText("##NewWindowName", ref NewWindowName, 32))
                {
                    // Name is updated
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Enter name for new window");
                }

                if (ImGui.Button("Add New Window", new Vector2(120, 25)))
                {
                    CreateNewWindow();
                }
            }
            ImGui.EndGroup();

            // Right side - Window deletion
            ImGui.SameLine(ImGui.GetWindowWidth() * 0.4f);
            DrawWindowDeletion();
        }
        ImGui.EndGroup();
    }

    private void CreateNewWindow()
    {
        // Make sure the name is unique
        string windowName = string.IsNullOrEmpty(NewWindowName.Trim())
            ? $"Window {MainWindow.ButtonWindows.Count + 1}"
            : NewWindowName;

        // Make the name unique by appending a number if needed
        int suffix = 1;
        string baseName = windowName;
        while (Configuration.Windows.Any(w => w.Name == windowName))
        {
            windowName = $"{baseName} {suffix++}";
        }

        var newConfig = new Configuration.ButtonWindowConfig
        {
            Name = windowName,
            IsVisible = true,
            Layout = Configuration.ButtonLayout.Grid,
            GridRows = 2,
            GridColumns = 2
        };

        // Add to configuration
        Configuration.Windows.Add(newConfig);

        // Create and add the window
        var newWindow = new ButtonWindow(Plugin, newConfig) { IsOpen = true };
        MainWindow.ButtonWindows.Add(newWindow);
        WindowSystem.AddWindow(newWindow);

        // Set the active tab to the new window
        MainWindow.ActiveTabName = windowName;

        // Save and reset name
        Configuration.Save();
        NewWindowName = "New Window";
    }

    private void DrawWindowDeletion()
    {
        ImGui.BeginGroup();
        {
            ImGui.SetNextItemWidth(250);
            if (ImGui.BeginCombo("##WindowSelect", SelectedWindowToDelete?.Config.Name ?? "Select Window to Delete"))
            {
                if (SelectedWindowToDelete != null && !MainWindow.ButtonWindows.Contains(SelectedWindowToDelete))
                {
                    SelectedWindowToDelete = null;
                }

                foreach (var window in MainWindow.ButtonWindows)
                {
                    bool isSelected = SelectedWindowToDelete == window;
                    if (ImGui.Selectable(window.Config.Name, isSelected))
                    {
                        SelectedWindowToDelete = window;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGuiHelper.PushButtonColors(ImGuiHelper.DangerButtonColor);
            if (ImGui.Button("Delete Selected Window", new Vector2(150, 25)))
            {
                if (SelectedWindowToDelete != null)
                {
                    ShowDeleteConfirmation = true;
                    ImGui.OpenPopup("Delete Window?");
                }
            }
            ImGuiHelper.PopButtonColors();
        }
        ImGui.EndGroup();
    }

    private void DrawDeleteConfirmationPopup()
    {
        ImGui.SetNextWindowSize(new Vector2(300, 0));
        if (ImGui.BeginPopupModal("Delete Window?", ref ShowDeleteConfirmation,
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text($"Are you sure you want to delete \"{SelectedWindowToDelete?.Config.Name}\"?");
            ImGui.Text("This action cannot be undone.");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGuiHelper.PushButtonColors(ImGuiHelper.DangerButtonColor);
            if (ImGui.Button("Yes, Delete", new Vector2(120, 0)))
            {
                if (SelectedWindowToDelete != null)
                {
                    RemoveButtonWindow(SelectedWindowToDelete);
                }
                ShowDeleteConfirmation = false;
                ImGui.CloseCurrentPopup();
            }
            ImGuiHelper.PopButtonColors();

            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                ShowDeleteConfirmation = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private void RemoveButtonWindow(ButtonWindow window)
    {
        MainWindow.ButtonWindows.Remove(window);
        WindowSystem.RemoveWindow(window);
        Configuration.Windows.Remove(window.Config);
        window.Dispose();

        if (SelectedWindowToDelete == window)
        {
            SelectedWindowToDelete = null;
        }

        Configuration.Save();
    }

    public void DrawWindowBasicSettings(ButtonWindow window)
    {
        // Check if we need to apply a name change from previous frame
        if (needToApplyNameChange && editingWindowName != null)
        {
            ApplyWindowNameChange(window, editingWindowName);
            editingWindowName = null;
            needToApplyNameChange = false;
        }

        // Window Name
        string windowName = window.Config.Name;
        ImGui.SetNextItemWidth(300);

        // Use a custom ID for this input to help maintain focus
        string inputId = $"WindowName_{window.Config.Name}";

        // Check for focus state changes
        bool hasFocus = ImGui.IsItemActive();

        // Input text field
        if (ImGui.InputText("Window Name", ref windowName, 32))
        {
            // Only store the new name, don't apply it immediately
            editingWindowName = windowName;
        }

        // Check if the input lost focus with Enter key or clicking away
        if (editingWindowName != null && (ImGui.IsItemDeactivatedAfterEdit() ||
            (ImGui.IsKeyPressed(ImGuiKey.Enter) && hasFocus)))
        {
            needToApplyNameChange = true;
        }

        ImGui.Spacing();

        // Window Options
        bool isVisible = window.Config.IsVisible;
        if (ImGui.Checkbox("Show Window", ref isVisible))
        {
            window.Config.IsVisible = isVisible;
            window.IsOpen = isVisible;
            Configuration.Save();
        }

        ImGui.SameLine(200);
        bool isLocked = window.Config.IsLocked;
        if (ImGui.Checkbox("Lock Position", ref isLocked))
        {
            window.Config.IsLocked = isLocked;
            Configuration.Save();
        }

        ImGui.SameLine(400);
        bool transparent = window.Config.TransparentBackground;
        if (ImGui.Checkbox("Transparent Background", ref transparent))
        {
            window.Config.TransparentBackground = transparent;
            Configuration.Save();
        }
    }

    private void ApplyWindowNameChange(ButtonWindow window, string newName)
    {
        if (newName != window.Config.Name)
        {
            bool wasVisible = window.IsOpen;
            WindowSystem.RemoveWindow(window);
            MainWindow.ButtonWindows.Remove(window);
            window.Config.Name = newName;
            var newWindow = new ButtonWindow(Plugin, window.Config) { IsOpen = wasVisible };
            MainWindow.ButtonWindows.Add(newWindow);
            WindowSystem.AddWindow(newWindow);

            // Set the active tab to show the renamed window
            MainWindow.ActiveTabName = newName;

            Configuration.Save();
        }
    }
}
