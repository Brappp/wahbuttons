using ImGuiNET;
using System;
using System.Numerics;
using System.Linq;
using WahButtons.Windows;

namespace WahButtons.UI.Components;

public class LayoutManager
{
    private readonly Configuration Configuration;
    private readonly Plugin Plugin;

    public LayoutManager(Configuration configuration, Plugin plugin)
    {
        Configuration = configuration;
        Plugin = plugin;
    }

    public void DrawLayoutSettings(ButtonWindow window)
    {
        // Move "Button Layout" text above the options
        ImGui.Text("Button Layout:");
        ImGui.Spacing();

        ImGui.Indent(10);
        DrawLayoutRadioButtons(window);
        ImGui.Unindent(10);

        // Display settings specific to each layout
        switch (window.Config.Layout)
        {
            case Configuration.ButtonLayout.Grid:
                DrawGridSettings(window);
                break;
            case Configuration.ButtonLayout.Expanding:
                DrawExpandingSettings(window);
                break;
            case Configuration.ButtonLayout.Tabbed:
                DrawTabbedSettings(window);
                break;
        }
    }

    private void DrawLayoutRadioButtons(ButtonWindow window)
    {
        if (ImGui.RadioButton("Vertical", window.Config.Layout == Configuration.ButtonLayout.Vertical))
        {
            window.Config.Layout = Configuration.ButtonLayout.Vertical;
            Configuration.Save();
        }
        ImGui.SameLine();

        if (ImGui.RadioButton("Horizontal", window.Config.Layout == Configuration.ButtonLayout.Horizontal))
        {
            window.Config.Layout = Configuration.ButtonLayout.Horizontal;
            Configuration.Save();
        }
        ImGui.SameLine();

        if (ImGui.RadioButton("Grid", window.Config.Layout == Configuration.ButtonLayout.Grid))
        {
            window.Config.Layout = Configuration.ButtonLayout.Grid;
            Configuration.Save();
        }
        ImGui.SameLine();

        if (ImGui.RadioButton("Expanding", window.Config.Layout == Configuration.ButtonLayout.Expanding))
        {
            window.Config.Layout = Configuration.ButtonLayout.Expanding;
            if (window.Config.Buttons.Count > 0)
            {
                EnsureExpandingLayout(window);
            }
            Configuration.Save();
        }
        ImGui.SameLine();

        if (ImGui.RadioButton("Tabbed", window.Config.Layout == Configuration.ButtonLayout.Tabbed))
        {
            window.Config.Layout = Configuration.ButtonLayout.Tabbed;
            if (window.Config.Buttons.Count > 0)
            {
                EnsureTabbedLayout(window);
            }
            Configuration.Save();
        }
    }

    private void DrawGridSettings(ButtonWindow window)
    {
        ImGui.Indent(10);
        ImGui.PushItemWidth(120);

        int rows = window.Config.GridRows;
        if (ImGui.DragInt("Grid Rows", ref rows, 0.1f, 1, 10))
        {
            window.Config.GridRows = Math.Max(1, rows);
            Configuration.Save();
        }

        int columns = window.Config.GridColumns;
        if (ImGui.DragInt("Grid Columns", ref columns, 0.1f, 1, 10))
        {
            window.Config.GridColumns = Math.Max(1, columns);
            Configuration.Save();
        }

        ImGui.PopItemWidth();
        ImGui.Unindent(10);
    }

