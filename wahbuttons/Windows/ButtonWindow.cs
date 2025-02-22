using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;
using WahButtons.Helpers;
using WahButtons;

namespace WahButtons.Windows
{
    public class ButtonWindow : Window, IDisposable
    {
        private Plugin Plugin;
        public Configuration.ButtonWindowConfig Config { get; }

        public ButtonWindow(Plugin plugin, Configuration.ButtonWindowConfig config)
            : base(config.Name + "##" + Guid.NewGuid(),
                  ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)
        {
            Plugin = plugin;
            Config = config;
            IsOpen = config.IsVisible;
            RespectCloseHotkey = false;
        }

        public void Dispose() { }

        public override void PreDraw()
        {
            ImGuiHelper.PushWahButtonsStyle();

            Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

            if (Config.IsLocked)
            {
                Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;
            }

            if (Config.TransparentBackground)
            {
                Flags |= ImGuiWindowFlags.NoBackground;
            }

            IsOpen = Config.IsVisible;

            // Handle window repositioning based on expansion state
            if (isResettingPosition && !collapsedWindowPos.Equals(Vector2.Zero))
            {
                // We're collapsing, so reset to the original position
                ImGui.SetNextWindowPos(collapsedWindowPos);
                Config.Position = collapsedWindowPos;
                isResettingPosition = false;
                Plugin.Configuration.Save();
            }
            else if (Config.Layout == Configuration.ButtonLayout.Expanding &&
                    Config.IsExpanded &&
                    positionsInitialized &&
                    (Config.ExpansionDirection == Configuration.ExpandDirection.Left ||
                     Config.ExpansionDirection == Configuration.ExpandDirection.Up))
            {
                // Special window positioning for left/up expansions to maintain menu button position
                int mainIndex = Math.Min(Config.MainButtonIndex, Config.Buttons.Count - 1);

                // Calculate total size of all non-menu buttons
                float totalWidth = 0;
                float totalHeight = 0;
                for (int i = 0; i < Config.Buttons.Count; i++)
                {
                    if (i != mainIndex)
                    {
                        totalWidth += Config.Buttons[i].Width + 5;
                        totalHeight += Config.Buttons[i].Height + 5;
                    }
                }

                // Calculate window position to keep menu button in same visual place
                Vector2 newWindowPos = collapsedWindowPos;

                if (Config.ExpansionDirection == Configuration.ExpandDirection.Left)
                {
                    newWindowPos.X = collapsedMenuPos.X - totalWidth;
                }
                else if (Config.ExpansionDirection == Configuration.ExpandDirection.Up)
                {
                    newWindowPos.Y = collapsedMenuPos.Y - totalHeight;
                }

                ImGui.SetNextWindowPos(newWindowPos);
            }
            else
            {
                // Default positioning
                ImGui.SetNextWindowPos(Config.Position, ImGuiCond.FirstUseEver);
            }
        }

        public override void PostDraw()
        {
            ImGuiHelper.PopWahButtonsStyle();
            base.PostDraw();
        }

        public override void Draw()
        {
            // For left expansion, we need to prepare the layout in advance
            if (Config.Layout == Configuration.ButtonLayout.Expanding &&
                Config.IsExpanded &&
                Config.ExpansionDirection == Configuration.ExpandDirection.Up)
            {
                PreDrawExpandingLayout();
            }

            if (!Config.IsLocked)
            {
                Vector2 currentPosition = ImGui.GetWindowPos();
                if (IsPositionDifferent(currentPosition, Config.Position))
                {
                    Config.Position = currentPosition;
                    Plugin.Configuration.Save();
                }

                Config.Size = ImGui.GetWindowSize();
            }

            if (Config.TransparentBackground)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 0);
            }

            switch (Config.Layout)
            {
                case Configuration.ButtonLayout.Vertical:
                    DrawVerticalLayout();
                    break;
                case Configuration.ButtonLayout.Horizontal:
                    DrawHorizontalLayout();
                    break;
                case Configuration.ButtonLayout.Grid:
                    DrawGridLayout();
                    break;
                case Configuration.ButtonLayout.Expanding:
                    DrawExpandingLayout();
                    break;
                case Configuration.ButtonLayout.Tabbed:
                    DrawTabbedLayout();
                    break;
                default:
                    DrawVerticalLayout();
                    break;
            }

