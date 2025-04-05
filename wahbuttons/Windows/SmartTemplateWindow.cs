using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using WahButtons.Helpers;

namespace WahButtons.Windows
{
    public class SmartTemplateWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private Configuration.ButtonData TargetButton;
        private string SelectedCategory = "All";
        private int SelectedTemplateIndex = -1;
        
        public SmartTemplateWindow(Plugin plugin, Configuration.ButtonData button)
            : base("Smart Button Templates##SmartTemplateSelector", ImGuiWindowFlags.AlwaysAutoResize)
        {
            Plugin = plugin;
            TargetButton = button;
            
            Size = new Vector2(650, 500);
            SizeCondition = ImGuiCond.FirstUseEver;
        }
        
        public void Dispose() { }
        
        public override void Draw()
        {
            // Header with explanation
            ImGui.TextColored(new Vector4(0.3f, 0.7f, 0.9f, 1.0f), "Smart Button Templates");
            ImGui.TextWrapped("Templates provide pre-configured sets of rules for common smart button use cases.");
            ImGui.Separator();
            
            // Two panel layout
            if (ImGui.BeginTable("TemplateLayout", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Categories", ImGuiTableColumnFlags.WidthFixed, 150);
                ImGui.TableSetupColumn("Templates", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();
                
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                DrawCategoriesList();
                
                ImGui.TableSetColumnIndex(1);
                DrawTemplatesList();
                
                ImGui.EndTable();
            }
            
            // Buttons at the bottom
            DrawFooter();
        }
        
        private void DrawCategoriesList()
        {
            if (ImGui.BeginChild("CategoriesChild", new Vector2(0, 350), false))
            {
                string[] categories = new[] { "All", "Combat", "Duty", "Movement", "Social", "Crafting" };
                
                foreach (var category in categories)
                {
                    bool isSelected = category == SelectedCategory;
                    
                    ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.2f, 0.4f, 0.6f, 0.7f));
                    ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.3f, 0.5f, 0.7f, 0.7f));
                    ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.4f, 0.6f, 0.8f, 0.7f));
                    
                    if (ImGui.Selectable(category, isSelected))
                    {
                        SelectedCategory = category;
                        SelectedTemplateIndex = -1;
                    }
                    
                    ImGui.PopStyleColor(3);
                }
                
                ImGui.EndChild();
            }
        }
        
        private void DrawTemplatesList()
        {
            if (ImGui.BeginChild("TemplatesChild", new Vector2(0, 350), false))
            {
                var templates = SmartButtonTemplates.GetTemplatesByCategory(SelectedCategory);
                
                if (templates.Count == 0)
                {
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "No templates in this category");
                }
                else
                {
                    // Header
                    ImGui.TextColored(new Vector4(0.3f, 0.7f, 0.9f, 1.0f), 
                        $"{SelectedCategory} Templates ({templates.Count})");
                    ImGui.Separator();
                    
                    // Templates list with descriptions
                    for (int i = 0; i < templates.Count; i++)
                    {
                        var template = templates[i];
                        bool isSelected = i == SelectedTemplateIndex;
                        
                        // Push styling for the selectable
                        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.2f, 0.4f, 0.6f, 0.7f));
                        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.3f, 0.5f, 0.7f, 0.7f));
                        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.4f, 0.6f, 0.8f, 0.7f));
                        
                        if (ImGui.Selectable($"{template.Name}##template_{i}", isSelected))
                        {
                            SelectedTemplateIndex = i;
                        }
                        
                        ImGui.PopStyleColor(3);
                        
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text(template.Name);
                            ImGui.Separator();
                            ImGui.TextWrapped(template.Description);
                            ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), 
                                $"Rules: {template.Rules.Count}");
                            ImGui.EndTooltip();
                        }
                        
                        // Display description under each template
                        ImGui.TextWrapped(template.Description);
                        
                        // Show a preview if selected
                        if (isSelected)
                        {
                            ImGui.Indent(10);
                            
                            // Add a visual cue
                            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.2f, 0.3f, 0.3f));
                            if (ImGui.BeginChild($"Preview_{i}", new Vector2(ImGui.GetContentRegionAvail().X - 10, 100), true))
                            {
                                ImGui.TextColored(new Vector4(0.3f, 0.7f, 0.9f, 1.0f), "Rule Preview:");
                                
                                foreach (var rule in template.Rules)
                                {
                                    string logicOp = rule.Operator == Configuration.RuleOperator.And ? "ALL" : "ANY";
                                    
                                    ImGui.TextWrapped($"IF {logicOp} of {rule.Conditions.Count} condition(s) match:");
                                    ImGui.Indent(20);
                                    
                                    foreach (var condition in rule.Conditions)
                                    {
                                        switch (condition.Type)
                                        {
                                            case Configuration.ConditionType.GameCondition:
                                                string condDesc = ConditionHelper.GetConditionDescription(condition.GameCondition);
                                                ImGui.TextWrapped($"- {condDesc} is {(condition.ExpectedState ? "active" : "inactive")}");
                                                break;
                                                
                                            case Configuration.ConditionType.CurrentZone:
                                                ImGui.TextWrapped($"- In zone with ID {condition.ZoneId}");
                                                break;
                                        }
                                    }
                                    
                                    ImGui.Unindent(20);
                                    ImGui.TextWrapped($"THEN run: {rule.AlternateCommand}");
                                    
                                    ImGui.Separator();
                                }
                                
                                ImGui.EndChild();
                            }
                            ImGui.PopStyleColor();
                            
                            ImGui.Unindent(10);
                        }
                        
                        ImGui.Separator();
                    }
                }
                
                ImGui.EndChild();
            }
        }
        
        private void DrawFooter()
        {
            // Tutorial text
            if (SelectedTemplateIndex == -1)
            {
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), 
                    "Select a template from the list to view and apply it");
            }
            
            // Apply button
            ImGui.BeginDisabled(SelectedTemplateIndex == -1);
            
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.7f, 0.4f, 1.0f));
            
            if (ImGui.Button("Apply Template", new Vector2(200, 30)))
            {
                var templates = SmartButtonTemplates.GetTemplatesByCategory(SelectedCategory);
                if (SelectedTemplateIndex >= 0 && SelectedTemplateIndex < templates.Count)
                {
                    // Apply the template
                    SmartButtonTemplates.ApplyTemplate(templates[SelectedTemplateIndex], TargetButton);
                    
                    // Save configuration
                    Plugin.Configuration.Save();
                    
                    // Show confirmation popup
                    ImGui.OpenPopup("TemplateAppliedPopup");
                }
            }
            
            ImGui.PopStyleColor(2);
            ImGui.EndDisabled();
            
            ImGui.SameLine();
            
            if (ImGui.Button("Cancel", new Vector2(120, 30)))
            {
                IsOpen = false;
            }
            
            // Confirmation popup
            bool templateAppliedPopupOpen = true;
            if (ImGui.BeginPopupModal("TemplateAppliedPopup", ref templateAppliedPopupOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                var templates = SmartButtonTemplates.GetTemplatesByCategory(SelectedCategory);
                if (SelectedTemplateIndex >= 0 && SelectedTemplateIndex < templates.Count)
                {
                    ImGui.Text($"Template '{templates[SelectedTemplateIndex].Name}' applied successfully!");
                }
                else
                {
                    ImGui.Text("Template applied successfully!");
                }
                
                ImGui.TextWrapped("The smart button has been configured with the selected template rules.");
                ImGui.Separator();
                
                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                    IsOpen = false;
                }
                
                ImGui.EndPopup();
            }
        }
    }
} 