    private void DrawExpandingSettings(ButtonWindow window)
    {
        ImGui.Indent(10);
        ImGui.PushItemWidth(120);

        // Make sure we have the required data structure for expanding layout
        EnsureExpandingLayout(window);

        // Expansion Direction Setting
        ImGui.Text("Expand Direction:");

        bool isRightExpand = window.Config.ExpansionDirection == Configuration.ExpandDirection.Right;
        if (ImGui.RadioButton("→ Right", isRightExpand))
        {
            window.Config.ExpansionDirection = Configuration.ExpandDirection.Right;
            Configuration.Save();
        }

        ImGui.SameLine();
        bool isDownExpand = window.Config.ExpansionDirection == Configuration.ExpandDirection.Down;
        if (ImGui.RadioButton("↓ Down", isDownExpand))
        {
            window.Config.ExpansionDirection = Configuration.ExpandDirection.Down;
            Configuration.Save();
        }

        ImGui.SameLine();
        bool isLeftExpand = window.Config.ExpansionDirection == Configuration.ExpandDirection.Left;
        if (ImGui.RadioButton("← Left", isLeftExpand))
        {
            window.Config.ExpansionDirection = Configuration.ExpandDirection.Left;
            Configuration.Save();
        }

        ImGui.SameLine();
        bool isUpExpand = window.Config.ExpansionDirection == Configuration.ExpandDirection.Up;
        if (ImGui.RadioButton("↑ Up", isUpExpand))
        {
            window.Config.ExpansionDirection = Configuration.ExpandDirection.Up;
            Configuration.Save();
        }

        // Move preview checkbox next to the Up button
        ImGui.SameLine(0, 30);
        bool isExpanded = window.Config.IsExpanded;
        if (ImGui.Checkbox("Show Preview", ref isExpanded))
        {
            window.Config.IsExpanded = isExpanded;
            Configuration.Save();
        }

        ImGui.PopItemWidth();
        ImGui.Unindent(10);
    }

    // This is a partial update of the DrawTabbedSettings method in LayoutManager.cs

