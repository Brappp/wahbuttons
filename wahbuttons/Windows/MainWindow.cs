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
    private string? activeTab = null;

    // This will now get button windows from the plugin
    public List<ButtonWindow> ButtonWindows => Plugin.ButtonWindows.Values.ToList();

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

    public void SetActiveTab(string tabName)
    {
        activeTab = tabName;
    }

    public override void Draw()
    {
        WindowManager.DrawWindowManagement();

        // Removed spacing and separator

        if (ImGui.BeginTabBar("WindowConfigTabs"))
        {
            var sortedWindows = ButtonWindows.OrderBy(w => w.Config.Name).ToList();

            foreach (var window in sortedWindows)
            {
                var isActive = activeTab == window.Config.Name;
                if (isActive)
                {
                    ImGui.SetNextItemOpen(true);
                }

                var flags = isActive ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

                // Use a dummy 'open' variable that we don't check
                bool open = true;
                if (ImGui.BeginTabItem($"{window.Config.Name}##tab", ref open, flags))
                {
                    if (isActive)
                    {
                        activeTab = null;  // Reset after tab is focused
                    }
                    DrawWindowSettings(window);
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }

    private void DrawWindowSettings(ButtonWindow window)
    {
        // Window Settings Section - removed the "Window Settings" text to condense
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
        Plugin.CreateButtonWindow(config);
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
