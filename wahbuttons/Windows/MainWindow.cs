using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using WahButtons.Windows;

namespace WahButtons
{
    public class MainWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private Configuration Configuration;
        public List<ButtonWindow> ButtonWindows = new();
        private ButtonWindow? SelectedWindowToDelete = null;
        private bool ShowDeleteConfirmation = false;

        public MainWindow(Plugin plugin, Configuration configuration, WindowSystem windowSystem)
            : base("Wah Buttons##Main") // Explicitly unique ID for MainWindow
        {
            Plugin = plugin;
            Configuration = configuration;

            // Load ButtonWindows from configuration
            foreach (var config in configuration.Windows)
            {
                AddButtonWindowFromConfig(config);
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

            // Add Window Button
            if (ImGui.Button("Add Window"))
            {
                var newConfig = new Configuration.ButtonWindowConfig
                {
                    Name = "Window " + (ButtonWindows.Count + 1),
                    IsVisible = true
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

            // Confirmation Dialog for Deletion
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

            bool isVisible = window.Config.IsVisible;
            if (ImGui.Checkbox("Show", ref isVisible))
            {
                window.Config.IsVisible = isVisible;
                window.IsOpen = isVisible;
                Configuration.Save();
            }

            bool isLocked = window.Config.IsLocked;
            if (ImGui.Checkbox("Lock Position", ref isLocked))
            {
                window.Config.IsLocked = isLocked;
                Configuration.Save();
            }

            bool transparent = window.Config.TransparentBackground;
            if (ImGui.Checkbox("Transparent Background", ref transparent))
            {
                window.Config.TransparentBackground = transparent;
                Configuration.Save();
            }

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
        }

        private void AddButtonWindowFromConfig(Configuration.ButtonWindowConfig config)
        {
            var window = new ButtonWindow(Plugin, config)
            {
                IsOpen = config.IsVisible
            };
            ButtonWindows.Add(window);
        }

        private void RemoveButtonWindow(ButtonWindow window)
        {
            ButtonWindows.Remove(window);
            Configuration.Windows.Remove(window.Config);
            Configuration.Save();
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
}
