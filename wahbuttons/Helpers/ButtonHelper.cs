using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using System;

namespace WahButtons.Helpers
{
    public static class ButtonHelper
    {
        // Button size and layout calculations
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

        // Button appearance and styling
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

            // Enhanced visual effects for regular buttons (non-smart buttons)
            if (!button.IsSmartButton)
            {
                return;
            }

            // Check all color change rules
            ApplyColorChangeRules(button);
        }

        // Enhanced color change rule handling
        private static void ApplyColorChangeRules(Configuration.ButtonData button)
        {
            // First check simple condition rules
            foreach (var rule in button.ConditionRules)
            {
                if (rule.Action != Configuration.RuleAction.ChangeColor)
                    continue;

                bool conditionMet = Plugin.Condition[rule.Flag] == rule.ExpectedState;
                if (conditionMet)
                {
                    // Override button color
                    ImGui.PopStyleColor(4);

                    // Apply alternate color with improved visual effects
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
                    return; // Exit after applying the first matching rule
                }
            }

            // Then check advanced rules
            foreach (var ruleGroup in button.AdvancedRules)
            {
                if (ruleGroup.Action != Configuration.RuleAction.ChangeColor)
                    continue;

                bool groupResult = EvaluateRuleGroup(ruleGroup);
                if (groupResult)
                {
                    // Override button color
                    ImGui.PopStyleColor(4);

                    // Apply alternate color with improved visual effects
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
                    return; // Exit after applying the first matching rule
                }
            }
        }

        // Button visibility handling
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

        // Button enabled/disabled state
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
                        
                    case Configuration.ConditionType.CurrentZone:
                        if (Plugin.ClientState == null)
                            return false;
                            
                        return Plugin.ClientState.TerritoryType == condition.ZoneId;
                        
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