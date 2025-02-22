using System;
using System.Numerics;
using System.Linq;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using WahButtons.Windows;
using WahButtons.Helpers;

namespace WahButtons.UI.Components
{
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
            // Keep the layout simple with just the controls
            DrawWindowCreation();

            if (ShowDeleteConfirmation)
            {
                DrawDeleteConfirmationPopup();
            }
        }

        private void DrawWindowCreation()
        {
            // Keep add window and delete window on a single line, arranged side by side
            ImGui.BeginGroup();
            {
                // New window name input
                ImGui.SetNextItemWidth(140);
                if (ImGui.InputText("##NewWindowName", ref NewWindowName, 32))
                {
                    // Name is updated
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Enter name for new window");
                }

                ImGui.SameLine();
                if (ImGui.Button("Add Window", new Vector2(100, 0)))
                {
                    CreateNewWindow();
                }

                // Leave some space between add and delete
                ImGui.SameLine(280);

                // Window selection dropdown
                ImGui.SetNextItemWidth(140);
                if (ImGui.BeginCombo("##WindowSelect", SelectedWindowToDelete?.Config.Name ?? "Select Window"))
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
                if (ImGui.Button("Delete", new Vector2(80, 0)))
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

            // Removed the unnecessary separator
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

            // Create the window via plugin
            var newWindow = Plugin.CreateButtonWindow(newConfig);

            // Set the active tab to the new window
            MainWindow.SetActiveTab(windowName);

            // Save and reset name
            Configuration.Save();
            NewWindowName = "New Window";
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
            // Remove the window from configuration
            Configuration.Windows.Remove(window.Config);

            // Remove the window via plugin
            Plugin.RemoveButtonWindow(window);

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

            ImGui.SameLine(190);
            bool isLocked = window.Config.IsLocked;
            if (ImGui.Checkbox("Lock Position", ref isLocked))
            {
                window.Config.IsLocked = isLocked;
                Configuration.Save();
            }

            ImGui.SameLine(350);
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
                // Create a new window with the new name
                var oldConfig = window.Config;

                // Create a copy of the config with the new name
                var newConfig = new Configuration.ButtonWindowConfig
                {
                    Name = newName,
                    IsVisible = oldConfig.IsVisible,
                    IsLocked = oldConfig.IsLocked,
                    TransparentBackground = oldConfig.TransparentBackground,
                    Layout = oldConfig.Layout,
                    Buttons = oldConfig.Buttons,
                    Position = oldConfig.Position,
                    Size = oldConfig.Size,
                    GridRows = oldConfig.GridRows,
                    GridColumns = oldConfig.GridColumns,
                    ExpandingColumns = oldConfig.ExpandingColumns,
                    IsExpanded = oldConfig.IsExpanded,
                    MainButtonIndex = oldConfig.MainButtonIndex,
                    ExpansionDirection = oldConfig.ExpansionDirection,
                    ActiveTab = oldConfig.ActiveTab,
                    Tabs = oldConfig.Tabs,
                    TabActiveColor = oldConfig.TabActiveColor,
                    TabHoverColor = oldConfig.TabHoverColor
                };

                // Remove old window from configuration and system
                Configuration.Windows.Remove(oldConfig);
                Plugin.RemoveButtonWindow(window);

                // Add new window to configuration and system
                Configuration.Windows.Add(newConfig);
                var newWindow = Plugin.CreateButtonWindow(newConfig);

                // Set the active tab to the new window
                MainWindow.SetActiveTab(newName);

                Configuration.Save();
            }
        }
    }
}
