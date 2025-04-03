using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using WahButtons.Windows;
using Dalamud.Game.ClientState.Conditions;
using WahButtons.Helpers;

namespace WahButtons;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration Configuration;
    public List<ButtonWindow> ButtonWindows = new();
    private WindowSystem WindowSystem;
    private ButtonWindow? SelectedWindowToDelete = null;
    private bool ShowDeleteConfirmation = false;
    private int SelectedButtonIndex = -1;
    private ConditionFlag SelectedCondition = ConditionFlag.None;
    private bool ConditionExpectedState = true;
    private Configuration.RuleAction SelectedRuleAction = Configuration.RuleAction.Hide;

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
        ImGui.Text("Manage Wah Buttons");
        ImGui.Separator();

        // Show advanced features button
        if (ImGui.Button("Advanced Features", new Vector2(200, 0)))
        {
            foreach (var window in WindowSystem.Windows)
            {
                if (window is AdvancedWindow advancedWindow)
                {
                    advancedWindow.IsOpen = true;
                    break;
                }
            }
        }

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
            var newButton = new Configuration.ButtonData("New Button", "/command", 75);
            window.Config.Buttons.Add(newButton);
            Configuration.Save();
            
            // Open the edit popup or SmartButtonRules window right away
            SelectedButtonIndex = window.Config.Buttons.Count - 1;
            ImGui.OpenPopup($"EditButton{window.Config.Buttons.Count - 1}");
        }

        ImGui.Separator();
        
        // Show button controls
        if (ImGui.BeginTabBar("ButtonTabBar"))
        {
            if (ImGui.BeginTabItem("Button Settings"))
            {
                DrawButtonListAndSettings(window);
                ImGui.EndTabItem();
            }
            
            if (ImGui.BeginTabItem("Condition Rules"))
            {
                DrawConditionRules(window);
                ImGui.EndTabItem();
            }
            
            ImGui.EndTabBar();
        }
    }
    
    private void DrawButtonListAndSettings(ButtonWindow window)
    {
        for (int i = 0; i < window.Config.Buttons.Count; i++)
        {
            var button = window.Config.Buttons[i];
            
            ImGui.PushID($"Button_{i}");
            
            // Show smart button indicator
            if (button.IsSmartButton)
            {
                ImGui.TextColored(new Vector4(0.2f, 0.8f, 0.2f, 1.0f), "[Smart]");
                ImGui.SameLine();
            }
            
            // Display button label
            ImGui.Text($"{i + 1}. {button.Label}");
            
            ImGui.SameLine();
            if (ImGui.SmallButton($"Edit##{i}"))
            {
                // Always open the regular edit popup
                ImGui.OpenPopup($"EditButton{i}");
            }

            if (ImGui.BeginPopup($"EditButton{i}"))
            {
                DrawEditButtonPopup(window, button);
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            if (ImGui.SmallButton($"Del##{i}"))
            {
                window.Config.Buttons.RemoveAt(i);
                Configuration.Save();
                i--; // Adjust the index after removal
                ImGui.PopID();
                continue;
            }

            ImGui.SameLine();
            if (i > 0 && ImGui.SmallButton($"↑##{i}"))
            {
                SwapButtons(window.Config.Buttons, i, i - 1);
                Configuration.Save();
            }

            ImGui.SameLine();
            if (i < window.Config.Buttons.Count - 1 && ImGui.SmallButton($"↓##{i}"))
            {
                SwapButtons(window.Config.Buttons, i, i + 1);
                Configuration.Save();
            }

            ImGui.PopID();
        }
    }
    
    private void DrawConditionRules(ButtonWindow window)
    {
        ImGui.Text("Configure smart button conditions");
        ImGui.Separator();
        
        // Select a button to configure
        ImGui.Text("Select Button:");
        ImGui.SetNextItemWidth(200);
        
        if (ImGui.BeginCombo("##SelectButtonForRules", 
            SelectedButtonIndex >= 0 && SelectedButtonIndex < window.Config.Buttons.Count 
                ? window.Config.Buttons[SelectedButtonIndex].Label 
                : "Select Button"))
        {
            for (int i = 0; i < window.Config.Buttons.Count; i++)
            {
                var button = window.Config.Buttons[i];
                if (ImGui.Selectable($"{button.Label}##{i}", SelectedButtonIndex == i))
                {
                    SelectedButtonIndex = i;
                }
            }
            ImGui.EndCombo();
        }
        
        if (SelectedButtonIndex >= 0 && SelectedButtonIndex < window.Config.Buttons.Count)
        {
            var button = window.Config.Buttons[SelectedButtonIndex];
            
            // Toggle smart button feature
            bool isSmartButton = button.IsSmartButton;
            if (ImGui.Checkbox("Is Smart Button", ref isSmartButton))
            {
                button.IsSmartButton = isSmartButton;
                if (isSmartButton && button.ConditionRules.Count == 0)
                {
                    // Add a default rule if becoming a smart button
                    button.ConditionRules.Add(new Configuration.ButtonConditionRule(
                        ConditionFlag.InCombat, true, Configuration.RuleAction.Hide));
                }
                Configuration.Save();
            }
            
            if (button.IsSmartButton)
            {
                ImGui.Separator();
                
                // Display existing rules
                if (button.ConditionRules.Count > 0)
                {
                    ImGui.Text("Current Rules:");
                    
                    if (ImGui.BeginTable("RulesTable", 4, ImGuiTableFlags.Borders))
                    {
                        ImGui.TableSetupColumn("Condition");
                        ImGui.TableSetupColumn("Expected State");
                        ImGui.TableSetupColumn("Action");
                        ImGui.TableSetupColumn("Remove");
                        ImGui.TableHeadersRow();
                        
                        for (int i = 0; i < button.ConditionRules.Count; i++)
                        {
                            var rule = button.ConditionRules[i];
                            
                            ImGui.TableNextRow();
                            
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text(ConditionHelper.GetConditionDescription(rule.Flag));
                            
                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text(rule.ExpectedState ? "Active" : "Inactive");
                            
                            ImGui.TableSetColumnIndex(2);
                            ImGui.Text(rule.Action.ToString());
                            
                            ImGui.TableSetColumnIndex(3);
                            ImGui.PushID($"RemoveRule_{i}");
                            if (ImGui.SmallButton("X"))
                            {
                                button.ConditionRules.RemoveAt(i);
                                Configuration.Save();
                                i--; // Adjust index
                            }
                            ImGui.PopID();
                        }
                        
                        ImGui.EndTable();
                    }
                }
                
                ImGui.Separator();
                
                // Add new rule
                ImGui.Text("Add New Rule:");
                
                // Condition selection
                ImGui.Text("Condition:");
                ImGui.SetNextItemWidth(200);
                if (ImGui.BeginCombo("##ConditionSelection", ConditionHelper.GetConditionDescription(SelectedCondition)))
                {
                    foreach (var flag in ConditionHelper.GetAllConditionFlags())
                    {
                        if (ImGui.Selectable(ConditionHelper.GetConditionDescription(flag), SelectedCondition == flag))
                        {
                            SelectedCondition = flag;
                        }
                    }
                    ImGui.EndCombo();
                }
                
                // Expected state
                ImGui.Text("Expected State:");
                if (ImGui.RadioButton("Active", ConditionExpectedState))
                {
                    ConditionExpectedState = true;
                }
                ImGui.SameLine();
                if (ImGui.RadioButton("Inactive", !ConditionExpectedState))
                {
                    ConditionExpectedState = false;
                }
                
                // Action to take
                ImGui.Text("Action:");
                if (ImGui.RadioButton("Hide", SelectedRuleAction == Configuration.RuleAction.Hide))
                {
                    SelectedRuleAction = Configuration.RuleAction.Hide;
                }
                ImGui.SameLine();
                if (ImGui.RadioButton("Disable", SelectedRuleAction == Configuration.RuleAction.Disable))
                {
                    SelectedRuleAction = Configuration.RuleAction.Disable;
                }
                ImGui.SameLine();
                if (ImGui.RadioButton("Change Color", SelectedRuleAction == Configuration.RuleAction.ChangeColor))
                {
                    SelectedRuleAction = Configuration.RuleAction.ChangeColor;
                }
                
                // Add rule button
                if (ImGui.Button("Add Rule"))
                {
                    var newRule = new Configuration.ButtonConditionRule(
                        SelectedCondition, 
                        ConditionExpectedState,
                        SelectedRuleAction);
                    
                    button.ConditionRules.Add(newRule);
                    Configuration.Save();
                }
            }
        }
    }

    private void DrawEditButtonPopup(ButtonWindow window, Configuration.ButtonData button)
    {
        ImGui.Text("Edit Button");
        ImGui.Separator();

        string label = button.Label;
        if (ImGui.InputText("Label", ref label, 100))
        {
            button.Label = label;
            Configuration.Save();
        }

        // Smart button toggle
        bool isSmartButton = button.IsSmartButton;
        if (ImGui.Checkbox("Smart Button", ref isSmartButton))
        {
            button.IsSmartButton = isSmartButton;
            Configuration.Save();
        }
        
        if (button.IsSmartButton)
        {
            ImGui.SameLine();
            if (ImGui.Button("Configure Rules"))
            {
                // Close this popup
                ImGui.CloseCurrentPopup();
                
                // Open the SmartButtonRulesWindow
                var smartWindow = new SmartButtonRulesWindow(Plugin, window, button);
                WindowSystem.AddWindow(smartWindow);
                smartWindow.IsOpen = true;
            }
            
            ImGui.TextWrapped("Smart buttons can run different commands based on game conditions.");
        }
        
        string command = button.Command;
        if (ImGui.InputText("Command", ref command, 100))
        {
            button.Command = command;
            Configuration.Save();
        }

        // Aetheryte Quick Selection
        if (ImGui.BeginCombo("Aetheryte Teleport", "Select Aetheryte"))
        {
            // Show major city aetherytes first
            var majorAetherytes = new Dictionary<uint, string>
            {
                { 1, "Ul'dah" },
                { 2, "Limsa Lominsa" },
                { 3, "Gridania" },
                { 70, "Ishgard" },
                { 98, "Rhalgr's Reach" },
                { 111, "Kugane" },
                { 133, "The Crystarium" },
                { 144, "Old Sharlayan" },
                { 145, "Radz-at-Han" }
            };

            foreach (var aetheryte in majorAetherytes)
            {
                if (ImGui.Selectable($"{aetheryte.Value} (ID: {aetheryte.Key})"))
                {
                    button.Command = $"/teleport {aetheryte.Key}";
                    Configuration.Save();
                    ImGui.CloseCurrentPopup();
                }
            }

            if (ImGui.Selectable("Browse All Aetherytes..."))
            {
                // Open the Aetheryte window
                foreach (var w in WindowSystem.Windows)
                {
                    if (w is AetheryteWindow aetheryteWindow)
                    {
                        aetheryteWindow.IsOpen = true;
                        break;
                    }
                }
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndCombo();
        }

        float width = button.Width;
        if (ImGui.InputFloat("Width", ref width))
        {
            button.Width = Math.Max(10, width);
            Configuration.Save();
        }

        float height = button.Height;
        if (ImGui.InputFloat("Height", ref height))
        {
            button.Height = Math.Max(10, height);
            Configuration.Save();
        }

        // Color picker
        Vector4 color = button.Color;
        if (ImGui.ColorEdit4("Button Color", ref color))
        {
            button.Color = color;
            Configuration.Save();
        }

        // Label color picker
        Vector4 labelColor = button.LabelColor;
        if (ImGui.ColorEdit4("Label Color", ref labelColor))
        {
            button.LabelColor = labelColor;
            Configuration.Save();
        }
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
}
