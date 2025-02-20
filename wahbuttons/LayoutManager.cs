using ImGuiNET;
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
        ImGui.Text("Button Layout:");
        ImGui.SameLine();

        DrawLayoutRadioButtons(window);

        if (window.Config.Layout == Configuration.ButtonLayout.Grid)
        {
            DrawGridSettings(window);
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
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
    }

    private void DrawGridSettings(ButtonWindow window)
    {
        ImGui.Indent(10);
        ImGui.PushItemWidth(120);

        int rows = window.Config.GridRows;
        if (ImGui.DragInt("Grid Rows", ref rows, 0.1f, 1, 10))
        {
            window.Config.GridRows = System.Math.Max(1, rows);
            Configuration.Save();
        }

        int columns = window.Config.GridColumns;
        if (ImGui.DragInt("Grid Columns", ref columns, 0.1f, 1, 10))
        {
            window.Config.GridColumns = System.Math.Max(1, columns);
            Configuration.Save();
        }

        ImGui.PopItemWidth();
        ImGui.Unindent(10);
    }
}
