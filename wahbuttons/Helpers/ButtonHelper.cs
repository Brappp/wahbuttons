using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using System;

namespace WahButtons.Helpers
{
    public static class ButtonHelper
    {
        public static float CalculateDefaultButtonWidth(Vector2 containerSize, int columns)
        {
            return containerSize.X / columns - 10;
        }

        public static float CalculateDefaultButtonHeight(Vector2 containerSize, int rows)
        {
            return containerSize.Y / rows - 10;
        }

        public static float GetButtonWidth(Configuration.ButtonData button, float defaultWidth)
        {
            return button.Width > 0 ? button.Width : defaultWidth;
        }

        public static float GetButtonHeight(Configuration.ButtonData button, float defaultHeight)
        {
            return button.Height > 0 ? button.Height : defaultHeight;
        }

        public static void ApplyButtonStyles(Configuration.ButtonData button)
        {
            // Push default styles
            ImGui.PushStyleColor(ImGuiCol.Button, button.Color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(
                Math.Min(button.Color.X * 1.2f, 1.0f),
                Math.Min(button.Color.Y * 1.2f, 1.0f),
                Math.Min(button.Color.Z * 1.2f, 1.0f),
                button.Color.W));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(
                Math.Min(button.Color.X * 0.8f, 1.0f),
                Math.Min(button.Color.Y * 0.8f, 1.0f),
                Math.Min(button.Color.Z * 0.8f, 1.0f),
                button.Color.W));
            ImGui.PushStyleColor(ImGuiCol.Text, button.LabelColor);
            
            // Check for color change rules
            if (button.IsSmartButton)
            {
                foreach (var rule in button.ConditionRules)
                {
                    if (rule.Action != Configuration.RuleAction.ChangeColor)
                        continue;
                        
                    bool conditionMet = Plugin.Condition[rule.Flag] == rule.ExpectedState;
                    if (conditionMet)
                    {
                        // Override button color
                        ImGui.PopStyleColor(4);
                        ImGui.PushStyleColor(ImGuiCol.Button, rule.AlternateColor);
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(
                            Math.Min(rule.AlternateColor.X * 1.2f, 1.0f),
                            Math.Min(rule.AlternateColor.Y * 1.2f, 1.0f),
                            Math.Min(rule.AlternateColor.Z * 1.2f, 1.0f),
                            rule.AlternateColor.W));
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(
                            Math.Min(rule.AlternateColor.X * 0.8f, 1.0f),
                            Math.Min(rule.AlternateColor.Y * 0.8f, 1.0f),
                            Math.Min(rule.AlternateColor.Z * 0.8f, 1.0f),
                            rule.AlternateColor.W));
                        ImGui.PushStyleColor(ImGuiCol.Text, button.LabelColor);
                        break;
                    }
                }
                
