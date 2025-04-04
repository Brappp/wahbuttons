using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using WahButtons.Helpers;
using Dalamud.Game.ClientState.Conditions;
using System.Linq;

namespace WahButtons.Windows
{
    public class SmartButtonRulesWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private Configuration.ButtonData CurrentButton;
        private ButtonWindow ParentWindow;
        private int SelectedRuleGroupIndex = -1;
        private int SelectedConditionIndex = -1;
        private Configuration.AdvancedCondition EditingCondition = null;
        private Configuration.AdvancedRuleGroup CurrentGroup = null;
        
        // For adding new conditions
        private Configuration.ConditionType NewConditionType = Configuration.ConditionType.GameCondition;
        private ConditionFlag SelectedGameCondition = ConditionFlag.None;
        private bool ExpectedConditionState = true;
        private int MinLevel = 1;
        private int MaxLevel = 90;
        private string JobName = string.Empty;
        private uint ZoneId = 0;
        private int StartHour = 0;
        private int EndHour = 23;
        
        // Add these fields
        private bool IsZoneSelectorOpen = false;
        private Dictionary<uint, string> ZoneMap = new Dictionary<uint, string>
        {
            { 0, "Unknown Zone" },
            { 128, "Limsa Lominsa" },
            { 129, "Gridania" },
            { 130, "Ul'dah" },
            { 132, "Ishgard" },
            { 133, "The Gold Saucer" },
            { 418, "Kugane" },
            { 819, "Crystarium" },
            { 962, "Old Sharlayan" }
            // Add more zones as needed
        };
        
        public SmartButtonRulesWindow(Plugin plugin, ButtonWindow parentWindow, Configuration.ButtonData button)
            : base("Smart Button Rules##SmartButtonRules", ImGuiWindowFlags.None)
        {
            Plugin = plugin;
            CurrentButton = button;
            ParentWindow = parentWindow;
            
            Size = new Vector2(700, 600);
            SizeCondition = ImGuiCond.FirstUseEver;
        }

        public void Dispose() 
        {
            // Make sure to save any changes when the window is disposed
            Plugin.Configuration.Save();
            
            // Reset state
            CurrentGroup = null;
            SelectedRuleGroupIndex = -1;
            SelectedConditionIndex = -1;
            EditingCondition = null;
        }

        public override void Draw()
        {
            try
            {
                // If the window was closed, remove it from the WindowSystem
                if (!IsOpen)
                {
                    Plugin.WindowSystem.RemoveWindow(this);
                    return;
                }
                
                DrawSimplifiedHeader();
                ImGui.Separator();
                
                if (ImGui.BeginTable("SmartButtonLayout", 2, ImGuiTableFlags.Borders))
                {
                    ImGui.TableSetupColumn("Rule Groups", ImGuiTableColumnFlags.WidthFixed, 200);
                    ImGui.TableSetupColumn("Rule Configuration", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableHeadersRow();
                    
                    // Left panel - Rule Groups
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    DrawRuleGroupsPanel();
                    
                    // Right panel - Rule Configuration
                    ImGui.TableSetColumnIndex(1);
                    DrawSimplifiedRuleConfigurationPanel();
                    
                    ImGui.EndTable();
                }
                
                // Handle popups outside the table structure
                ProcessPopups();
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the plugin
                Plugin.PluginLog.Error($"Error in SmartButtonRulesWindow.Draw: {ex.Message}");
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "An error occurred rendering the window");
            }
        }
        
