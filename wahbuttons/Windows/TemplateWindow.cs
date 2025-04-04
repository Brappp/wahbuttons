using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using WahButtons.Helpers;
using WahButtons.Windows;
using Dalamud.Game.ClientState.Conditions;

namespace WahButtons.Windows
{
    public class TemplateWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private WindowSystem WindowSystem;
        private Configuration Configuration;
        private ButtonWindow TargetWindow;
        private string SelectedCategory = "All";
        private string SearchTerm = string.Empty;
        private Vector4 PreviewButtonColor = new Vector4(0.26f, 0.59f, 0.98f, 1f);
        private Vector4 PreviewLabelColor = new Vector4(1f, 1f, 1f, 1f);
        private string PreviewLabel = "Button";
        private string PreviewCommand = "/echo Hello!";
        private float PreviewWidth = 75;
        private float PreviewHeight = 30;
        private bool IsPreviewSmartButton = false;
        private bool ButtonAddedPopupOpen = false;

        // Button templates database
        private List<ButtonTemplate> buttonTemplates = new List<ButtonTemplate>();

        public class ButtonTemplate
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public string Label { get; set; }
            public string Command { get; set; }
            public Vector4 Color { get; set; } = new Vector4(0.26f, 0.59f, 0.98f, 1f);
            public Vector4 LabelColor { get; set; } = new Vector4(1f, 1f, 1f, 1f);
            public float Width { get; set; } = 75;
            public float Height { get; set; } = 30;
            public bool IsSmartButton { get; set; } = false;
            public List<Configuration.ButtonConditionRule> ConditionRules { get; set; } = new();
        }

        public TemplateWindow(Plugin plugin, WindowSystem windowSystem, Configuration configuration)
            : base("Button Templates##TemplateWindow", ImGuiWindowFlags.AlwaysAutoResize)
        {
            Plugin = plugin;
            WindowSystem = windowSystem;
            Configuration = configuration;

            Size = new Vector2(700, 500);
            SizeCondition = ImGuiCond.FirstUseEver;

            // Initialize button templates
            InitializeButtonTemplates();
        }

        public void Dispose() { }

        public void SetTargetWindow(ButtonWindow window)
        {
            TargetWindow = window;
        }

        private void InitializeButtonTemplates()
        {
            // Combat Templates
            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Sprint",
                Description = "Sprint ability that's disabled in combat",
                Category = "Combat",
                Label = "Sprint",
                Command = "/ac Sprint",
                Color = new Vector4(0.4f, 0.7f, 1.0f, 1.0f),
                IsSmartButton = true,
                ConditionRules = new List<Configuration.ButtonConditionRule>
                {
                    new Configuration.ButtonConditionRule(ConditionFlag.InCombat, true, Configuration.RuleAction.Disable)
                }
            });

            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Limit Break",
                Description = "Limit Break button that's only enabled in duties",
                Category = "Combat",
                Label = "Limit Break",
                Command = "/ac \"Limit Break\" <t>",
                Color = new Vector4(1.0f, 0.4f, 0.4f, 1.0f),
                IsSmartButton = true,
                ConditionRules = new List<Configuration.ButtonConditionRule>
                {
                    new Configuration.ButtonConditionRule(ConditionFlag.BoundByDuty, false, Configuration.RuleAction.Disable)
                }
            });

            // Emote Templates
            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Dance",
                Description = "Makes your character dance",
                Category = "Emotes",
                Label = "Dance",
                Command = "/dance",
                Color = new Vector4(0.9f, 0.5f, 0.9f, 1.0f)
            });

            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Sit",
                Description = "Makes your character sit",
                Category = "Emotes",
                Label = "Sit",
                Command = "/sit",
                Color = new Vector4(0.9f, 0.5f, 0.9f, 1.0f)
            });

            // Teleport Templates
            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Return",
                Description = "Return to your home point",
                Category = "Teleport",
                Label = "Return",
                Command = "/return",
                Color = new Vector4(0.3f, 0.7f, 0.9f, 1.0f)
            });

            // Gear Templates
            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Gear Set Switch",
                Description = "Switch to a specific gear set",
                Category = "Gear",
                Label = "Tank Gear",
                Command = "/gearset change 1",
                Color = new Vector4(0.5f, 0.5f, 0.5f, 1.0f)
            });

            // Chat Templates
            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Party Greeting",
                Description = "Send a greeting to party chat",
                Category = "Chat",
                Label = "Hello",
                Command = "/p Hello everyone!",
                Color = new Vector4(0.8f, 0.8f, 0.2f, 1.0f)
            });

            // Color Schemes
            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Red Button",
                Description = "A red button template",
                Category = "Visual",
                Label = "Red Button",
                Command = "/echo Red button clicked!",
                Color = new Vector4(0.9f, 0.2f, 0.2f, 1.0f),
            });

            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Green Button",
                Description = "A green button template",
                Category = "Visual",
                Label = "Green Button",
                Command = "/echo Green button clicked!",
                Color = new Vector4(0.2f, 0.8f, 0.2f, 1.0f),
            });

            buttonTemplates.Add(new ButtonTemplate
            {
                Name = "Blue Button",
                Description = "A blue button template",
                Category = "Visual",
                Label = "Blue Button",
                Command = "/echo Blue button clicked!",
                Color = new Vector4(0.2f, 0.2f, 0.9f, 1.0f),
            });
        }

        public override void Draw()
        {
            if (TargetWindow == null)
            {
                ImGui.TextColored(new Vector4(1, 1, 0, 1),
                    "No target window selected. Please close this window and select a target window first.");
                return;
            }

            ImGui.Text($"Adding buttons to: {TargetWindow.Config.Name}");
            ImGui.Separator();

            // Search and category filter
            DrawSearchAndFilters();

            ImGui.Separator();

            // Templates section
            DrawTemplateList();

            ImGui.Separator();

            // Preview section
            DrawPreviewSection();

            // Popup for button added confirmation
            DrawButtonAddedPopup();
        }

        private void DrawSearchAndFilters()
        {
            // Search box
            ImGui.Text("Search: ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputText("##TemplateSearch", ref SearchTerm, 100))
            {
                // Search is handled in the template list
            }

            ImGui.SameLine();

            // Category dropdown
            ImGui.Text("Category: ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);

            string[] categories = { "All", "Combat", "Emotes", "Teleport", "Gear", "Chat", "Visual" };

            if (ImGui.BeginCombo("##CategoryFilter", SelectedCategory))
            {
                foreach (var category in categories)
                {
                    if (ImGui.Selectable(category, SelectedCategory == category))
                    {
                        SelectedCategory = category;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();

            // Clear filters button
            if (ImGui.Button("Clear Filters"))
            {
                SearchTerm = string.Empty;
                SelectedCategory = "All";
            }
        }

        private void DrawTemplateList()
        {
            WindowHelper.DrawSectionHeader("Available Templates");

            // Filter templates based on search and category
            var filteredTemplates = buttonTemplates
                .Where(t =>
                    (string.IsNullOrEmpty(SearchTerm) ||
                     t.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                     t.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) &&
                    (SelectedCategory == "All" || t.Category == SelectedCategory))
                .ToList();

            if (filteredTemplates.Count == 0)
            {
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "No templates match your search criteria.");
                return;
            }

            // Template table
            if (ImGui.BeginTable("TemplatesTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Template", ImGuiTableColumnFlags.WidthFixed, 150);
                ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Preview", ImGuiTableColumnFlags.WidthFixed, 80);
                ImGui.TableSetupColumn("Add", ImGuiTableColumnFlags.WidthFixed, 60);
                ImGui.TableHeadersRow();

                foreach (var template in filteredTemplates)
                {
                    ImGui.TableNextRow();

                    // Template name column
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(template.Name);
                    ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), $"({template.Category})");

                    // Description column
                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextWrapped(template.Description);

                    // Preview column
                    ImGui.TableSetColumnIndex(2);
                    ImGui.PushID($"preview_{template.Name}");
                    if (ImGui.Button("Preview"))
                    {
                        // Set preview values
                        PreviewLabel = template.Label;
                        PreviewCommand = template.Command;
                        PreviewButtonColor = template.Color;
                        PreviewLabelColor = template.LabelColor;
                        PreviewWidth = template.Width;
                        PreviewHeight = template.Height;
                        IsPreviewSmartButton = template.IsSmartButton;
                    }
                    ImGui.PopID();

                    // Add column
                    ImGui.TableSetColumnIndex(3);
                    ImGui.PushID($"add_{template.Name}");
                    if (ImGui.Button("Add"))
                    {
                        AddTemplateToWindow(template);
                    }
                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
        }

        private void DrawPreviewSection()
        {
            WindowHelper.DrawSectionHeader("Button Preview");

            // Left panel - Preview
            float previewPanelWidth = ImGui.GetContentRegionAvail().X / 2;
            if (ImGui.BeginChild("PreviewPanel", new Vector2(previewPanelWidth, 150), true))
            {
                // Center the preview button
                float windowWidth = ImGui.GetContentRegionAvail().X;
                float windowHeight = ImGui.GetContentRegionAvail().Y;
                float buttonX = (windowWidth - PreviewWidth) / 2;
                float buttonY = (windowHeight - PreviewHeight) / 2;

                ImGui.SetCursorPos(new Vector2(buttonX, buttonY));

                // Draw preview button
                ImGui.PushStyleColor(ImGuiCol.Button, PreviewButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(
                    Math.Min(PreviewButtonColor.X * 1.2f, 1.0f),
                    Math.Min(PreviewButtonColor.Y * 1.2f, 1.0f),
                    Math.Min(PreviewButtonColor.Z * 1.2f, 1.0f),
                    PreviewButtonColor.W));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(
                    Math.Min(PreviewButtonColor.X * 0.8f, 1.0f),
                    Math.Min(PreviewButtonColor.Y * 0.8f, 1.0f),
                    Math.Min(PreviewButtonColor.Z * 0.8f, 1.0f),
                    PreviewButtonColor.W));
                ImGui.PushStyleColor(ImGuiCol.Text, PreviewLabelColor);

                ImGui.Button(PreviewLabel, new Vector2(PreviewWidth, PreviewHeight));
                ImGui.PopStyleColor(4);

                ImGui.EndChild();
            }

            ImGui.SameLine();

            // Right panel - Customization
            if (ImGui.BeginChild("CustomizationPanel", new Vector2(previewPanelWidth, 150), true))
            {
                ImGui.Text("Customize Template:");

                // Label
                ImGui.SetNextItemWidth(150);
                if (ImGui.InputText("Label", ref PreviewLabel, 100))
                {
                    // Update preview label
                }

                // Command
                ImGui.SetNextItemWidth(150);
                if (ImGui.InputText("Command", ref PreviewCommand, 100))
                {
                    // Update preview command
                }

                // Colors
                if (ImGui.ColorEdit4("Button Color##Preview", ref PreviewButtonColor, ImGuiColorEditFlags.NoInputs))
                {
                    // Update preview color
                }

                if (ImGui.ColorEdit4("Label Color##Preview", ref PreviewLabelColor, ImGuiColorEditFlags.NoInputs))
                {
                    // Update preview label color
                }

                // Add custom button
                if (ImGui.Button("Add Custom Button", new Vector2(150, 30)))
                {
                    AddCustomButtonToWindow();
                }

                ImGui.EndChild();
            }
        }

        private void DrawButtonAddedPopup()
        {
            if (ImGui.BeginPopupModal("ButtonAddedPopup", ref ButtonAddedPopupOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Button added successfully!");

                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                    ButtonAddedPopupOpen = false;
                }

                ImGui.EndPopup();
            }
        }

        private void AddTemplateToWindow(ButtonTemplate template)
        {
            var newButton = new Configuration.ButtonData(template.Label, template.Command, template.Width)
            {
                Height = template.Height,
                Color = template.Color,
                LabelColor = template.LabelColor,
                IsSmartButton = template.IsSmartButton
            };

            // Copy condition rules if any exist
            if (template.IsSmartButton && template.ConditionRules.Count > 0)
            {
                foreach (var rule in template.ConditionRules)
                {
                    newButton.ConditionRules.Add(new Configuration.ButtonConditionRule(
                        rule.Flag, rule.ExpectedState, rule.Action)
                    {
                        AlternateCommand = rule.AlternateCommand,
                        AlternateLabel = rule.AlternateLabel,
                        AlternateColor = rule.AlternateColor
                    });
                }
            }

            TargetWindow.Config.Buttons.Add(newButton);
            Configuration.Save();

            // Show confirmation popup
            ButtonAddedPopupOpen = true;
            ImGui.OpenPopup("ButtonAddedPopup");
        }

        private void AddCustomButtonToWindow()
        {
            var newButton = new Configuration.ButtonData(PreviewLabel, PreviewCommand, PreviewWidth)
            {
                Height = PreviewHeight,
                Color = PreviewButtonColor,
                LabelColor = PreviewLabelColor,
                IsSmartButton = IsPreviewSmartButton
            };

            TargetWindow.Config.Buttons.Add(newButton);
            Configuration.Save();

            // Show confirmation popup
            ButtonAddedPopupOpen = true;
            ImGui.OpenPopup("ButtonAddedPopup");
        }
    }
}