                // Check advanced rules
                foreach (var ruleGroup in button.AdvancedRules)
                {
                    if (ruleGroup.Action != Configuration.RuleAction.ChangeColor)
                        continue;
                        
                    bool groupResult = EvaluateRuleGroup(ruleGroup);
                    if (groupResult)
                    {
                        // Override button color
                        ImGui.PopStyleColor(4);
                        ImGui.PushStyleColor(ImGuiCol.Button, ruleGroup.AlternateColor);
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(
                            Math.Min(ruleGroup.AlternateColor.X * 1.2f, 1.0f),
                            Math.Min(ruleGroup.AlternateColor.Y * 1.2f, 1.0f),
                            Math.Min(ruleGroup.AlternateColor.Z * 1.2f, 1.0f),
                            ruleGroup.AlternateColor.W));
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(
                            Math.Min(ruleGroup.AlternateColor.X * 0.8f, 1.0f),
                            Math.Min(ruleGroup.AlternateColor.Y * 0.8f, 1.0f),
                            Math.Min(ruleGroup.AlternateColor.Z * 0.8f, 1.0f),
                            ruleGroup.AlternateColor.W));
                        ImGui.PushStyleColor(ImGuiCol.Text, button.LabelColor);
                        break;
                    }
                }
            }
        }

        public static bool ShouldRenderButton(Configuration.ButtonData button)
        {
            if (!button.IsSmartButton)
                return true;
                
            // Skip rendering if any rule says to hide
            foreach (var rule in button.ConditionRules)
            {
                if (rule.Action != Configuration.RuleAction.Hide)
                    continue;
                    
                bool conditionMet = Plugin.Condition[rule.Flag] == rule.ExpectedState;
                if (conditionMet)
                    return false;
            }
            
            // Check advanced rules
            foreach (var ruleGroup in button.AdvancedRules)
            {
                if (ruleGroup.Action != Configuration.RuleAction.Hide)
                    continue;
                    
                bool groupResult = EvaluateRuleGroup(ruleGroup);
                if (groupResult)
                    return false;
            }
            
            return true;
        }

        public static bool IsButtonEnabled(Configuration.ButtonData button)
        {
            if (!button.IsSmartButton)
                return true;
                
            // Check for disable actions
            foreach (var rule in button.ConditionRules)
            {
                if (rule.Action != Configuration.RuleAction.Disable)
                    continue;
                    
                bool conditionMet = Plugin.Condition[rule.Flag] == rule.ExpectedState;
                if (conditionMet)
                    return false;
            }
            
            // Check advanced rules
            foreach (var ruleGroup in button.AdvancedRules)
            {
                if (ruleGroup.Action != Configuration.RuleAction.Disable)
                    continue;
                    
                bool groupResult = EvaluateRuleGroup(ruleGroup);
                if (groupResult)
                    return false;
            }
            
            return true;
        }

        public static Vector4 GetButtonColor(Configuration.ButtonData button)
        {
            if (!button.IsSmartButton || button.ConditionRules.Count == 0)
                return button.Color;

            Vector4 color = button.Color;
            
            foreach (var rule in button.ConditionRules)
            {
                bool conditionState = ConditionHelper.IsConditionActive(rule.Flag);
                
                // If condition state doesn't match the expected state for this rule
                if (conditionState != rule.ExpectedState)
                {
                    // If the action is to change the color
                    if (rule.Action == Configuration.RuleAction.ChangeColor)
                    {
                        // Make the button more transparent/faded
                        color.W = 0.5f;
                    }
                }
            }
            
            return color;
        }

        public static string GetButtonLabel(Configuration.ButtonData button)
        {
            if (!button.IsSmartButton)
                return button.Label;
                
            // Check for label change rules
            foreach (var ruleGroup in button.AdvancedRules)
            {
                if (ruleGroup.Action != Configuration.RuleAction.ChangeLabel)
                    continue;
                    
                bool groupResult = EvaluateRuleGroup(ruleGroup);
                if (groupResult && !string.IsNullOrEmpty(ruleGroup.AlternateLabel))
                {
                    return ruleGroup.AlternateLabel;
                }
            }
            
            return button.Label;
        }
        
        public static string GetButtonCommand(Configuration.ButtonData button)
        {
            if (!button.IsSmartButton)
                return button.Command;
                
            // Check each rule group for command changes
            foreach (var ruleGroup in button.AdvancedRules)
            {
                // Only process ChangeCommand rules
                if (ruleGroup.Action != Configuration.RuleAction.ChangeCommand)
                    continue;
                    
                // Skip empty commands
                if (string.IsNullOrEmpty(ruleGroup.AlternateCommand))
                    continue;
                    
                // Evaluate if this rule group's conditions are met
                bool groupResult = EvaluateRuleGroup(ruleGroup);
                if (groupResult)
                {
                    // Use the alternate command from the first matching rule group
                    return ruleGroup.AlternateCommand;
                }
            }
            
            // If no matching rules, use the default command
            return button.Command;
        }
        
        private static bool EvaluateRuleGroup(Configuration.AdvancedRuleGroup group)
        {
            if (group.Conditions.Count == 0)
                return false;
                
            bool result = group.Operator == Configuration.RuleOperator.And;
            
            foreach (var condition in group.Conditions)
            {
                bool conditionResult = EvaluateCondition(condition);
                
                if (group.Operator == Configuration.RuleOperator.And)
                {
                    // AND - If any condition is false, the result is false
                    if (!conditionResult)
                        return false;
                }
                else
                {
                    // OR - If any condition is true, the result is true
                    if (conditionResult)
                        return true;
                }
            }
            
            return result;
        }
        
        private static bool EvaluateCondition(Configuration.AdvancedCondition condition)
        {
            try
            {
                switch (condition.Type)
                {
                    case Configuration.ConditionType.GameCondition:
                        return Plugin.Condition[condition.GameCondition] == condition.ExpectedState;
                        
                    case Configuration.ConditionType.PlayerLevel:
                        if (Plugin.ClientState == null || !Plugin.ClientState.IsLoggedIn)
                            return false;
                            
                        int playerLevel = Plugin.ClientState.LocalPlayer?.Level ?? 0;
                        return playerLevel >= condition.MinLevel && playerLevel <= condition.MaxLevel;
                        
                    case Configuration.ConditionType.PlayerJob:
                        if (Plugin.ClientState == null || !Plugin.ClientState.IsLoggedIn || Plugin.ClientState.LocalPlayer == null)
                            return false;
                        
                        // For now, we'll just use a simplified check
                        // In a real implementation we would map job IDs to abbreviations
                        return condition.JobName == "ANY"; // Just a placeholder check
                        
                    case Configuration.ConditionType.CurrentZone:
                        if (Plugin.ClientState == null)
                            return false;
                            
                        return Plugin.ClientState.TerritoryType == condition.ZoneId;
                        
                    case Configuration.ConditionType.TimeOfDay:
                        int currentHour = DateTime.Now.Hour;
                        return currentHour >= condition.StartHour && currentHour <= condition.EndHour;
                        
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error evaluating condition: {ex.Message}");
                return false;
            }
        }
    }
} 