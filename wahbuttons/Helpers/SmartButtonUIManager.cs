using System;
using System.Collections.Generic;
using System.Linq;
using WahButtons.Windows;
using Dalamud.Game.ClientState.Conditions;

namespace WahButtons.Helpers
{
    public static class SmartButtonUIManager
    {
        // Opens the Smart Button Rules window
        public static void OpenSmartButtonRulesWindow(Plugin plugin, ButtonWindow parentWindow, Configuration.ButtonData button)
        {
            // Remove any existing SmartButtonRulesWindow first
            foreach (var window in plugin.WindowSystem.Windows.ToList())
            {
                if (window is SmartButtonRulesWindow)
                {
                    plugin.WindowSystem.RemoveWindow(window);
                }
            }
            
            // Create and open the new window
            var smartWindow = new SmartButtonRulesWindow(plugin, parentWindow, button);
            plugin.WindowSystem.AddWindow(smartWindow);
            smartWindow.IsOpen = true;
        }
        
        // Opens the Smart Button Template Selector window
        public static void OpenTemplateWindow(Plugin plugin, Configuration.ButtonData button)
        {
            // Remove any existing SmartTemplateWindow first
            foreach (var window in plugin.WindowSystem.Windows.ToList())
            {
                if (window is SmartTemplateWindow)
                {
                    plugin.WindowSystem.RemoveWindow(window);
                }
            }
            
            // Create and open the new window
            var templateWindow = new SmartTemplateWindow(plugin, button);
            plugin.WindowSystem.AddWindow(templateWindow);
            templateWindow.IsOpen = true;
        }
        
        // Add a "Quick Smart Button" with a custom name
        public static void AddQuickSmartButton(Plugin plugin, Configuration.ButtonWindowConfig windowConfig, string buttonName)
        {
            // Create a new button with default settings but custom name
            var newButton = new Configuration.ButtonData
            {
                Label = buttonName,
                Command = "/echo Default action",
                Width = 100,
                Height = 40,
                Color = new System.Numerics.Vector4(0.4f, 0.6f, 0.9f, 1.0f),
                LabelColor = new System.Numerics.Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                IsSmartButton = true,
                AdvancedRules = new List<Configuration.AdvancedRuleGroup>
                {
                    new Configuration.AdvancedRuleGroup
                    {
                        Name = "In Combat Rule",
                        Action = Configuration.RuleAction.ChangeCommand,
                        AlternateCommand = "/echo In-combat action",
                        Conditions = new List<Configuration.AdvancedCondition>
                        {
                            new Configuration.AdvancedCondition
                            {
                                Type = Configuration.ConditionType.GameCondition,
                                GameCondition = ConditionFlag.InCombat,
                                ExpectedState = true
                            }
                        }
                    }
                }
            };
            
            // Add the button to the window
            windowConfig.Buttons.Add(newButton);
            plugin.Configuration.Save();
            
            // Find the ButtonWindow and open the SmartButtonRulesWindow
            ButtonWindow targetWindow = null;
            foreach (var window in plugin.WindowSystem.Windows)
            {
                if (window is ButtonWindow btnWindow && btnWindow.Config == windowConfig)
                {
                    targetWindow = btnWindow;
                    break;
                }
            }
            
            if (targetWindow != null)
            {
                OpenSmartButtonRulesWindow(plugin, targetWindow, newButton);
            }
        }
        
        // Overload for the original AddQuickSmartButton that uses a default name
        public static void AddQuickSmartButton(Plugin plugin, Configuration.ButtonWindowConfig windowConfig)
        {
            AddQuickSmartButton(plugin, windowConfig, "Smart Button");
        }
        
        // Quick-configure common smart button scenarios
        public static void ConfigureSmartButton(Plugin plugin, Configuration.ButtonData button, string scenario)
        {
            switch (scenario)
            {
                case "Combat Toggle":
                    SetupCombatToggle(button);
                    break;
                    
                case "Mount/Sprint":
                    SetupMountSprintToggle(button);
                    break;
                    
                case "Duty Chat":
                    SetupDutyChat(button);
                    break;
                    
                case "City Teleport":
                    SetupCityTeleport(button);
                    break;
            }
            
            plugin.Configuration.Save();
        }
        
