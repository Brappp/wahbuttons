using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using WahButtons.Windows;
using WahButtons.Helpers;
using System.Linq;

namespace WahButtons.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration Configuration;
    public List<ButtonWindow> ButtonWindows = new();
    private WindowSystem WindowSystem;
    private ButtonWindow? SelectedWindowToDelete = null;
    private bool ShowDeleteConfirmation = false;
    private int SelectedButtonIndex = -1;

    public MainWindow(Plugin plugin, Configuration configuration, WindowSystem windowSystem)
        : base("Wah Buttons##Main")
    {
        Plugin = plugin;
        Configuration = configuration;
        WindowSystem = windowSystem;

        // Load ButtonWindows from configuration
        foreach (var config in Configuration.Windows)
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
        // Create a header bar with consistent styling and layout
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.2f, 0.2f, 0.5f));
        ImGui.BeginChild("GlobalControls", new Vector2(ImGui.GetWindowWidth(), 50), false);
        
        // Center buttons in the bar
        float totalWidth = 330; // Total width of all buttons plus spacing
        float startPosX = (ImGui.GetWindowWidth() - totalWidth) / 2;
        ImGui.SetCursorPosX(startPosX);
        
        // Add Window - primary action button
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.5f, 0.7f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 0.8f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.7f, 0.9f, 1.0f));
        
        if (ImGui.Button("Add Window", new Vector2(100, 30)))
        {
            AddButtonWindow();
        }
        ImGui.PopStyleColor(3);
        
        ImGui.SameLine(0, 15);
        
        // Advanced Features button
        if (ImGui.Button("Advanced", new Vector2(100, 30)))
        {
            foreach (var window in Plugin.WindowSystem.Windows)
            {
                if (window is AdvancedWindow advWindow)
                {
                    advWindow.IsOpen = true;
                    break;
                }
            }
        }
        
        ImGui.SameLine(0, 15);
        
        // Help button
        if (ImGui.Button("Help", new Vector2(100, 30)))
        {
            foreach (var window in Plugin.WindowSystem.Windows)
            {
                if (window is HelpWindow helpWindow)
                {
                    helpWindow.IsOpen = true;
                    break;
                }
            }
        }
        
        ImGui.EndChild();
        ImGui.PopStyleColor();
        
        // Add a separator below the header bar
        ImGui.Separator();
        
        // Draw the window tabs - now after the global controls
        if (ImGui.BeginTabBar("WindowsTabBar"))
        {
            // Window Tabs - one tab per button window config
            foreach (var windowConfig in Configuration.Windows)
            {
                if (ImGui.BeginTabItem(windowConfig.Name + "##Tab"))
                {
                    // Contents of the tab for each window
                    DrawWindowConfigTab(windowConfig);
                    ImGui.EndTabItem();
                }
            }
            
            ImGui.EndTabBar();
        }
    }

    private void DrawWindowConfigTab(Configuration.ButtonWindowConfig windowConfig)
    {
        // Window Settings Section
        if (ImGui.CollapsingHeader("Window Settings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Apply background color for better visibility 
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.15f, 0.15f, 0.15f, 0.5f));
            ImGui.BeginChild("WindowSettings", new Vector2(ImGui.GetWindowWidth() - 20, 110), true);
            
            // Window Name - Full width single field
            string windowName = windowConfig.Name;
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() * 0.7f);
            if (ImGui.InputText("Window Name", ref windowName, 100))
            {
                windowConfig.Name = windowName;
                Configuration.Save();
                
                // Find and update the actual window
                foreach (var window in WindowSystem.Windows)
                {
                    if (window is ButtonWindow btnWindow && btnWindow.Config == windowConfig)
                    {
                        window.WindowName = windowConfig.Name + "##" + Guid.NewGuid();
                        break;
                    }
                }
            }
            
            ImGui.Separator();
            
            // Create a grid layout for checkboxes and layout settings
            if (ImGui.BeginTable("SettingsGrid", 2, ImGuiTableFlags.None))
            {
                // Column 1: Checkboxes
                ImGui.TableNextColumn();
                
                // Checkbox group with better alignment
                ImGui.BeginGroup();
                ImGui.Text("Window Properties:");
                
                bool isVisible = windowConfig.IsVisible;
                if (ImGui.Checkbox("Visible", ref isVisible))
                {
                    windowConfig.IsVisible = isVisible;
                    Configuration.Save();
                }
                
                bool isLocked = windowConfig.IsLocked;
                if (ImGui.Checkbox("Locked Position", ref isLocked))
                {
                    windowConfig.IsLocked = isLocked;
                    Configuration.Save();
                }
                
                bool transparentBg = windowConfig.TransparentBackground;
                if (ImGui.Checkbox("Transparent Background", ref transparentBg))
                {
                    windowConfig.TransparentBackground = transparentBg;
                    Configuration.Save();
                }
                ImGui.EndGroup();
                
                // Column 2: Layout settings
                ImGui.TableNextColumn();
                
                ImGui.BeginGroup();
                ImGui.Text("Layout Settings:");
                
                // Layout Type
                ImGui.SetNextItemWidth(120);
                string[] layoutTypes = { "Vertical", "Horizontal", "Grid" };
                int layoutIndex = (int)windowConfig.Layout;
                if (ImGui.Combo("Layout", ref layoutIndex, layoutTypes, layoutTypes.Length))
                {
                    windowConfig.Layout = (Configuration.ButtonLayout)layoutIndex;
                    Configuration.Save();
                }
                
                // Grid settings (only show if grid layout is selected)
                if (windowConfig.Layout == Configuration.ButtonLayout.Grid)
                {
                    ImGui.SetNextItemWidth(60);
                    int rows = windowConfig.GridRows;
                    if (ImGui.InputInt("Rows", ref rows, 1))
                    {
                        windowConfig.GridRows = Math.Max(1, rows);
                        Configuration.Save();
                    }
                    
                    ImGui.SameLine(120);
                    ImGui.SetNextItemWidth(60);
                    int cols = windowConfig.GridColumns;
                    if (ImGui.InputInt("Columns", ref cols, 1))
                    {
                        windowConfig.GridColumns = Math.Max(1, cols);
                        Configuration.Save();
                    }
                }
                ImGui.EndGroup();
                
                ImGui.EndTable();
            }
            
            ImGui.EndChild();
            ImGui.PopStyleColor();
        }
        
        // Buttons Section
        if (ImGui.CollapsingHeader("Buttons", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Apply background color for better visibility
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.15f, 0.15f, 0.15f, 0.5f));
            ImGui.BeginChild("ButtonsList", new Vector2(ImGui.GetWindowWidth() - 20, 250), true);
            
            // Button list
            if (windowConfig.Buttons.Count == 0)
            {
                // Centered "No buttons" message
                float windowWidth = ImGui.GetWindowWidth();
                float textWidth = ImGui.CalcTextSize("No buttons added yet.").X;
                ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
                
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "No buttons added yet.");
                
                textWidth = ImGui.CalcTextSize("Click 'Add Button' below to create your first button.").X;
                ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
                ImGui.TextWrapped("Click 'Add Button' below to create your first button.");
            }
            else
            {
                // Table for buttons with improved styling
                ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, new Vector4(0.2f, 0.2f, 0.4f, 1.0f));
                if (ImGui.BeginTable("ButtonsTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Smart", ImGuiTableColumnFlags.WidthFixed, 50);
                    ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 120);
                    ImGui.TableHeadersRow();
                    ImGui.PopStyleColor();
                    
                    for (int i = 0; i < windowConfig.Buttons.Count; i++)
                    {
                        var button = windowConfig.Buttons[i];
                        ImGui.TableNextRow();
                        
                        // Label column
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(button.Label);
                        
                        // Command column
                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextWrapped(button.Command);
                        
                        // Smart button column
                        ImGui.TableSetColumnIndex(2);
                        if (button.IsSmartButton)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0.8f, 0, 1));
                            ImGui.Text("✓");
                            ImGui.PopStyleColor();
                        }
                        
                        // Actions column
                        ImGui.TableSetColumnIndex(3);
                        
                        // Need a unique ID for each button's actions
                        ImGui.PushID($"actions_{windowConfig.Name}_{i}");
                        
                        // Edit button
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.5f, 0.7f, 1.0f));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 0.8f, 1.0f));
                        if (ImGui.Button("Edit", new Vector2(50, 20)))
                        {
                            // Store the button index and open the popup directly here
                            ImGui.OpenPopup($"EditButton_{windowConfig.Name}_{i}");
                        }
                        ImGui.PopStyleColor(2);
                        
                        ImGui.SameLine();
                        
                        // Delete button
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f, 0.3f, 0.3f, 1.0f));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.4f, 0.4f, 1.0f));
                        if (ImGui.Button("Delete", new Vector2(60, 20)))
                        {
                            ImGui.OpenPopup($"DeleteButtonConfirm_{windowConfig.Name}_{i}");
                        }
                        ImGui.PopStyleColor(2);
                        
                        // Handle the delete confirmation popup (no longer using BeginPopupModal)
                        if (ImGui.BeginPopup($"DeleteButtonConfirm_{windowConfig.Name}_{i}"))
                        {
                            ImGui.Text($"Delete button \"{button.Label}\"?");
                            ImGui.Separator();
                            
                            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f, 0.3f, 0.3f, 1.0f));
                            if (ImGui.Button("Delete", new Vector2(120, 0)))
                            {
                                // Remove the button directly
                                windowConfig.Buttons.RemoveAt(i);
                                Plugin.Configuration.Save();
                                ImGui.CloseCurrentPopup();
                            }
                            ImGui.PopStyleColor();
                            
                            ImGui.SameLine();
                            
                            if (ImGui.Button("Cancel", new Vector2(120, 0)))
                            {
                                ImGui.CloseCurrentPopup();
                            }
                            
                            ImGui.EndPopup();
                        }
                        
                        // Handle the edit popup directly inside the loop
                        if (ImGui.BeginPopup($"EditButton_{windowConfig.Name}_{i}"))
                        {
                            DrawButtonEditor(windowConfig, button);
                            ImGui.EndPopup();
                        }
                        
                        ImGui.PopID();
                    }
                    
                    ImGui.EndTable();
                }
            }
            
            ImGui.EndChild();
            ImGui.PopStyleColor();
            
            // Controls below the button list with improved styling
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.7f, 0.4f, 1.0f));
            if (ImGui.Button("Add Button", new Vector2(120, 30)))
            {
                AddNewButton(windowConfig);
            }
            ImGui.PopStyleColor(2);
            
            ImGui.SameLine();
            
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.5f, 0.7f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.6f, 0.8f, 1.0f));
            if (ImGui.Button("Add Smart Button", new Vector2(120, 30)))
            {
                AddNewSmartButton(windowConfig);
            }
            ImGui.PopStyleColor(2);
            
            ImGui.SameLine(ImGui.GetWindowWidth() - 160); // Position delete window button to the right
            
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f, 0.2f, 0.2f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.8f, 0.3f, 0.3f, 1.0f));
            if (ImGui.Button("Delete Window", new Vector2(140, 30)))
            {
                ImGui.OpenPopup("ConfirmDeleteWindow");
            }
            ImGui.PopStyleColor(2);
            
            // Delete window confirmation popup
            bool confirmOpen = true;
            if (ImGui.BeginPopupModal("ConfirmDeleteWindow", ref confirmOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.TextColored(new Vector4(1, 0.5f, 0.5f, 1), "Warning!");
                ImGui.Text($"Are you sure you want to delete window '{windowConfig.Name}'?");
                ImGui.Text("This action cannot be undone.");
                ImGui.Separator();
                
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f, 0.2f, 0.2f, 1.0f));
                if (ImGui.Button("Delete", new Vector2(120, 0)))
                {
                    DeleteButtonWindow(windowConfig);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.PopStyleColor();
                
                ImGui.SameLine();
                
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }
        }
    }

    private void AddButtonWindow()
    {
        var newConfig = new Configuration.ButtonWindowConfig
        {
            Name = "Window " + (Configuration.Windows.Count + 1),
            IsVisible = true,
            Layout = Configuration.ButtonLayout.Grid,
            GridRows = 2,
            GridColumns = 2
        };

        Configuration.Windows.Add(newConfig);
        AddButtonWindowFromConfig(newConfig);
        Configuration.Save();
    }

    private void AddButtonWindowFromConfig(Configuration.ButtonWindowConfig config)
    {
        var buttonWindow = new ButtonWindow(Plugin, config);
        ButtonWindows.Add(buttonWindow);
        WindowSystem.AddWindow(buttonWindow);
    }

    private void RemoveButtonWindow(ButtonWindow window)
    {
        WindowSystem.RemoveWindow(window);
        ButtonWindows.Remove(window);
        Configuration.Windows.Remove(window.Config);
        Configuration.Save();
    }

    private void SwapButtons(List<Configuration.ButtonData> buttons, int indexA, int indexB)
    {
        var temp = buttons[indexA];
        buttons[indexA] = buttons[indexB];
        buttons[indexB] = temp;
    }

    public void SaveAllButtonWindows()
    {
        Configuration.Save();
    }

    // Modify the PreDraw method to handle the new popup IDs
    public override void PreDraw()
    {
        base.PreDraw();
        
        // We're now handling all popups directly in the rendering loop
        // No need to process them again here
    }

    private void DrawButtonEditor(Configuration.ButtonWindowConfig windowConfig, Configuration.ButtonData button)
    {
        ImGui.Text("Edit Button");
        ImGui.Separator();
        
        // Basic settings
        string label = button.Label;
        if (ImGui.InputText("Button Label", ref label, 100))
        {
            button.Label = label;
            Plugin.Configuration.Save();
        }
        
        string command = button.Command;
        if (ImGui.InputText("Command", ref command, 100))
        {
            button.Command = command;
            Plugin.Configuration.Save();
        }
        
        ImGui.Separator();
        
        // Smart button settings
        bool isSmartButton = button.IsSmartButton;
        if (ImGui.Checkbox("Smart Button", ref isSmartButton))
        {
            button.IsSmartButton = isSmartButton;
            Plugin.Configuration.Save();
        }
        
        if (button.IsSmartButton)
        {
            ImGui.SameLine();
            if (ImGui.Button("Configure Rules"))
            {
                // Find the button window for this config
                ButtonWindow? targetWindow = null;
                foreach (var window in Plugin.WindowSystem.Windows)
                {
                    if (window is ButtonWindow btnWindow && btnWindow.Config == windowConfig)
                    {
                        targetWindow = btnWindow;
                        break;
                    }
                }
                
                if (targetWindow != null)
                {
                    // Open the SmartButtonRulesWindow
                    var smartWindow = new SmartButtonRulesWindow(Plugin, targetWindow, button);
                    
                    // Remove any existing SmartButtonRulesWindow first
                    foreach (var window in Plugin.WindowSystem.Windows.ToList())
                    {
                        if (window is SmartButtonRulesWindow)
                        {
                            Plugin.WindowSystem.RemoveWindow(window);
                        }
                    }
                    
                    Plugin.WindowSystem.AddWindow(smartWindow);
                    smartWindow.IsOpen = true;
                    
                    // Close the edit popup
                    ImGui.CloseCurrentPopup();
                }
            }
        }
        
        ImGui.Separator();
        
        // Button appearance
        float width = button.Width;
        if (ImGui.InputFloat("Width", ref width, 5))
        {
            button.Width = Math.Max(10, width);
            Plugin.Configuration.Save();
        }
        
        float height = button.Height;
        if (ImGui.InputFloat("Height", ref height, 5))
        {
            button.Height = Math.Max(10, height);
            Plugin.Configuration.Save();
        }
        
        // Add color pickers
        Vector4 color = button.Color;
        if (ImGui.ColorEdit4("Button Color", ref color))
        {
            button.Color = color;
            Plugin.Configuration.Save();
        }
        
        Vector4 labelColor = button.LabelColor;
        if (ImGui.ColorEdit4("Label Color", ref labelColor))
        {
            button.LabelColor = labelColor;
            Plugin.Configuration.Save();
        }
    }

    private void AddNewButton(Configuration.ButtonWindowConfig windowConfig)
    {
        var newButton = new Configuration.ButtonData
        {
            Label = "New Button",
            Command = "/echo Button clicked!",
            Width = 75,
            Height = 30,
            Color = new Vector4(0.26f, 0.59f, 0.98f, 1f),
            LabelColor = new Vector4(1f, 1f, 1f, 1f)
        };
        
        // Add the new button to the configuration
        windowConfig.Buttons.Add(newButton);
        Plugin.Configuration.Save();
        
        // Note: We no longer need to open an edit popup here
        // The user can click the Edit button to make changes after it's added
    }

    private void AddNewSmartButton(Configuration.ButtonWindowConfig windowConfig)
    {
        var newButton = new Configuration.ButtonData
        {
            Label = "Smart Button",
            Command = "/echo Smart button clicked!",
            Width = 75,
            Height = 30,
            Color = new Vector4(0.2f, 0.7f, 0.4f, 1f), // Green color for smart buttons
            LabelColor = new Vector4(1f, 1f, 1f, 1f),
            IsSmartButton = true
        };
        
        // Add the new button to the configuration
        windowConfig.Buttons.Add(newButton);
        Plugin.Configuration.Save();
        
        // Find the button window for this config
        ButtonWindow? targetWindow = null;
        foreach (var window in Plugin.WindowSystem.Windows)
        {
            if (window is ButtonWindow btnWindow && btnWindow.Config == windowConfig)
            {
                targetWindow = btnWindow;
                break;
            }
        }
        
        if (targetWindow != null)
        {
            // Open the SmartButtonRulesWindow for the newly created button
            var smartWindow = new SmartButtonRulesWindow(Plugin, targetWindow, newButton);
            
            // Remove any existing SmartButtonRulesWindow first
            foreach (var window in Plugin.WindowSystem.Windows.ToList())
            {
                if (window is SmartButtonRulesWindow)
                {
                    Plugin.WindowSystem.RemoveWindow(window);
                }
            }
            
            Plugin.WindowSystem.AddWindow(smartWindow);
            smartWindow.IsOpen = true;
        }
    }

    private void DeleteButtonWindow(Configuration.ButtonWindowConfig windowConfig)
    {
        // Find and remove the actual window
        Window? windowToRemove = null;
        foreach (var window in Plugin.WindowSystem.Windows)
        {
            if (window is ButtonWindow btnWindow && btnWindow.Config == windowConfig)
            {
                windowToRemove = window;
                break;
            }
        }
        
        if (windowToRemove != null)
        {
            Plugin.WindowSystem.RemoveWindow(windowToRemove);
        }
        
        // Remove from the configuration
        Plugin.Configuration.Windows.Remove(windowConfig);
        Plugin.Configuration.Save();
    }
}