    private void DrawTabbedSettings(ButtonWindow window)
    {
        ImGui.Indent(10);

        // Make sure we have a default tab if none exist
        EnsureTabbedLayout(window);

        // Tab color configuration
        ImGui.Text("Tab Style:");
        ImGui.SameLine();

        // Active tab color
        Vector4 tabActiveColor = window.Config.TabActiveColor;
        if (ImGui.ColorEdit4("Active##tabcolor", ref tabActiveColor,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
        {
            window.Config.TabActiveColor = tabActiveColor;
            Configuration.Save();
        }

        ImGui.SameLine();

        // Tab hover color
        Vector4 tabHoverColor = window.Config.TabHoverColor;
        if (ImGui.ColorEdit4("Hover##tabcolor", ref tabHoverColor,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
        {
            window.Config.TabHoverColor = tabHoverColor;
            Configuration.Save();
        }

        // Add a hint for the color pickers
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Tab Colors (Active/Hover)");
        }

        ImGui.Spacing();

        // Tab management section with improved layout
        ImGui.Text("Tab Management:");
        ImGui.BeginGroup();
        {
            // Add New Tab button
            if (ImGui.Button("Add New Tab"))
            {
                int suffix = 1;
                string tabName = "Tab";
                while (window.Config.Tabs.Exists(t => t.Name == tabName))
                {
                    tabName = $"Tab {suffix++}";
                }
                window.Config.Tabs.Add(new Configuration.TabData(tabName));
                Configuration.Save();
            }

            // Add Active Tab selector next to Add New Tab button
            ImGui.SameLine(150);
            ImGui.Text("Currently Editing:");
            ImGui.SameLine();

            // Preview active tab selector
            int activeTab = window.Config.ActiveTab;
            ImGui.SetNextItemWidth(150);
            if (ImGui.Combo("##ActiveTabEditor", ref activeTab, GetTabNames(window.Config.Tabs), window.Config.Tabs.Count))
            {
                window.Config.ActiveTab = activeTab;
                Configuration.Save();
            }
        }
        ImGui.EndGroup();

        ImGui.Spacing();

        // Tab list with buttons to edit/delete
        if (ImGui.BeginTable("##tabs", 3, ImGuiTableFlags.BordersInnerH))
        {
            ImGui.TableSetupColumn("Tab Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Buttons", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("##actions", ImGuiTableColumnFlags.WidthFixed, 80);
            ImGui.TableHeadersRow();

            for (int i = 0; i < window.Config.Tabs.Count; i++)
            {
                var tab = window.Config.Tabs[i];
                ImGui.TableNextRow();

                // Tab Name
                ImGui.TableNextColumn();
                string tabName = tab.Name;
                ImGui.PushID($"tab_name_{i}");
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputText("##tabname", ref tabName, 32))
                {
                    tab.Name = tabName;
                    Configuration.Save();
                }
                ImGui.PopID();

                // Button Count
                ImGui.TableNextColumn();
                ImGui.Text($"{tab.ButtonIndices.Count} buttons");

                // Actions
                ImGui.TableNextColumn();
                ImGui.PushID($"tab_actions_{i}");
                if (ImGui.Button("Delete") && window.Config.Tabs.Count > 1)
                {
                    window.Config.Tabs.RemoveAt(i);
                    if (window.Config.ActiveTab >= window.Config.Tabs.Count)
                    {
                        window.Config.ActiveTab = window.Config.Tabs.Count - 1;
                    }
                    Configuration.Save();
                    i--;
                }
                ImGui.PopID();
            }
            ImGui.EndTable();
        }

        ImGui.Spacing();
        ImGui.Text("Button Assignment:");

        // Tab & Button Assignment
        ImGui.Indent(10);
        for (int i = 0; i < window.Config.Buttons.Count; i++)
        {
            var button = window.Config.Buttons[i];
            ImGui.PushID($"btn_assign_{i}");

            // Show button label
            ImGui.Text($"{button.Label}:");

            ImGui.SameLine(150);

            // Dropdown to select tab
            int tabIndex = -1;
            for (int t = 0; t < window.Config.Tabs.Count; t++)
            {
                if (window.Config.Tabs[t].ButtonIndices.Contains(i))
                {
                    tabIndex = t;
                    break;
                }
            }

            ImGui.SetNextItemWidth(150);
            if (ImGui.Combo($"##tab_for_btn_{i}", ref tabIndex, GetTabNames(window.Config.Tabs), window.Config.Tabs.Count))
            {
                // First remove this button from all tabs
                foreach (var tab in window.Config.Tabs)
                {
                    tab.ButtonIndices.Remove(i);
                }

                // Now add it to the selected tab
                if (tabIndex >= 0 && tabIndex < window.Config.Tabs.Count)
                {
                    window.Config.Tabs[tabIndex].ButtonIndices.Add(i);
                }

                Configuration.Save();
            }

            ImGui.PopID();
        }
        ImGui.Unindent(10);

        ImGui.Unindent(10);
    }

    // Helper methods for dropdown options
    private string GetButtonLabels(System.Collections.Generic.List<Configuration.ButtonData> buttons)
    {
        string result = "";
        for (int i = 0; i < buttons.Count; i++)
        {
            result += buttons[i].Label + '\0';
        }
        return result;
    }

    private string GetTabNames(System.Collections.Generic.List<Configuration.TabData> tabs)
    {
        string result = "";
        foreach (var tab in tabs)
        {
            result += tab.Name + '\0';
        }
        return result;
    }

    // Helper methods for layout data structure setup
    private void EnsureExpandingLayout(ButtonWindow window)
    {
        // Make sure we set a default main button if none is set
        if (window.Config.MainButtonIndex >= window.Config.Buttons.Count && window.Config.Buttons.Count > 0)
        {
            window.Config.MainButtonIndex = 0;
        }
    }

    private void EnsureTabbedLayout(ButtonWindow window)
    {
        // Create a default tab if none exist
        if (window.Config.Tabs.Count == 0)
        {
            var defaultTab = new Configuration.TabData("General");

            // Add all existing buttons to the default tab
            for (int i = 0; i < window.Config.Buttons.Count; i++)
            {
                defaultTab.ButtonIndices.Add(i);
            }

            window.Config.Tabs.Add(defaultTab);
            window.Config.ActiveTab = 0;
        }
    }
}
