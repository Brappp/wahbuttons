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

            ImGui.BeginGroup();

            // Draw tabs
            for (int i = 0; i < Config.Tabs.Count; i++)
            {
                if (i > 0) ImGui.SameLine();

                // Apply custom tab styling
                if (i == activeTab)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Config.TabActiveColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Config.TabActiveColor * 1.1f);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, Config.TabActiveColor * 1.2f);
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, ImGuiHelper.DefaultButtonColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Config.TabHoverColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, Config.TabHoverColor * 1.2f);
                }

                if (ImGui.Button(Config.Tabs[i].Name))
                {
                    Config.ActiveTab = i;
                    activeTab = i;
                    Plugin.Configuration.Save();
                }

                ImGui.PopStyleColor(3);
            }

            ImGui.Spacing();

            // Draw buttons for active tab
            var buttonIndices = Config.Tabs[activeTab].ButtonIndices;
            ImGui.BeginGroup();
            for (int i = 0; i < buttonIndices.Count; i++)
            {
                int index = buttonIndices[i];
                if (index >= 0 && index < Config.Buttons.Count)
                {
                    RenderButton(Config.Buttons[index]);
                    ImGui.Spacing();
                }
            }
            ImGui.EndGroup();

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
            ImGui.PushStyleColor(ImGuiCol.Button, mainButton.Color);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, mainButton.Color * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, mainButton.Color * 0.8f);
            ImGui.PushStyleColor(ImGuiCol.Text, mainButton.LabelColor);

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

            if (ImGui.Button("Menu" + directionIndicator, menuButtonSize))
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
