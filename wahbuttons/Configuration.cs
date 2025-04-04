using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Conditions;
using Newtonsoft.Json;

namespace WahButtons;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public List<ButtonWindowConfig> Windows { get; set; } = new();
    public bool ShowConditionWindow { get; set; } = false;
    
    // Backup configuration
    public bool AutoBackupEnabled { get; set; } = true;
    public int BackupRetentionDays { get; set; } = 7;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    [Serializable]
    public class ButtonWindowConfig
    {
        public string Name { get; set; } = "New Window";
        public bool IsVisible { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public bool TransparentBackground { get; set; } = false;
        public ButtonLayout Layout { get; set; } = ButtonLayout.Vertical;
        public List<ButtonData> Buttons { get; set; } = new();
        public Vector2 Position { get; set; } = new Vector2(100, 100);
        public Vector2 Size { get; set; } = new Vector2(300, 200);

        // Grid-specific settings
        public int GridRows { get; set; } = 6;
        public int GridColumns { get; set; } = 6;
    }

    [Serializable]
    public class ButtonData
    {
        public string Label { get; set; }
        public string Command { get; set; }
        public float Width { get; set; } = 75;
        public float Height { get; set; } = 30;
        public Vector4 Color { get; set; } = new Vector4(0.26f, 0.59f, 0.98f, 1f);
        public Vector4 LabelColor { get; set; } = new Vector4(1f, 1f, 1f, 1f);
        
        // Smart button properties
        public bool IsSmartButton { get; set; } = false;
        public List<ButtonConditionRule> ConditionRules { get; set; } = new();

        // Advanced rules
        public List<AdvancedRuleGroup> AdvancedRules { get; set; } = new();

        public ButtonData(string label, string command, float width)
        {
            Label = label;
            Command = command;
            Width = width;
        }

        public ButtonData() { }
    }

    [Serializable]
    public class ButtonConditionRule
    {
        public ConditionFlag Flag { get; set; }
        public bool ExpectedState { get; set; } = true; // true = condition must be active, false = condition must be inactive
        public RuleAction Action { get; set; } = RuleAction.Hide;

        // For ChangeCommand action
        public string AlternateCommand { get; set; } = string.Empty;
        
        // For ChangeLabel action
        public string AlternateLabel { get; set; } = string.Empty;
        
        // For ChangeColor action
        public Vector4 AlternateColor { get; set; } = new Vector4(1, 0, 0, 1);

        public ButtonConditionRule() { }

        public ButtonConditionRule(ConditionFlag flag, bool expectedState, RuleAction action)
        {
            Flag = flag;
            ExpectedState = expectedState;
            Action = action;
        }
    }

    public enum ButtonLayout
    {
        Vertical,
        Horizontal,
        Grid
    }

    public enum RuleAction
    {
        Hide,       // Hide the button
        Disable,    // Disable the button
        ChangeColor,   // Change button color
        ChangeCommand, // Change the command
        ChangeLabel    // Change the button label
    }

    // Smart Button Rules
    public enum RuleOperator
    {
        And,
        Or
    }

    public enum ConditionType
    {
        GameCondition,
        PlayerLevel,
        PlayerJob,
        CurrentZone,
        TimeOfDay
    }

    [Serializable]
    public class AdvancedCondition
    {
        public ConditionType Type { get; set; } = ConditionType.GameCondition;
        public ConditionFlag GameCondition { get; set; } = ConditionFlag.None; // For Type == GameCondition
        public bool ExpectedState { get; set; } = true; // For Type == GameCondition
        public int MinLevel { get; set; } = 0; // For Type == PlayerLevel
        public int MaxLevel { get; set; } = 90; // For Type == PlayerLevel
        public string JobName { get; set; } = string.Empty; // For Type == PlayerJob
        public uint ZoneId { get; set; } = 0; // For Type == CurrentZone
        public int StartHour { get; set; } = 0; // For Type == TimeOfDay
        public int EndHour { get; set; } = 23; // For Type == TimeOfDay
    }
    
    [Serializable]
    public class AdvancedRuleGroup
    {
        public List<AdvancedCondition> Conditions { get; set; } = new();
        public RuleOperator Operator { get; set; } = RuleOperator.And;
        public RuleAction Action { get; set; } = RuleAction.Hide;
        public string AlternateCommand { get; set; } = string.Empty;
        public string AlternateLabel { get; set; } = string.Empty;
        public Vector4 AlternateColor { get; set; } = new Vector4(1, 0, 0, 1);
        public string Name { get; set; } = "New Rule Group";
    }
}