            if (Config.TransparentBackground)
            {
                ImGui.PopStyleVar();
            }
        }

        private void DrawVerticalLayout()
        {
            ImGui.BeginGroup();
            foreach (var button in Config.Buttons)
            {
                RenderButton(button);
                ImGui.Spacing();
            }
            ImGui.EndGroup();
        }

        private void DrawHorizontalLayout()
        {
            ImGui.BeginGroup();
            for (int i = 0; i < Config.Buttons.Count; i++)
            {
                RenderButton(Config.Buttons[i]);
                if (i < Config.Buttons.Count - 1)
                {
                    ImGui.SameLine();
                }
            }
            ImGui.EndGroup();
        }

        private void DrawGridLayout()
        {
            if (Config.Buttons.Count == 0) return;

            int rows = Math.Max(1, Config.GridRows);
            int cols = Math.Max(1, Config.GridColumns);

            ImGui.BeginGroup();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int index = row * cols + col;
                    if (index < Config.Buttons.Count)
                    {
                        RenderButton(Config.Buttons[index]);
                    }
                    else
                    {
                        // Empty space for visual consistency
                        ImGui.InvisibleButton("##empty", new Vector2(85, 25));
                    }

                    if (col < cols - 1)
                    {
                        ImGui.SameLine();
                    }
                }
            }
            ImGui.EndGroup();
        }

        private void DrawTabbedLayout()
        {
            if (Config.Tabs.Count == 0 || Config.Buttons.Count == 0) return;

            int activeTab = Math.Min(Config.ActiveTab, Config.Tabs.Count - 1);

            // Container for the entire tabbed layout
            ImGui.BeginGroup();

            // Draw the tab bar
            float tabHeight = 25;  // Standard tab height
            float tabSpacing = 2;  // Spacing between tabs
            float availableWidth = ImGui.GetContentRegionAvail().X;

            // First pass: calculate maximum tab width needed
            float maxTabTextWidth = 0;
            for (int i = 0; i < Config.Tabs.Count; i++)
            {
                Vector2 textSize = ImGui.CalcTextSize(Config.Tabs[i].Name);
                maxTabTextWidth = Math.Max(maxTabTextWidth, textSize.X);
            }

            // Add padding to the text width
            float tabWidth = maxTabTextWidth + 20; // 10px padding on each side

            // Ensure tabs don't overflow available width
            if (tabWidth * Config.Tabs.Count + tabSpacing * (Config.Tabs.Count - 1) > availableWidth)
            {
                // Adjust to fit all tabs
                tabWidth = (availableWidth - (tabSpacing * (Config.Tabs.Count - 1))) / Config.Tabs.Count;
            }

            // Minimum tab width
            tabWidth = Math.Max(50, tabWidth);

            // Draw tabs in a scrollable area if needed
            if (tabWidth * Config.Tabs.Count + tabSpacing * (Config.Tabs.Count - 1) > availableWidth)
            {
                // Calculate total width needed
                float totalTabWidth = tabWidth * Config.Tabs.Count + tabSpacing * (Config.Tabs.Count - 1);

                // Create a horizontal scrollbar if needed
                ImGui.BeginChild("##TabScrollArea", new Vector2(availableWidth, tabHeight + 5), false, ImGuiWindowFlags.HorizontalScrollbar);
            }

            // Draw tabs as a row of buttons with custom styling
            for (int i = 0; i < Config.Tabs.Count; i++)
            {
                if (i > 0) ImGui.SameLine(0, tabSpacing);

                // Apply custom tab styling
                if (i == activeTab)
                {
                    // Active tab styling
                    ImGui.PushStyleColor(ImGuiCol.Button, Config.TabActiveColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(
                        Math.Min(Config.TabActiveColor.X * 1.1f, 1.0f),
                        Math.Min(Config.TabActiveColor.Y * 1.1f, 1.0f),
                        Math.Min(Config.TabActiveColor.Z * 1.1f, 1.0f),
                        Config.TabActiveColor.W
                    ));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(
                        Math.Min(Config.TabActiveColor.X * 1.2f, 1.0f),
                        Math.Min(Config.TabActiveColor.Y * 1.2f, 1.0f),
                        Math.Min(Config.TabActiveColor.Z * 1.2f, 1.0f),
                        Config.TabActiveColor.W
                    ));

                    // Add bottom border to indicate active tab
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0);
                }
                else
                {
                    // Inactive tab styling
                    ImGui.PushStyleColor(ImGuiCol.Button, ImGuiHelper.DefaultButtonColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Config.TabHoverColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(
                        Math.Min(Config.TabHoverColor.X * 1.2f, 1.0f),
                        Math.Min(Config.TabHoverColor.Y * 1.2f, 1.0f),
                        Math.Min(Config.TabHoverColor.Z * 1.2f, 1.0f),
                        Config.TabHoverColor.W
                    ));

                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1);
                }

                // Draw the tab button
                if (ImGui.Button(Config.Tabs[i].Name, new Vector2(tabWidth, tabHeight)))
                {
                    Config.ActiveTab = i;
                    activeTab = i;
                    Plugin.Configuration.Save();
                }

                ImGui.PopStyleColor(3);
                ImGui.PopStyleVar();
            }

            // End the scrollable area if we created one
            if (tabWidth * Config.Tabs.Count + tabSpacing * (Config.Tabs.Count - 1) > availableWidth)
            {
                ImGui.EndChild();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Draw buttons for active tab
            var buttonIndices = Config.Tabs[activeTab].ButtonIndices;

            // Choose the layout for buttons within the tab
            if (buttonIndices.Count > 0)
            {
                DrawTabButtonsInGrid(buttonIndices);
            }

            ImGui.EndGroup();
        }

        private void DrawTabButtonsInGrid(System.Collections.Generic.List<int> buttonIndices)
        {
            // Calculate optimal grid layout based on number of buttons and their sizes
            int maxButtonsPerRow = 4;  // Maximum buttons per row for readability

            // Measure button sizes to determine optimal layout
            float totalWidth = ImGui.GetContentRegionAvail().X;

            // Find widest button to determine column count
            float maxButtonWidth = 0;
            for (int i = 0; i < buttonIndices.Count; i++)
            {
                int index = buttonIndices[i];
                if (index >= 0 && index < Config.Buttons.Count)
                {
                    maxButtonWidth = Math.Max(maxButtonWidth, Config.Buttons[index].Width);
                }
            }

            // Add spacing between buttons
            maxButtonWidth += 10; // 10px spacing between buttons

            // Calculate how many buttons can fit per row
            int buttonsPerRow = Math.Min(
                maxButtonsPerRow,
                Math.Max(1, (int)(totalWidth / maxButtonWidth))
            );

            // Draw buttons in a grid layout
            ImGui.BeginGroup();

            int currentColumn = 0;
            for (int i = 0; i < buttonIndices.Count; i++)
            {
                int buttonIndex = buttonIndices[i];

                if (buttonIndex >= 0 && buttonIndex < Config.Buttons.Count)
                {
                    // Only add SameLine after the first button in a row
                    if (currentColumn > 0)
                    {
                        ImGui.SameLine();
                    }

                    // Render the button
                    RenderButton(Config.Buttons[buttonIndex]);

                    // Update column counter and create a new row if needed
                    currentColumn++;
                    if (currentColumn >= buttonsPerRow)
                    {
                        currentColumn = 0;
                        ImGui.Spacing();
                        ImGui.Spacing(); // Double spacing for better visual separation
                    }
                }
            }

            ImGui.EndGroup();
        }

        private void PreDrawExpandingLayout()
        {
            if (Config.Buttons.Count == 0) return;

            int mainIndex = Math.Min(Config.MainButtonIndex, Config.Buttons.Count - 1);
            var mainButton = Config.Buttons[mainIndex];

            if (Config.ExpansionDirection == Configuration.ExpandDirection.Left)
            {
                // Calculate space needed for left expansion
                float totalWidth = 0;
                foreach (var button in Config.Buttons)
                {
                    if (Config.Buttons.IndexOf(button) == mainIndex) continue;
                    totalWidth += button.Width + 5;
                }

                // Create a dummy element to reserve space
                ImGui.SetCursorPos(new Vector2(0, 0));
                ImGui.Dummy(new Vector2(totalWidth, mainButton.Height));
            }
            else if (Config.ExpansionDirection == Configuration.ExpandDirection.Up)
            {
                // Calculate space needed for upward expansion
                float totalHeight = 0;
                foreach (var button in Config.Buttons)
                {
                    if (Config.Buttons.IndexOf(button) == mainIndex) continue;
                    totalHeight += button.Height + 5;
                }

                // Create a dummy element to reserve space
                ImGui.SetCursorPos(new Vector2(0, 0));
                ImGui.Dummy(new Vector2(mainButton.Width, totalHeight));
            }
        }

        // Position tracking variables for each state
        private Vector2 collapsedWindowPos = Vector2.Zero;
        private Vector2 expandedWindowPos = Vector2.Zero;
        private Vector2 collapsedMenuPos = Vector2.Zero;
        private Vector2 expandedMenuPos = Vector2.Zero;
        private bool positionsInitialized = false;
        private bool isResettingPosition = false;

        private void DrawExpandingLayout()
        {
            if (Config.Buttons.Count == 0) return;

            int mainIndex = Math.Min(Config.MainButtonIndex, Config.Buttons.Count - 1);
            var mainButton = Config.Buttons[mainIndex];

            string directionIndicator = Config.ExpansionDirection switch
            {
                Configuration.ExpandDirection.Right => " →",
                Configuration.ExpandDirection.Left => " ←",
                Configuration.ExpandDirection.Down => " ↓",
                Configuration.ExpandDirection.Up => " ↑",
                _ => " ↓"
            };

            // Main Button - Click to toggle expansion
            ImGui.PushID("MenuButton");

            // Use the menu button's color settings rather than hardcoded colors
            Vector4 menuButtonColor = mainButton.Color;
            Vector4 menuButtonHoveredColor = menuButtonColor * 1.2f;
            Vector4 menuButtonActiveColor = menuButtonColor * 0.8f;
            Vector4 menuButtonTextColor = mainButton.LabelColor;

            ImGui.PushStyleColor(ImGuiCol.Button, menuButtonColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, menuButtonHoveredColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, menuButtonActiveColor);
            ImGui.PushStyleColor(ImGuiCol.Text, menuButtonTextColor);

            Vector2 menuButtonSize = new Vector2(mainButton.Width, mainButton.Height);

            // Position the menu button appropriately when expanded
            if (Config.IsExpanded)
            {
                if (Config.ExpansionDirection == Configuration.ExpandDirection.Left)
                {
                    float totalWidth = 0;
                    for (int i = 0; i < Config.Buttons.Count; i++)
                    {
                        if (i != mainIndex)
                        {
                            totalWidth += Config.Buttons[i].Width + 5;
                        }
                    }
                    ImGui.SetCursorPosX(totalWidth);
                }
                else if (Config.ExpansionDirection == Configuration.ExpandDirection.Up)
                {
                    float totalHeight = 0;
                    for (int i = 0; i < Config.Buttons.Count; i++)
                    {
                        if (i != mainIndex)
                        {
                            totalHeight += Config.Buttons[i].Height + 5;
                        }
                    }
                    ImGui.SetCursorPosY(totalHeight);
                }
            }

            // Use a custom label for the menu button that includes the direction indicator
            string menuButtonLabel = "Menu" + directionIndicator;
            if (!string.IsNullOrEmpty(mainButton.Label) && mainButton.Label != "New Button")
            {
                // If the user has set a custom label, use that instead of "Menu"
                menuButtonLabel = mainButton.Label + directionIndicator;
            }

            if (ImGui.Button(menuButtonLabel, menuButtonSize))
            {
                // We're about to toggle the expansion state
                if (!Config.IsExpanded)
                {
                    // Transitioning from collapsed to expanded
                    if (!positionsInitialized)
                    {
                        // First time opening - initialize positions
                        collapsedWindowPos = ImGui.GetWindowPos();
                        collapsedMenuPos = ImGui.GetItemRectMin();
                        positionsInitialized = true;
                    }

                    // Remember the collapsed state values each time
                    collapsedWindowPos = ImGui.GetWindowPos();
                    collapsedMenuPos = ImGui.GetItemRectMin();
                }
                else
                {
                    // Transitioning from expanded to collapsed
                    expandedWindowPos = ImGui.GetWindowPos();
                    expandedMenuPos = ImGui.GetItemRectMin();

                    // Reset window position on next frame for left/up
                    if (Config.ExpansionDirection == Configuration.ExpandDirection.Left ||
                        Config.ExpansionDirection == Configuration.ExpandDirection.Up)
                    {
                        isResettingPosition = true;
                    }
                }

                // Toggle the expansion state
                Config.IsExpanded = !Config.IsExpanded;
                Plugin.Configuration.Save();
            }

            ImGui.PopStyleColor(4);
            ImGui.PopID();

            // Store menu button position for layout calculations
            Vector2 menuPos = ImGui.GetItemRectMin();
            Vector2 menuSize = menuButtonSize;

            // Expanded Buttons
            if (Config.IsExpanded)
            {
                switch (Config.ExpansionDirection)
                {
                    case Configuration.ExpandDirection.Left:
                        DrawLeftExpandingLayout(menuPos, menuSize);
                        break;
                    case Configuration.ExpandDirection.Right:
                        DrawRightExpandingLayout(menuPos, menuSize);
                        break;
                    case Configuration.ExpandDirection.Up:
                        DrawUpExpandingLayout(menuPos, menuSize);
                        break;
                    case Configuration.ExpandDirection.Down:
                        DrawDownExpandingLayout(menuPos, menuSize);
                        break;
                }
            }
        }

        private void DrawLeftExpandingLayout(Vector2 menuPos, Vector2 menuSize)
        {
            float posX = menuPos.X;
            float posY = menuPos.Y;

            // Draw buttons right-to-left starting from the menu position
            ImGui.BeginGroup();

            // Sort buttons by index to maintain consistent ordering
            var buttonList = new System.Collections.Generic.List<(int index, Configuration.ButtonData button)>();

            for (int i = 0; i < Config.Buttons.Count; i++)
            {
                if (i != Config.MainButtonIndex)
                {
                    buttonList.Add((i, Config.Buttons[i]));
                }
            }

            // Draw each button in order
            foreach (var (_, button) in buttonList)
            {
                posX -= button.Width + 5;

                // Set the cursor position relative to the window
                ImGui.SetCursorPos(new Vector2(posX - ImGui.GetWindowPos().X, posY - ImGui.GetWindowPos().Y));
                RenderButton(button);
            }

            ImGui.EndGroup();
        }

        private void DrawRightExpandingLayout(Vector2 menuPos, Vector2 menuSize)
        {
            float posX = menuPos.X + menuSize.X + 5;
            float posY = menuPos.Y;

            ImGui.BeginGroup();
            for (int i = 0; i < Config.Buttons.Count; i++)
            {
                if (i == Config.MainButtonIndex) continue;

                var button = Config.Buttons[i];

                // Set the cursor position relative to the window
                ImGui.SetCursorPos(new Vector2(posX - ImGui.GetWindowPos().X, posY - ImGui.GetWindowPos().Y));
                RenderButton(button);

                posX += button.Width + 5;
            }
            ImGui.EndGroup();
        }

        private void DrawUpExpandingLayout(Vector2 menuPos, Vector2 menuSize)
        {
            float posX = menuPos.X;
            float posY = menuPos.Y;

            // Draw buttons bottom-to-top starting from the menu position
            ImGui.BeginGroup();

            // Sort buttons by index to maintain consistent ordering
            var buttonList = new System.Collections.Generic.List<(int index, Configuration.ButtonData button)>();

            for (int i = 0; i < Config.Buttons.Count; i++)
            {
                if (i != Config.MainButtonIndex)
                {
                    buttonList.Add((i, Config.Buttons[i]));
                }
            }

            // Draw each button in order
            foreach (var (_, button) in buttonList)
            {
                posY -= button.Height + 5;

                // Set the cursor position relative to the window
                ImGui.SetCursorPos(new Vector2(posX - ImGui.GetWindowPos().X, posY - ImGui.GetWindowPos().Y));
                RenderButton(button);
            }

            ImGui.EndGroup();
        }

        private void DrawDownExpandingLayout(Vector2 menuPos, Vector2 menuSize)
        {
            float posX = menuPos.X;
            float posY = menuPos.Y + menuSize.Y + 5;

            ImGui.BeginGroup();
            for (int i = 0; i < Config.Buttons.Count; i++)
            {
                if (i == Config.MainButtonIndex) continue;

                var button = Config.Buttons[i];

                // Set the cursor position relative to the window
                ImGui.SetCursorPos(new Vector2(posX - ImGui.GetWindowPos().X, posY - ImGui.GetWindowPos().Y));
                RenderButton(button);

                posY += button.Height + 5;
            }
            ImGui.EndGroup();
        }

        private void RenderButton(Configuration.ButtonData button)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, button.Color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, button.Color * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, button.Color * 0.8f);
            ImGui.PushStyleColor(ImGuiCol.Text, button.LabelColor);

            ImGui.Button(button.Label, new Vector2(button.Width, button.Height));

            ImGui.PopStyleColor(4);
        }

        private bool IsPositionDifferent(Vector2 current, Vector2 saved)
        {
            return Math.Abs(current.X - saved.X) > 0.5f || Math.Abs(current.Y - saved.Y) > 0.5f;
        }
    }
}
