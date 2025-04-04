using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using WahButtons.Helpers;

namespace WahButtons.Windows
{
    public class HelpWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private string searchTerm = string.Empty;
        private Dictionary<string, string> helpTopics = new();
        private List<string> filteredTopics = new();
        private string currentTopic = string.Empty;
        private bool clipboardPopupOpen = true;
        private int _currentTab = 0;

        public HelpWindow(Plugin plugin)
            : base("Wah Buttons Help##HelpWindow", ImGuiWindowFlags.AlwaysAutoResize)
        {
            Plugin = plugin;
            Size = new Vector2(600, 500);
            SizeCondition = ImGuiCond.FirstUseEver;

            // Initialize help topics
            InitializeHelpTopics();
            filteredTopics = helpTopics.Keys.ToList();
        }

        public void Dispose() { }

        private void InitializeHelpTopics()
        {
            // Basic Concepts
            helpTopics["Getting Started"] =
@"# Getting Started with Wah Buttons

Wah Buttons allows you to create custom button interfaces for Final Fantasy XIV.

## Basic Steps:
1. Use the **Add Window** button to create a new button window
2. Add buttons to your window with the commands you want
3. Position the window where you want it on your screen
4. Lock the window to prevent accidental movement

For more detailed instructions, see the other help topics.";

            helpTopics["Creating Windows"] =
@"# Creating Button Windows

1. Click the **Add Window** button in the main interface
2. The new window will appear on your screen
3. In the main interface, select the window's tab to customize it
4. Set the window layout (Grid, Vertical, or Horizontal)
5. For grid layouts, set the number of rows and columns
6. Use the checkbox options to:
   - Show/hide the window
   - Lock the window position
   - Make the background transparent";

            helpTopics["Adding Buttons"] =
@"# Adding Buttons to Windows

1. Select a window tab in the main interface
2. Click **Add Button** to create a new button
3. Click **Edit** next to the new button to configure:
   - Label: The text displayed on the button
   - Command: The game command to execute when clicked
   - Width/Height: Button dimensions
   - Colors: Background and text colors
4. You can also choose from pre-made button templates
5. Rearrange buttons using the ↑ and ↓ arrows";

            // Smart Buttons
            helpTopics["Smart Buttons"] =
@"# Smart Buttons

Smart buttons change their behavior based on game conditions.

## Creating a Smart Button:
1. Edit any button and check the **Smart Button** checkbox
2. Click **Configure Rules** to open the rules editor
3. Add rule groups and conditions that determine when the button changes

## Smart Button Features:
- Hide or disable the button based on conditions
- Change the button color based on conditions
- Change the command executed based on conditions
- Change the button label based on conditions";

            helpTopics["Condition Rules"] =
@"# Understanding Condition Rules

Conditions determine when smart buttons change their behavior.

## Types of Conditions:
- **Game Condition**: In-game states like combat, mounted, fishing
- **Player Level**: Level range for the button to respond to
- **Player Job**: Specific jobs for the button to respond to
- **Current Zone**: Location-specific button behavior
- **Time of Day**: Time-based button behavior

## Rule Operators:
- **AND**: All conditions must be true
- **OR**: Any condition can be true

## Actions:
- **Hide**: Make the button invisible
- **Disable**: Show but disable the button
- **Change Color**: Alter appearance
- **Change Command**: Run a different command
- **Change Label**: Display different text";

            // Teleport Features
            helpTopics["Aetheryte Teleports"] =
@"# Aetheryte Teleport Buttons

Create buttons to quickly teleport to any aetheryte.

1. Open the **Advanced Features** window
2. Go to the **Aetherytes** tab
3. Select a region or search for a specific aetheryte
4. Select a target window from the dropdown
5. Click **Add** next to any aetheryte
6. Optional: Customize the button appearance

## Tips:
- Use **Auto-Detect Location** to find aetherytes near your current position
- Mark frequently used aetherytes as favorites
- Create themed teleport windows for different activities";

            // Command Reference
            helpTopics["Game Commands"] =
@"# Useful Game Commands

## General Commands:
- /echo message - Display text in chat
- /sit, /doze, /dance - Emotes
- /gpose - Enter group pose mode
- /return - Return to home point

## Combat Commands:
- /ac ""Ability Name"" <t> - Use ability on target
- /macroicon ""Ability Name"" - Set macro icon
- /gs change # - Change gear set

## Chat Commands:
- /p message - Party chat
- /fc message - Free Company chat
- /s message - Say
- /sh message - Shout

## Teleport Commands:
- /teleport # - Teleport to aetheryte by ID
- /return - Return to home point
- /estate - Teleport to your house";

            // Troubleshooting
            helpTopics["Troubleshooting"] =
@"# Troubleshooting

## Commands Not Working:
- Make sure the command syntax is correct
- Check if the button is disabled by a condition
- Some commands may only work in certain game states

## Windows Not Appearing:
- Check if the window is set to visible
- Make sure it's not positioned off-screen
- Try resetting the window position in settings

## Condition Rules Issues:
- Check the condition tracker to see current game states
- Verify rule syntax and rule operators
- Use the Data Tracker window to debug active conditions

## Performance Issues:
- Reduce the number of windows and buttons
- Simplify condition rules to improve performance
- Avoid using too many time-based conditions";
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("HelpTabBar"))
            {
                if (ImGui.BeginTabItem("Basic Usage"))
                {
                    _currentTab = 0;
                    DrawBasicUsageTab();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Smart Buttons"))
                {
                    _currentTab = 1;
                    DrawSmartButtonsTab();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Advanced Features"))
                {
                    _currentTab = 2;
                    DrawAdvancedFeaturesTab();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Commands"))
                {
                    _currentTab = 3;
                    DrawCommandsTab();
                    ImGui.EndTabItem();
                }
                
                ImGui.EndTabBar();
            }
            
            // Footer with plugin version
            ImGui.Separator();
            ImGui.Text($"Wah Buttons v{Plugin.Configuration.Version}");
        }
        
        private void DrawBasicUsageTab()
        {
            ImGui.TextWrapped("Wah Buttons lets you create custom button panels for your most-used commands in FFXIV.");
            ImGui.Separator();
            
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Getting Started:");
            ImGui.Bullet(); ImGui.TextWrapped("Click 'Add Window' to create a new button panel");
            ImGui.Bullet(); ImGui.TextWrapped("Use the tabs to customize each window");
            ImGui.Bullet(); ImGui.TextWrapped("Add buttons with commands like '/teleport 1' for teleporting to Ul'dah");
            ImGui.Bullet(); ImGui.TextWrapped("Customize button appearance with colors and size");
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Layout Options:");
            ImGui.Bullet(); ImGui.TextWrapped("Grid: Arrange buttons in rows and columns");
            ImGui.Bullet(); ImGui.TextWrapped("Vertical: Stack buttons from top to bottom");
            ImGui.Bullet(); ImGui.TextWrapped("Horizontal: Arrange buttons from left to right");
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Button Properties:");
            ImGui.Bullet(); ImGui.TextWrapped("Label: The text shown on the button");
            ImGui.Bullet(); ImGui.TextWrapped("Command: The FFXIV command to execute when clicked");
            ImGui.Bullet(); ImGui.TextWrapped("Size: Width and height of the button");
            ImGui.Bullet(); ImGui.TextWrapped("Colors: Background and text colors");
        }
        
        private void DrawSmartButtonsTab()
        {
            ImGui.TextWrapped("Smart Buttons can change their behavior based on game conditions, allowing you to create dynamic buttons that adapt to your current situation.");
            ImGui.Separator();
            
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Creating Smart Buttons:");
            ImGui.Bullet(); ImGui.TextWrapped("Add a button, then enable 'Smart Button' in the button editor");
            ImGui.Bullet(); ImGui.TextWrapped("Click 'Configure Rules' to set up conditional behavior");
            ImGui.Bullet(); ImGui.TextWrapped("Create rule groups that determine when alternative commands should be used");
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "How Smart Buttons Work:");
            ImGui.TextWrapped("Smart Buttons use IF-THEN logic to run different commands based on conditions:");
            ImGui.Bullet(); ImGui.TextWrapped("IF [condition(s) are true] THEN [run alternative command]");
            ImGui.Bullet(); ImGui.TextWrapped("ELSE [run default command]");
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Available Conditions:");
            ImGui.Bullet(); ImGui.TextWrapped("Game Conditions (In Combat, Mounted, etc.)");
            ImGui.Bullet(); ImGui.TextWrapped("Current Zone (Location-aware buttons)");
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Example Uses:");
            ImGui.Bullet(); ImGui.TextWrapped("Combat buttons that do one thing in combat and another out of combat");
            ImGui.Bullet(); ImGui.TextWrapped("Teleport buttons that change destinations based on zone");
            ImGui.Bullet(); ImGui.TextWrapped("Greeting buttons that say different things depending on location");
            ImGui.Bullet(); ImGui.TextWrapped("Macro buttons that execute different macros based on whether you're mounted");
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Multiple Conditions:");
            ImGui.Bullet(); ImGui.TextWrapped("AND Logic: All conditions must be true (e.g., in combat AND in a specific zone)");
            ImGui.Bullet(); ImGui.TextWrapped("OR Logic: Any condition can be true (e.g., in combat OR in a specific zone)");
        }
        
        private void DrawAdvancedFeaturesTab()
        {
            ImGui.TextWrapped("Wah Buttons includes several advanced features to enhance your gameplay experience.");
            ImGui.Separator();
            
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Condition Tracking:");
            ImGui.Bullet(); ImGui.TextWrapped("Monitor game conditions like 'In Combat', 'Mounted', etc.");
            ImGui.Bullet(); ImGui.TextWrapped("Useful for understanding when Smart Buttons will change behavior");
            ImGui.Bullet(); ImGui.TextWrapped("Access via 'Advanced Features' > 'Conditions' tab");
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Aetheryte Teleport Manager:");
            ImGui.Bullet(); ImGui.TextWrapped("Quick access to all aetherytes for creating teleport buttons");
            ImGui.Bullet(); ImGui.TextWrapped("Automatically detects your current region");
            ImGui.Bullet(); ImGui.TextWrapped("Access via 'Advanced Features' > 'Aetherytes' tab");
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Rule Groups:");
            ImGui.Bullet(); ImGui.TextWrapped("Combine multiple conditions with AND/OR logic");
            ImGui.Bullet(); ImGui.TextWrapped("Create complex condition checks for Smart Buttons");
            ImGui.Bullet(); ImGui.TextWrapped("Multiple rule groups per button for different scenarios");
        }
        
        private void DrawCommandsTab()
        {
            ImGui.TextWrapped("Wah Buttons supports these commands:");
            ImGui.Separator();
            
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Plugin Commands:");
            ImGui.Text("/wahbuttons");
            ImGui.Indent(20);
            ImGui.TextWrapped("Opens the main Wah Buttons interface");
            ImGui.Unindent(20);
            
            ImGui.Text("/wahbuttons help");
            ImGui.Indent(20);
            ImGui.TextWrapped("Opens this help window");
            ImGui.Unindent(20);
            
            ImGui.Text("/wahbuttons advanced");
            ImGui.Indent(20);
            ImGui.TextWrapped("Opens the Advanced Features window");
            ImGui.Unindent(20);
            
            ImGui.Text("/wahbuttons conditions");
            ImGui.Indent(20);
            ImGui.TextWrapped("Opens the Condition Tracker window");
            ImGui.Unindent(20);
            
            ImGui.Text("/wahbuttons aetherytes");
            ImGui.Indent(20);
            ImGui.TextWrapped("Opens the Aetheryte Teleport Manager");
            ImGui.Unindent(20);
            
            ImGui.Spacing();
            ImGui.TextColored(new Vector4(1, 1, 0, 1), "Useful Game Commands for Buttons:");
            ImGui.Text("/teleport [id]");
            ImGui.Indent(20);
            ImGui.TextWrapped("Teleports to the specified aetheryte ID");
            ImGui.Unindent(20);
            
            ImGui.Text("/gpose");
            ImGui.Indent(20);
            ImGui.TextWrapped("Enters Group Pose mode");
            ImGui.Unindent(20);
            
            ImGui.Text("/sit");
            ImGui.Indent(20);
            ImGui.TextWrapped("Makes your character sit");
            ImGui.Unindent(20);
            
            ImGui.Text("/macroicon [name]");
            ImGui.Indent(20);
            ImGui.TextWrapped("Executes the specified macro");
            ImGui.Unindent(20);
        }
    }
}