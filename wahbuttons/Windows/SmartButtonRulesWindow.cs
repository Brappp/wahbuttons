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
        
        public SmartButtonRulesWindow(Plugin plugin, ButtonWindow parentWindow, Configuration.ButtonData button)
            : base("Smart Button Rules##SmartButtonRules", ImGuiWindowFlags.AlwaysAutoResize)
        {
            Plugin = plugin;
            CurrentButton = button;
            ParentWindow = parentWindow;
            
            Size = new Vector2(700, 600);
            SizeCondition = ImGuiCond.FirstUseEver;
        }

        public void Dispose() { }

        public override void Draw()
        {
            DrawHeader();
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
                DrawRuleConfigurationPanel();
                
                ImGui.EndTable();
            }
        }
        
        private void DrawHeader()
        {
            ImGui.Text($"Configure Smart Button: {CurrentButton.Label}");
            ImGui.TextWrapped("Smart buttons can run different commands based on game conditions. Create rules to determine when to use alternative commands.");
            
            // Basic button preview
            ImGui.Separator();
            ImGui.Text("Button Preview:");
            
            ImGui.PushStyleColor(ImGuiCol.Button, CurrentButton.Color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(
                Math.Min(CurrentButton.Color.X * 1.2f, 1.0f),
                Math.Min(CurrentButton.Color.Y * 1.2f, 1.0f),
                Math.Min(CurrentButton.Color.Z * 1.2f, 1.0f),
                CurrentButton.Color.W));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(
                Math.Min(CurrentButton.Color.X * 0.8f, 1.0f),
                Math.Min(CurrentButton.Color.Y * 0.8f, 1.0f),
                Math.Min(CurrentButton.Color.Z * 0.8f, 1.0f),
                CurrentButton.Color.W));
            
            ImGui.Button(CurrentButton.Label, new Vector2(CurrentButton.Width, CurrentButton.Height));
            ImGui.PopStyleColor(3);
            
            ImGui.Text($"Default Command: {CurrentButton.Command}");
            
            if (CurrentButton.AdvancedRules.Count > 0)
            {
                int ruleCount = CurrentButton.AdvancedRules.Sum(r => r.Conditions.Count);
                ImGui.TextColored(new Vector4(0, 0.8f, 0, 1), $"This button has {CurrentButton.AdvancedRules.Count} rule groups with {ruleCount} total conditions.");
            }
        }
        
        private void DrawRuleGroupsPanel()
        {
            if (ImGui.BeginChild("RuleGroupsChild", new Vector2(0, 450), false))
            {
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
                
                ImGui.EndChild();
            }
            
            // Rename group popup
            bool renameOpen = true;
            if (ImGui.BeginPopupModal("RenameGroupPopup", ref renameOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (CurrentGroup != null)
                {
                    string groupName = CurrentGroup.Name;
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
        
        private void DrawRuleConfigurationPanel()
        {
            if (CurrentGroup == null)
            {
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "Select or create a rule group to configure.");
                return;
            }
            
            if (ImGui.BeginChild("RuleConfigChild", new Vector2(0, 450), false))
            {
                // Group operator (AND/OR)
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
                
                ImGui.Separator();
                
                // List all conditions
                ImGui.Text("Conditions:");
                
                if (CurrentGroup.Conditions.Count == 0)
                {
                    ImGui.TextColored(new Vector4(1, 1, 0, 1), "No conditions defined for this rule group.");
                    ImGui.TextWrapped("Add conditions to define when this rule should activate.");
                }
                else
                {
                    if (ImGui.BeginTable("ConditionsTable", 3, ImGuiTableFlags.Borders))
                    {
                        ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 120);
                        ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("##Actions", ImGuiTableColumnFlags.WidthFixed, 80);
                        ImGui.TableHeadersRow();
                        
                        for (int i = 0; i < CurrentGroup.Conditions.Count; i++)
                        {
                            var condition = CurrentGroup.Conditions[i];
                            
                            ImGui.TableNextRow();
                            
                            // Type column
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text(condition.Type.ToString());
                            
                            // Condition details column
                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text(FormatConditionDescription(condition));
                            
                            // Actions column
                            ImGui.TableSetColumnIndex(2);
                            ImGui.PushID($"cond_action_{i}");
                            
                            if (ImGui.SmallButton("Edit"))
                            {
                                EditingCondition = condition;
                                SelectedConditionIndex = i;
                                ImGui.OpenPopup("EditConditionPopup");
                            }
                            
                            ImGui.SameLine();
                            
                            if (ImGui.SmallButton("Del"))
                            {
                                CurrentGroup.Conditions.RemoveAt(i);
                                Plugin.Configuration.Save();
                                i--;
                            }
                            
                            ImGui.PopID();
                        }
                        
                        ImGui.EndTable();
                    }
                }
                
                ImGui.Separator();
                
                // Add condition section
                ImGui.Text("Add New Condition:");
                
                string[] conditionTypes = {
                    "Game Condition",
                    "Player Level",
                    "Player Job",
                    "Current Zone",
                    "Time of Day"
                };
                
                int typeIndex = (int)NewConditionType;
                ImGui.SetNextItemWidth(200);
                if (ImGui.Combo("Condition Type", ref typeIndex, conditionTypes, conditionTypes.Length))
                {
                    NewConditionType = (Configuration.ConditionType)typeIndex;
                }
                
                DrawConditionInputs();
                
                if (ImGui.Button("Add Condition", new Vector2(150, 0)))
                {
                    AddNewCondition();
                }
                
                ImGui.Separator();
                
                // THEN section - Focus on command changes
                ImGui.Text("THEN");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0, 0.8f, 0, 1), "run this command instead:");
                
                // Always set to ChangeCommand for simplicity
                CurrentGroup.Action = Configuration.RuleAction.ChangeCommand;
                
                ImGui.Text("Alternative Command:");
                ImGui.SetNextItemWidth(350);
                string altCommand = CurrentGroup.AlternateCommand;
                if (ImGui.InputText("##AltCommand", ref altCommand, 100))
                {
                    CurrentGroup.AlternateCommand = altCommand;
                    Plugin.Configuration.Save();
                }
                
                // Add a preview section
                ImGui.Separator();
                ImGui.TextColored(new Vector4(0.9f, 0.6f, 0.1f, 1), "Command Preview:");
                ImGui.BeginTable("CommandPreviewTable", 2, ImGuiTableFlags.Borders);
                
                ImGui.TableSetupColumn("When Rule Applies", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Otherwise", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();
                
                ImGui.TableNextRow();
                
                // When rule applies
                ImGui.TableSetColumnIndex(0);
                ImGui.TextWrapped(string.IsNullOrEmpty(CurrentGroup.AlternateCommand) ? 
                    "(Not set)" : CurrentGroup.AlternateCommand);
                
                // Otherwise (default command)
                ImGui.TableSetColumnIndex(1);
                ImGui.TextWrapped(CurrentButton.Command);
                
                ImGui.EndTable();
                
                ImGui.EndChild();
            }
            
            // Edit condition popup
            bool editCondOpen = true;
            if (EditingCondition != null && ImGui.BeginPopupModal("EditConditionPopup", ref editCondOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Edit Condition");
                ImGui.Separator();
                
                // Cannot change condition type after creation
                ImGui.Text($"Condition Type: {EditingCondition.Type}");
                
                // Type-specific fields
                switch (EditingCondition.Type)
                {
                    case Configuration.ConditionType.GameCondition:
                        DrawGameConditionEdit();
                        break;
                    case Configuration.ConditionType.PlayerLevel:
                        DrawPlayerLevelEdit();
                        break;
                    case Configuration.ConditionType.PlayerJob:
                        DrawPlayerJobEdit();
                        break;
                    case Configuration.ConditionType.CurrentZone:
                        DrawCurrentZoneEdit();
                        break;
                    case Configuration.ConditionType.TimeOfDay:
                        DrawTimeOfDayEdit();
                        break;
                }
                
                ImGui.Separator();
                
                if (ImGui.Button("Save Changes", new Vector2(150, 0)))
                {
                    Plugin.Configuration.Save();
                    EditingCondition = null;
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.SameLine();
                
                if (ImGui.Button("Cancel", new Vector2(100, 0)))
                {
                    EditingCondition = null;
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }
        }
        
        private void DrawConditionInputs()
        {
            switch (NewConditionType)
            {
                case Configuration.ConditionType.GameCondition:
                    ImGui.Text("Condition:");
                    ImGui.SetNextItemWidth(300);
                    if (ImGui.BeginCombo("##ConditionSelection", ConditionHelper.GetConditionDescription(SelectedGameCondition)))
                    {
                        foreach (var flag in ConditionHelper.GetAllConditionFlags())
                        {
                            if (ImGui.Selectable(ConditionHelper.GetConditionDescription(flag), SelectedGameCondition == flag))
                            {
                                SelectedGameCondition = flag;
                            }
                        }
                        ImGui.EndCombo();
                    }
                    
                    ImGui.Text("Expected State:");
                    if (ImGui.RadioButton("Active##GameCond", ExpectedConditionState))
                    {
                        ExpectedConditionState = true;
                    }
                    ImGui.SameLine();
                    if (ImGui.RadioButton("Inactive##GameCond", !ExpectedConditionState))
                    {
                        ExpectedConditionState = false;
                    }
                    break;
                    
                case Configuration.ConditionType.PlayerLevel:
                    ImGui.Text("Level Range:");
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("Min Level", ref MinLevel, 1);
                    MinLevel = Math.Clamp(MinLevel, 1, 90);
                    
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("Max Level", ref MaxLevel, 1);
                    MaxLevel = Math.Clamp(MaxLevel, MinLevel, 90);
                    break;
                    
                case Configuration.ConditionType.PlayerJob:
                    ImGui.Text("Job Name:");
                    ImGui.SetNextItemWidth(150);
                    
                    string[] jobs = {
                        "PLD", "WAR", "DRK", "GNB", // Tanks
                        "WHM", "SCH", "AST", "SGE", // Healers
                        "MNK", "DRG", "NIN", "SAM", "RPR", // Melee DPS
                        "BRD", "MCH", "DNC", // Ranged DPS
                        "BLM", "SMN", "RDM", "BLU", // Casters
                        "DOH", "DOL" // Crafters/Gatherers
                    };
                    
                    int jobIndex = -1;
                    for (int i = 0; i < jobs.Length; i++)
                    {
                        if (jobs[i] == JobName)
                        {
                            jobIndex = i;
                            break;
                        }
                    }
                    
                    if (ImGui.Combo("##JobSelection", ref jobIndex, jobs, jobs.Length))
                    {
                        if (jobIndex >= 0 && jobIndex < jobs.Length)
                        {
                            JobName = jobs[jobIndex];
                        }
                    }
                    break;
                    
                case Configuration.ConditionType.CurrentZone:
                    ImGui.Text("Zone ID:");
                    ImGui.SetNextItemWidth(100);
                    int zoneIdInt = (int)ZoneId;
                    if (ImGui.InputInt("##ZoneId", ref zoneIdInt))
                    {
                        ZoneId = zoneIdInt >= 0 ? (uint)zoneIdInt : 0;
                    }
                    
                    if (ImGui.Button("Current Zone"))
                    {
                        // Get current territory ID
                        ZoneId = Plugin.ClientState.TerritoryType;
                    }
                    break;
                    
                case Configuration.ConditionType.TimeOfDay:
                    ImGui.Text("Hours (0-23):");
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("Start Hour", ref StartHour, 1);
                    StartHour = Math.Clamp(StartHour, 0, 23);
                    
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("End Hour", ref EndHour, 1);
                    EndHour = Math.Clamp(EndHour, 0, 23);
                    break;
            }
        }
        
        private void DrawActionInputs()
        {
            switch (CurrentGroup.Action)
            {
                case Configuration.RuleAction.ChangeColor:
                    ImGui.Text("Alternative Color:");
                    Vector4 altColor = CurrentGroup.AlternateColor;
                    if (ImGui.ColorEdit4("##AltColor", ref altColor))
                    {
                        CurrentGroup.AlternateColor = altColor;
                    }
                    break;
                    
                case Configuration.RuleAction.ChangeCommand:
                    ImGui.Text("Alternative Command:");
                    ImGui.SetNextItemWidth(350);
                    string altCommand = CurrentGroup.AlternateCommand;
                    if (ImGui.InputText("##AltCommand", ref altCommand, 100))
                    {
                        CurrentGroup.AlternateCommand = altCommand;
                    }
                    break;
                    
                case Configuration.RuleAction.ChangeLabel:
                    ImGui.Text("Alternative Label:");
                    ImGui.SetNextItemWidth(250);
                    string altLabel = CurrentGroup.AlternateLabel;
                    if (ImGui.InputText("##AltLabel", ref altLabel, 100))
                    {
                        CurrentGroup.AlternateLabel = altLabel;
                    }
                    break;
            }
        }
        
        private void DrawGameConditionEdit()
        {
            ImGui.Text("Condition:");
            ImGui.SetNextItemWidth(300);
            if (ImGui.BeginCombo("##EditConditionSelection", ConditionHelper.GetConditionDescription(EditingCondition.GameCondition)))
            {
                foreach (var flag in ConditionHelper.GetAllConditionFlags())
                {
                    if (ImGui.Selectable(ConditionHelper.GetConditionDescription(flag), EditingCondition.GameCondition == flag))
                    {
                        EditingCondition.GameCondition = flag;
                    }
                }
                ImGui.EndCombo();
            }
            
            ImGui.Text("Expected State:");
            if (ImGui.RadioButton("Active##EditCond", EditingCondition.ExpectedState))
            {
                EditingCondition.ExpectedState = true;
            }
            ImGui.SameLine();
            if (ImGui.RadioButton("Inactive##EditCond", !EditingCondition.ExpectedState))
            {
                EditingCondition.ExpectedState = false;
            }
        }
        
        private void DrawPlayerLevelEdit()
        {
            ImGui.Text("Level Range:");
            ImGui.SetNextItemWidth(100);
            int minLevel = EditingCondition.MinLevel;
            if (ImGui.InputInt("Min Level", ref minLevel, 1))
            {
                EditingCondition.MinLevel = Math.Clamp(minLevel, 1, 90);
            }
            
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            int maxLevel = EditingCondition.MaxLevel;
            if (ImGui.InputInt("Max Level", ref maxLevel, 1))
            {
                EditingCondition.MaxLevel = Math.Clamp(maxLevel, EditingCondition.MinLevel, 90);
            }
        }
        
        private void DrawPlayerJobEdit()
        {
            ImGui.Text("Job Name:");
            ImGui.SetNextItemWidth(150);
            
            string[] jobs = {
                "PLD", "WAR", "DRK", "GNB", // Tanks
                "WHM", "SCH", "AST", "SGE", // Healers
                "MNK", "DRG", "NIN", "SAM", "RPR", // Melee DPS
                "BRD", "MCH", "DNC", // Ranged DPS
                "BLM", "SMN", "RDM", "BLU", // Casters
                "DOH", "DOL" // Crafters/Gatherers
            };
            
            int jobIndex = -1;
            for (int i = 0; i < jobs.Length; i++)
            {
                if (jobs[i] == EditingCondition.JobName)
                {
                    jobIndex = i;
                    break;
                }
            }
            
            if (ImGui.Combo("##EditJobSelection", ref jobIndex, jobs, jobs.Length))
            {
                if (jobIndex >= 0 && jobIndex < jobs.Length)
                {
                    EditingCondition.JobName = jobs[jobIndex];
                }
            }
        }
        
        private void DrawCurrentZoneEdit()
        {
            ImGui.Text("Zone ID:");
            ImGui.SetNextItemWidth(100);
            int zoneIdInt = (int)EditingCondition.ZoneId;
            if (ImGui.InputInt("##EditZoneId", ref zoneIdInt))
            {
                EditingCondition.ZoneId = zoneIdInt >= 0 ? (uint)zoneIdInt : 0;
            }
            
            if (ImGui.Button("Current Zone"))
            {
                // Get current territory ID
                EditingCondition.ZoneId = Plugin.ClientState.TerritoryType;
            }
        }
        
        private void DrawTimeOfDayEdit()
        {
            ImGui.Text("Hours (0-23):");
            ImGui.SetNextItemWidth(100);
            int startHour = EditingCondition.StartHour;
            if (ImGui.InputInt("Start Hour", ref startHour, 1))
            {
                EditingCondition.StartHour = Math.Clamp(startHour, 0, 23);
            }
            
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            int endHour = EditingCondition.EndHour;
            if (ImGui.InputInt("End Hour", ref endHour, 1))
            {
                EditingCondition.EndHour = Math.Clamp(endHour, 0, 23);
            }
        }
        
        private void AddNewCondition()
        {
            var newCondition = new Configuration.AdvancedCondition
            {
                Type = NewConditionType
            };
            
            switch (NewConditionType)
            {
                case Configuration.ConditionType.GameCondition:
                    newCondition.GameCondition = SelectedGameCondition;
                    newCondition.ExpectedState = ExpectedConditionState;
                    break;
                    
                case Configuration.ConditionType.PlayerLevel:
                    newCondition.MinLevel = MinLevel;
                    newCondition.MaxLevel = MaxLevel;
                    break;
                    
                case Configuration.ConditionType.PlayerJob:
                    newCondition.JobName = JobName;
                    break;
                    
                case Configuration.ConditionType.CurrentZone:
                    newCondition.ZoneId = ZoneId;
                    break;
                    
                case Configuration.ConditionType.TimeOfDay:
                    newCondition.StartHour = StartHour;
                    newCondition.EndHour = EndHour;
                    break;
            }
            
            CurrentGroup.Conditions.Add(newCondition);
            Plugin.Configuration.Save();
        }
        
        private string FormatConditionDescription(Configuration.AdvancedCondition condition)
        {
            switch (condition.Type)
            {
                case Configuration.ConditionType.GameCondition:
                    return $"{ConditionHelper.GetConditionDescription(condition.GameCondition)} is {(condition.ExpectedState ? "Active" : "Inactive")}";
                    
                case Configuration.ConditionType.PlayerLevel:
                    return $"Player level is between {condition.MinLevel} and {condition.MaxLevel}";
                    
                case Configuration.ConditionType.PlayerJob:
                    return $"Current job is {condition.JobName}";
                    
                case Configuration.ConditionType.CurrentZone:
                    return $"Current zone ID is {condition.ZoneId}";
                    
                case Configuration.ConditionType.TimeOfDay:
                    return $"Time is between {condition.StartHour}:00 and {condition.EndHour}:59";
                    
                default:
                    return "Unknown condition";
            }
        }
    }
} 