        // Helper methods for quick configuration
        private static void SetupCombatToggle(Configuration.ButtonData button)
        {
            button.Label = "Combat Toggle";
            button.Command = "/echo Out of combat";
            button.IsSmartButton = true;
            button.Color = new System.Numerics.Vector4(0.2f, 0.6f, 0.3f, 1.0f);
            
            // Clear existing rules
            button.AdvancedRules.Clear();
            
            // Add combat rule
            button.AdvancedRules.Add(new Configuration.AdvancedRuleGroup
            {
                Name = "In Combat",
                Action = Configuration.RuleAction.ChangeCommand,
                AlternateCommand = "/echo In combat mode!",
                Conditions = new List<Configuration.AdvancedCondition>
                {
                    new Configuration.AdvancedCondition
                    {
                        Type = Configuration.ConditionType.GameCondition,
                        GameCondition = ConditionFlag.InCombat,
                        ExpectedState = true
                    }
                }
            });
        }
        
        private static void SetupMountSprintToggle(Configuration.ButtonData button)
        {
            button.Label = "Move";
            button.Command = "/mount \"Company Chocobo\"";
            button.IsSmartButton = true;
            button.Color = new System.Numerics.Vector4(0.8f, 0.6f, 0.2f, 1.0f);
            
            // Clear existing rules
            button.AdvancedRules.Clear();
            
            // Add combat/duty rule
            button.AdvancedRules.Add(new Configuration.AdvancedRuleGroup
            {
                Name = "In Combat or Duty",
                Action = Configuration.RuleAction.ChangeCommand,
                AlternateCommand = "/ac Sprint",
                Operator = Configuration.RuleOperator.Or,
                Conditions = new List<Configuration.AdvancedCondition>
                {
                    new Configuration.AdvancedCondition
                    {
                        Type = Configuration.ConditionType.GameCondition,
                        GameCondition = ConditionFlag.InCombat,
                        ExpectedState = true
                    },
                    new Configuration.AdvancedCondition
                    {
                        Type = Configuration.ConditionType.GameCondition,
                        GameCondition = ConditionFlag.BoundByDuty,
                        ExpectedState = true
                    }
                }
            });
        }
        
        private static void SetupDutyChat(Configuration.ButtonData button)
        {
            button.Label = "Chat";
            button.Command = "/sh Hello everyone!";
            button.IsSmartButton = true;
            button.Color = new System.Numerics.Vector4(0.8f, 0.3f, 0.6f, 1.0f);
            
            // Clear existing rules
            button.AdvancedRules.Clear();
            
            // Add duty rule
            button.AdvancedRules.Add(new Configuration.AdvancedRuleGroup
            {
                Name = "In Duty",
                Action = Configuration.RuleAction.ChangeCommand,
                AlternateCommand = "/p Hello, party members!",
                Conditions = new List<Configuration.AdvancedCondition>
                {
                    new Configuration.AdvancedCondition
                    {
                        Type = Configuration.ConditionType.GameCondition,
                        GameCondition = ConditionFlag.BoundByDuty,
                        ExpectedState = true
                    }
                }
            });
        }
        
        private static void SetupCityTeleport(Configuration.ButtonData button)
        {
            button.Label = "City Teleport";
            button.Command = "/teleport";
            button.IsSmartButton = true;
            button.Color = new System.Numerics.Vector4(0.3f, 0.3f, 0.8f, 1.0f);
            
            // Clear existing rules
            button.AdvancedRules.Clear();
            
            // Add teleport rules for different cities
            button.AdvancedRules.Add(new Configuration.AdvancedRuleGroup
            {
                Name = "In Limsa",
                Action = Configuration.RuleAction.ChangeCommand,
                AlternateCommand = "/teleport 2", // Limsa Lominsa Lower Decks
                Conditions = new List<Configuration.AdvancedCondition>
                {
                    new Configuration.AdvancedCondition
                    {
                        Type = Configuration.ConditionType.CurrentZone,
                        ZoneId = 128 // Limsa Lominsa
                    }
                }
            });
            
            button.AdvancedRules.Add(new Configuration.AdvancedRuleGroup
            {
                Name = "In Gridania",
                Action = Configuration.RuleAction.ChangeCommand,
                AlternateCommand = "/teleport 3", // New Gridania
                Conditions = new List<Configuration.AdvancedCondition>
                {
                    new Configuration.AdvancedCondition
                    {
                        Type = Configuration.ConditionType.CurrentZone,
                        ZoneId = 132 // Gridania
                    }
                }
            });
            
            button.AdvancedRules.Add(new Configuration.AdvancedRuleGroup
            {
                Name = "In Ul'dah",
                Action = Configuration.RuleAction.ChangeCommand,
                AlternateCommand = "/teleport 1", // Ul'dah - Steps of Nald
                Conditions = new List<Configuration.AdvancedCondition>
                {
                    new Configuration.AdvancedCondition
                    {
                        Type = Configuration.ConditionType.CurrentZone,
                        ZoneId = 130 // Ul'dah
                    }
                }
            });
        }
    }
} 