        private void DrawSimplifiedHeader()
        {
            ImGui.Text($"Configure Smart Button: {CurrentButton.Label}");
            ImGui.TextWrapped("Configure when this button should use an alternate command.");
            
            // Button preview
            ImGui.SameLine(ImGui.GetWindowWidth() - 80);
            ImGui.PushStyleColor(ImGuiCol.Button, CurrentButton.Color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, CurrentButton.Color);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, CurrentButton.Color);
            ImGui.Button(CurrentButton.Label, new Vector2(70, 30));
            ImGui.PopStyleColor(3);
        }
        
        private void DrawSimplifiedRuleConfigurationPanel()
        {
            if (CurrentGroup == null)
            {
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "Select or create a rule group →");
                ImGui.TextWrapped("Each rule group represents a different condition when your button should behave differently.");
                return;
            }
            
            bool childStarted = false;
            try
            {
                if (ImGui.BeginChild("RuleConfigChild", new Vector2(0, 450), false))
                {
                    childStarted = true;
                    
                    // Group operator (AND/OR) - simplified with help text
                    ImGui.Text("IF");
                    ImGui.SameLine();
                    
                    string[] operators = { "ALL conditions are true (AND)", "ANY condition is true (OR)" };
                    int opIndex = CurrentGroup.Operator == Configuration.RuleOperator.And ? 0 : 1;
                    
                    ImGui.SetNextItemWidth(250);
                    if (ImGui.Combo("##GroupOperator", ref opIndex, operators, operators.Length))
                    {
                        CurrentGroup.Operator = opIndex == 0 ? Configuration.RuleOperator.And : Configuration.RuleOperator.Or;
                        Plugin.Configuration.Save();
                    }
                    
                    ImGui.SameLine();
                    ImGui.TextDisabled("(?)");
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextWrapped("AND: All conditions must be true for the rule to apply");
                        ImGui.TextWrapped("OR: Any condition can be true for the rule to apply");
                        ImGui.EndTooltip();
                    }
                    
                    ImGui.Separator();
                    
                    // List all conditions with simplified display
                    ImGui.Text("Conditions:");
                    
                    if (CurrentGroup.Conditions.Count == 0)
                    {
                        ImGui.TextColored(new Vector4(1, 1, 0, 1), "No conditions defined yet.");
                        ImGui.TextWrapped("Add conditions below to define when this rule should activate.");
                    }
                    else
                    {
                        for (int i = 0; i < CurrentGroup.Conditions.Count; i++)
                        {
                            var condition = CurrentGroup.Conditions[i];
                            
                            ImGui.PushID($"cond_{i}");
                            
                            // Condition description with delete button
                            ImGui.Text($"{i+1}. ");
                            ImGui.SameLine();
                            
                            ImGui.TextColored(new Vector4(0.7f, 0.9f, 0.7f, 1f), FormatConditionDescription(condition));
                            
                            ImGui.SameLine(ImGui.GetWindowWidth() - 50);
                            if (ImGui.SmallButton("❌"))
                            {
                                CurrentGroup.Conditions.RemoveAt(i);
                                Plugin.Configuration.Save();
                                i--;
                            }
                            
                            ImGui.PopID();
                        }
                    }
                    
                    ImGui.Separator();
                    
                    // Simplified condition adder
                    ImGui.TextColored(new Vector4(0.3f, 0.7f, 0.9f, 1f), "Add a Condition:");
                    
                    // Two-button approach for condition types
                    float buttonWidth = (ImGui.GetWindowWidth() - 20) / 2;
                    
                    if (ImGui.Button("Game Condition (Combat, Mounted, etc.)", new Vector2(buttonWidth, 0)))
                    {
                        NewConditionType = Configuration.ConditionType.GameCondition;
                        OpenGameConditionPopup();
                        Plugin.PluginLog.Debug("Opening Game Condition popup");
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("Current Zone (Location-based)", new Vector2(buttonWidth, 0)))
                    {
                        NewConditionType = Configuration.ConditionType.CurrentZone;
                        OpenZoneConditionPopup();
                        Plugin.PluginLog.Debug("Opening Zone Condition popup");
                    }
                    
                    ImGui.Separator();
                    
                    // THEN section - Focus on command changes
                    ImGui.TextColored(new Vector4(0, 0.8f, 0, 1), "THEN run this command instead:");
                    
                    // Always set to ChangeCommand for simplicity
                    CurrentGroup.Action = Configuration.RuleAction.ChangeCommand;
                    
                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 20);
                    string altCommand = CurrentGroup.AlternateCommand;
                    if (ImGui.InputText("##AltCommand", ref altCommand, 100))
                    {
                        CurrentGroup.AlternateCommand = altCommand;
                        Plugin.Configuration.Save();
                    }
                    
                    // Add a preview section
                    ImGui.Separator();
                    DrawCommandPreview();
                }
            }
            finally
            {
                if (childStarted)
                {
                    ImGui.EndChild();
                }
            }
        }
        
        private void DrawCommandPreview()
        {
            try
            {
                ImGui.TextColored(new Vector4(0.9f, 0.6f, 0.1f, 1), "Preview:");
                
                if (ImGui.BeginTable("CommandPreviewTable", 1, ImGuiTableFlags.Borders))
                {
                    try
                    {
                        // Header
                        ImGui.TableSetupColumn("Command Behavior", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableHeadersRow();
                        
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        
                        // When rule applies
                        ImGui.TextColored(new Vector4(0, 0.8f, 0, 1), "When conditions are met:");
                        ImGui.SameLine();
                        ImGui.TextWrapped(string.IsNullOrEmpty(CurrentGroup.AlternateCommand) ? 
                            "(Not set)" : CurrentGroup.AlternateCommand);
                        
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        
                        // Otherwise (default command)
                        ImGui.TextColored(new Vector4(0.8f, 0, 0, 1), "Otherwise (default):");
                        ImGui.SameLine();
                        ImGui.TextWrapped(CurrentButton.Command);
                    }
                    finally
                    {
                        // Ensure the table is always closed
                        ImGui.EndTable();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash
                Plugin.PluginLog.Error($"Error in DrawCommandPreview: {ex.Message}");
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Error displaying preview");
            }
        }
        
        private void OpenGameConditionPopup()
        {
            SelectedGameCondition = ConditionFlag.InCombat; // Default selection
            ExpectedConditionState = true; // Default value
            ImGui.OpenPopup("AddGameConditionPopup");
        }
        
        private void OpenZoneConditionPopup()
        {
            ImGui.OpenPopup("AddZoneConditionPopup");
        }
        
        private void ProcessPopups()
        {
            // Game condition popup
            if (ImGui.BeginPopup("AddGameConditionPopup"))
            {
                ImGui.Text("Select Game Condition:");
                
                string[] gameConditions = { 
                    "In Combat", "Mounted", "In Duty", 
                    "In Instance", "Watching Cutscene", "Crafting" 
                };
                
                ConditionFlag[] conditionFlags = {
                    ConditionFlag.InCombat, ConditionFlag.Mounted, ConditionFlag.BoundByDuty,
                    ConditionFlag.BetweenAreas, ConditionFlag.WatchingCutscene, ConditionFlag.Crafting
                };
                
                int selectedCondition = Array.IndexOf(conditionFlags, SelectedGameCondition);
                if (selectedCondition < 0) selectedCondition = 0;
                
                if (ImGui.ListBox("##GameConditions", ref selectedCondition, gameConditions, gameConditions.Length, 6))
                {
                    SelectedGameCondition = conditionFlags[selectedCondition];
                }
                
                ImGui.Text("Condition should be:");
                bool active = ExpectedConditionState;
                if (ImGui.RadioButton("Active (true)", active))
                {
                    ExpectedConditionState = true;
                }
                
                bool inactive = !ExpectedConditionState;
                if (ImGui.RadioButton("Inactive (false)", inactive))
                {
                    ExpectedConditionState = false;
                }
                
                ImGui.Separator();
                
                if (ImGui.Button("Add Condition", new Vector2(120, 0)))
                {
                    if (CurrentGroup != null)
                    {
                        var newCondition = new Configuration.AdvancedCondition
                        {
                            Type = Configuration.ConditionType.GameCondition,
                            GameCondition = SelectedGameCondition,
                            ExpectedState = ExpectedConditionState
                        };
                        
                        CurrentGroup.Conditions.Add(newCondition);
                        Plugin.Configuration.Save();
                        ImGui.CloseCurrentPopup();
                    }
                }
                
                ImGui.SameLine();
                
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }
            
            // Zone condition popup
            if (ImGui.BeginPopup("AddZoneConditionPopup"))
            {
                ImGui.Text("Set Zone Condition:");
                
                if (ImGui.Button("Use Current Zone", new Vector2(200, 0)))
                {
                    if (CurrentGroup != null)
                    {
                        var newCondition = new Configuration.AdvancedCondition
                        {
                            Type = Configuration.ConditionType.CurrentZone,
                            ZoneId = Plugin.ClientState.TerritoryType
                        };
                        
                        CurrentGroup.Conditions.Add(newCondition);
                        Plugin.Configuration.Save();
                        ImGui.CloseCurrentPopup();
                    }
                }
                
                ImGui.Separator();
                ImGui.Text("Or select from common zones:");
                
                if (ImGui.BeginChild("ZoneSelector", new Vector2(300, 200), true))
                {
                    foreach (var zone in ZoneMap)
                    {
                        if (ImGui.Selectable(zone.Value))
                        {
                            if (CurrentGroup != null)
                            {
                                var newCondition = new Configuration.AdvancedCondition
                                {
                                    Type = Configuration.ConditionType.CurrentZone,
                                    ZoneId = zone.Key
                                };
                                
                                CurrentGroup.Conditions.Add(newCondition);
                                Plugin.Configuration.Save();
                                ImGui.CloseCurrentPopup();
                            }
                        }
                    }
                    ImGui.EndChild();
                }
                
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }
        }
        
        private void DrawRuleGroupsPanel()
        {
            bool childStarted = false;
            try
            {
                if (ImGui.BeginChild("RuleGroupsChild", new Vector2(0, 450), false))
                {
                    childStarted = true;
                    ImGui.Text("Rule Groups:");
                    ImGui.Separator();
                    
                    if (CurrentButton.AdvancedRules.Count == 0)
                    {
                        ImGui.TextColored(new Vector4(1, 1, 0, 1), "No rule groups defined.");
                        ImGui.TextWrapped("Click 'Add Rule Group' to create your first rule group.");
                    }
                    else
                    {
                        // Display all rule groups
                        for (int i = 0; i < CurrentButton.AdvancedRules.Count; i++)
                        {
                            var group = CurrentButton.AdvancedRules[i];
                            bool isSelected = i == SelectedRuleGroupIndex;
                            
                            // Create a unique ID for this group
                            ImGui.PushID($"group_{i}");
                            
                            // Show group name with condition count
                            if (ImGui.Selectable($"{group.Name} ({group.Conditions.Count} conditions)", isSelected))
                            {
                                SelectedRuleGroupIndex = i;
                                CurrentGroup = group;
                                SelectedConditionIndex = -1;
                            }
                            
                            // Context menu for renaming or deleting
                            if (ImGui.BeginPopupContextItem())
                            {
                                if (ImGui.MenuItem("Rename"))
                                {
                                    SelectedRuleGroupIndex = i;
                                    CurrentGroup = group;
                                    ImGui.OpenPopup("RenameGroupPopup");
                                }
                                
                                if (ImGui.MenuItem("Delete"))
                                {
                                    CurrentButton.AdvancedRules.RemoveAt(i);
                                    if (SelectedRuleGroupIndex == i)
                                    {
                                        SelectedRuleGroupIndex = -1;
                                        CurrentGroup = null;
                                    }
                                    else if (SelectedRuleGroupIndex > i)
                                    {
                                        SelectedRuleGroupIndex--;
                                    }
                                    Plugin.Configuration.Save();
                                }
                                
                                ImGui.EndPopup();
                            }
                            
                            ImGui.PopID();
                        }
                    }
                    
                    ImGui.Separator();
                    
                    // Add new rule group button
                    if (ImGui.Button("Add Rule Group", new Vector2(150, 0)))
                    {
                        var newGroup = new Configuration.AdvancedRuleGroup
                        {
                            Name = $"Rule Group {CurrentButton.AdvancedRules.Count + 1}"
                        };
                        
                        CurrentButton.AdvancedRules.Add(newGroup);
                        SelectedRuleGroupIndex = CurrentButton.AdvancedRules.Count - 1;
                        CurrentGroup = newGroup;
                        Plugin.Configuration.Save();
                    }
                }
            }
            finally
            {
                if (childStarted)
                {
                    ImGui.EndChild();
                }
            }
            
            // Rename group popup - FIXED: Don't use BeginPopupModal, use regular BeginPopup instead
            if (ImGui.BeginPopup("RenameGroupPopup"))
            {
                if (CurrentGroup != null)
                {
                    ImGui.Text("Rename Group");
                    ImGui.Separator();
                    
                    string groupName = CurrentGroup.Name;
                    ImGui.SetNextItemWidth(250);
                    if (ImGui.InputText("Group Name", ref groupName, 100))
                    {
                        CurrentGroup.Name = groupName;
                    }
                    
                    if (ImGui.Button("Save", new Vector2(120, 0)))
                    {
                        Plugin.Configuration.Save();
                        ImGui.CloseCurrentPopup();
                    }
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("Cancel", new Vector2(120, 0)))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }
                
                ImGui.EndPopup();
            }
        }
        
        private string FormatConditionDescription(Configuration.AdvancedCondition condition)
        {
            switch (condition.Type)
            {
                case Configuration.ConditionType.GameCondition:
                    return $"{ConditionHelper.GetConditionDescription(condition.GameCondition)} is {(condition.ExpectedState ? "Active" : "Inactive")}";
                    
                case Configuration.ConditionType.CurrentZone:
                    return $"Current zone ID is {condition.ZoneId}";
                    
                default:
                    return "Unknown condition";
            }
        }
    }
} 