using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using WahButtons.Windows;
using WahButtons.Helpers;
using WahButtons.UI.Components;

namespace WahButtons;

public class MainWindow : Window, IDisposable
{
    public readonly Plugin Plugin;
    private readonly Configuration Configuration;
    private readonly WindowSystem WindowSystem;
    private readonly WindowManager WindowManager;
    private readonly ButtonListManager ButtonListManager;
    private readonly DefaultSizeManager DefaultSizeManager;
    private readonly LayoutManager LayoutManager;

    // Make this public so WindowManager can access it
    public string? ActiveTabName { get; set; } = null;

    public List<ButtonWindow> ButtonWindows { get; } = new();

    public MainWindow(Plugin plugin, Configuration configuration, WindowSystem windowSystem)
        : base("Wah Buttons Configuration")
    {
        Plugin = plugin;
        Configuration = configuration;
        WindowSystem = windowSystem;

        WindowManager = new WindowManager(this, Configuration, WindowSystem);
        ButtonListManager = new ButtonListManager(Configuration);
        DefaultSizeManager = new DefaultSizeManager(Configuration, ButtonWindows);
        LayoutManager = new LayoutManager(Configuration, Plugin);

        Size = new Vector2(800, 600);
        SizeCondition = ImGuiCond.FirstUseEver;

        foreach (var config in Configuration.Windows)
        {
            AddButtonWindowFromConfig(config);
        }
    }

    public void Dispose()
    {
        SaveAllButtonWindows();
    }

    public override void PreDraw()
    {
        base.PreDraw();
        ImGuiHelper.PushWahButtonsStyle();
    }

    public override void PostDraw()
    {
        ImGuiHelper.PopWahButtonsStyle();
        base.PostDraw();
    }

    public override void Draw()
    {
        WindowManager.DrawWindowManagement();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Add extra spacing for tab area
        ImGui.Dummy(new Vector2(0, 5));

        // Draw window tabs
        if (ImGui.BeginTabBar("WindowConfigTabs", ImGuiTabBarFlags.None))
        {
            bool foundActiveTab = false;

            // Sort windows for consistent ordering
            var windows = ButtonWindows.OrderBy(w => w.Config.Name).ToList();

            foreach (var window in windows)
            {
                // Using a reference parameter for open state
                bool isOpen = true;
                ImGuiTabItemFlags flags = ImGuiTabItemFlags.None;

                // If this is the tab we want to activate
                if (ActiveTabName == window.Config.Name)
                {
                    flags = ImGuiTabItemFlags.SetSelected;
                }

                // Begin tab item with the required ref parameter
                if (ImGui.BeginTabItem($"{window.Config.Name}##tab", ref isOpen, flags))
                {
                    // If this was our active tab, mark it as found and clear it
                    if (ActiveTabName == window.Config.Name)
                    {
                        foundActiveTab = true;
                        ActiveTabName = null;
                    }

                    // Draw the tab content
                    DrawWindowSettings(window);
                    ImGui.EndTabItem();
                }
            }

            // If we didn't find the active tab, clear it (may have been removed)
            if (!foundActiveTab && ActiveTabName != null)
            {
                ActiveTabName = null;
            }

            ImGui.EndTabBar();
        }

        ImGui.Dummy(new Vector2(0, 5));
    }

    private void DrawWindowSettings(ButtonWindow window)
    {
        // Window Settings Section
        ImGui.Text("Window Settings");
        ImGui.Indent(10);

        WindowManager.DrawWindowBasicSettings(window);

        ImGui.Unindent(10);
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        DefaultSizeManager.DrawDefaultButtonSize(window);
        DrawLayoutSettings(window);
        ButtonListManager.DrawButtonList(window);
    }

    private void DrawLayoutSettings(ButtonWindow window)
    {
        LayoutManager.DrawLayoutSettings(window);
    }

    public void AddButtonWindowFromConfig(Configuration.ButtonWindowConfig config)
    {
        var window = new ButtonWindow(Plugin, config)
        {
            IsOpen = config.IsVisible
        };
        ButtonWindows.Add(window);
        WindowSystem.AddWindow(window);
    }

    public void SaveAllButtonWindows()
    {
        foreach (var window in ButtonWindows)
        {
            if (!Configuration.Windows.Contains(window.Config))
            {
                Configuration.Windows.Add(window.Config);
            }
        }
        Configuration.Save();
    }
}
