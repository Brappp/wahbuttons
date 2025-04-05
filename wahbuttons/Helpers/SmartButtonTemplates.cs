using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;

namespace WahButtons.Helpers
{
    public class SmartButtonTemplates
    {
        public class ButtonTemplate
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public System.Numerics.Vector4 Color { get; set; } = new System.Numerics.Vector4(0.3f, 0.5f, 0.9f, 1.0f);
            public string DefaultCommand { get; set; } = "/echo Default Action";
            public string DefaultLabel { get; set; } = "Smart Button";
            public List<Configuration.AdvancedRuleGroup> Rules { get; set; } = new List<Configuration.AdvancedRuleGroup>();
        }
        
        private static List<ButtonTemplate> _templates = null;
        
        public static List<ButtonTemplate> GetTemplates()
        {
            if (_templates == null)
            {
                InitializeTemplates();
            }
            
            return _templates;
        }
        
        public static List<ButtonTemplate> GetTemplatesByCategory(string category)
        {
            var allTemplates = GetTemplates();
            
            if (category == "All")
            {
                return allTemplates;
            }
            
            return allTemplates.Where(t => t.Category == category).ToList();
        }
        
        public static void ApplyTemplate(ButtonTemplate template, Configuration.ButtonData button)
        {
            // Update button appearance and behavior
            button.Label = template.DefaultLabel;
            button.Command = template.DefaultCommand;
            button.Color = template.Color;
            button.IsSmartButton = true;
            
            // Clear existing rules
            button.AdvancedRules.Clear();
            
            // Clone the template rules
            foreach (var rule in template.Rules)
            {
                var newRule = new Configuration.AdvancedRuleGroup
                {
                    Name = rule.Name,
                    Action = rule.Action,
                    AlternateCommand = rule.AlternateCommand,
                    AlternateLabel = rule.AlternateLabel,
                    AlternateColor = rule.AlternateColor,
                    Operator = rule.Operator
                };
                
                // Clone conditions
                foreach (var condition in rule.Conditions)
                {
                    var newCondition = new Configuration.AdvancedCondition
                    {
                        Type = condition.Type,
                        GameCondition = condition.GameCondition,
                        ExpectedState = condition.ExpectedState,
                        ZoneId = condition.ZoneId
                        // Add other properties as needed
                    };
                    
                    newRule.Conditions.Add(newCondition);
                }
                
                button.AdvancedRules.Add(newRule);
            }
        }
        
        private static void InitializeTemplates()
        {
            _templates = new List<ButtonTemplate>
            {
                // Combat Templates
                new ButtonTemplate
                {
                    Name = "Combat Toggle",
                    Description = "Changes behavior based on whether you're in combat or not",
                    Category = "Combat",
                    DefaultLabel = "Combat Toggle",
                    DefaultCommand = "/echo Out of combat",
                    Color = new System.Numerics.Vector4(0.2f, 0.6f, 0.3f, 1.0f),
                    Rules = new List<Configuration.AdvancedRuleGroup>
                    {
                        new Configuration.AdvancedRuleGroup
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
                        }
                    }
                },
                
                // Movement Templates
                new ButtonTemplate
                {
                    Name = "Mount/Sprint Toggle",
                    Description = "Uses mount normally, but sprint when in combat or duty",
                    Category = "Movement",
                    DefaultLabel = "Move",
                    DefaultCommand = "/mount \"Company Chocobo\"",
                    Color = new System.Numerics.Vector4(0.8f, 0.6f, 0.2f, 1.0f),
                    Rules = new List<Configuration.AdvancedRuleGroup>
                    {
                        new Configuration.AdvancedRuleGroup
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
                        }
                    }
                },
                
                // Duty Templates
                new ButtonTemplate
                {
                    Name = "Duty Chat",
                    Description = "Uses /shout normally, but switches to /party when in a duty",
                    Category = "Duty",
                    DefaultLabel = "Chat",
                    DefaultCommand = "/sh Hello everyone!",
                    Color = new System.Numerics.Vector4(0.8f, 0.3f, 0.6f, 1.0f),
                    Rules = new List<Configuration.AdvancedRuleGroup>
                    {
                        new Configuration.AdvancedRuleGroup
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
                        }
                    }
                },
                
                // Social Templates
                new ButtonTemplate
                {
                    Name = "City-Specific Greetings",
                    Description = "Changes your greeting based on which city you're in",
                    Category = "Social",
                    DefaultLabel = "Greet",
                    DefaultCommand = "/em waves hello.",
                    Color = new System.Numerics.Vector4(0.9f, 0.5f, 0.3f, 1.0f),
                    Rules = new List<Configuration.AdvancedRuleGroup>
                    {
                        new Configuration.AdvancedRuleGroup
                        {
                            Name = "In Limsa",
                            Action = Configuration.RuleAction.ChangeCommand,
                            AlternateCommand = "/em says, \"Ahoy, mateys!\"",
                            Conditions = new List<Configuration.AdvancedCondition>
                            {
                                new Configuration.AdvancedCondition
                                {
                                    Type = Configuration.ConditionType.CurrentZone,
                                    ZoneId = 128 // Limsa Lominsa
                                }
                            }
                        },
                        new Configuration.AdvancedRuleGroup
                        {
                            Name = "In Gridania",
                            Action = Configuration.RuleAction.ChangeCommand,
                            AlternateCommand = "/em bows respectfully to the elementals.",
                            Conditions = new List<Configuration.AdvancedCondition>
                            {
                                new Configuration.AdvancedCondition
                                {
                                    Type = Configuration.ConditionType.CurrentZone,
                                    ZoneId = 132 // Gridania
                                }
                            }
                        },
                        new Configuration.AdvancedRuleGroup
                        {
                            Name = "In Ul'dah",
                            Action = Configuration.RuleAction.ChangeCommand,
                            AlternateCommand = "/em flashes a bag of gil.",
                            Conditions = new List<Configuration.AdvancedCondition>
                            {
                                new Configuration.AdvancedCondition
                                {
                                    Type = Configuration.ConditionType.CurrentZone,
                                    ZoneId = 130 // Ul'dah
                                }
                            }
                        }
                    }
                },
                
                // Crafting Templates
                new ButtonTemplate
                {
                    Name = "Crafting Helper",
                    Description = "Changes behavior based on whether you're crafting or not",
                    Category = "Crafting",
                    DefaultLabel = "Craft",
                    DefaultCommand = "/ac \"Basic Synthesis\"",
                    Color = new System.Numerics.Vector4(0.4f, 0.4f, 0.8f, 1.0f),
                    Rules = new List<Configuration.AdvancedRuleGroup>
                    {
                        new Configuration.AdvancedRuleGroup
                        {
                            Name = "When Crafting",
                            Action = Configuration.RuleAction.ChangeCommand,
                            AlternateCommand = "/ac \"Careful Synthesis\"",
                            Conditions = new List<Configuration.AdvancedCondition>
                            {
                                new Configuration.AdvancedCondition
                                {
                                    Type = Configuration.ConditionType.GameCondition,
                                    GameCondition = ConditionFlag.Crafting,
                                    ExpectedState = true
                                }
                            }
                        }
                    }
                }
            };
        }
    }
} 