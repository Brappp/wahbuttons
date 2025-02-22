using ImGuiNET;
using System.Numerics;
using WahButtons.Helpers;
using WahButtons.Windows;
using Dalamud.Interface;

namespace WahButtons.UI;

public class WindowTabManager
{
    private readonly Configuration Configuration;

    public WindowTabManager(Configuration configuration)
    {
        Configuration = configuration;
    }

    public void DrawWindowTab(ButtonWindow window)
    {
        var config = window.Config;

        // Window Name input
        var name = config.Name;
        if (ImGui.InputText("Window Name", ref name, 32))
        {
            config.Name = name;
            Configuration.Save();
        }

        ImGui.Spacing();

        // Window Options - Horizontal Layout
        var isVisible = config.IsVisible;
        if (ImGui.Checkbox("Show Window", ref isVisible))
        {
            config.IsVisible = isVisible;
            window.IsOpen = isVisible;
            Configuration.Save();
        }

        ImGui.SameLine(200);
        var isLocked = config.IsLocked;
        if (ImGui.Checkbox("Lock Position", ref isLocked))
        {
            config.IsLocked = isLocked;
            Configuration.Save();
        }

        ImGui.SameLine(400);
        var transparentBg = config.TransparentBackground;
        if (ImGui.Checkbox("Transparent", ref transparentBg))
        {
            config.TransparentBackground = transparentBg;
            Configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Layout Selection
        ImGui.Text("Button Layout:");

        var layout = config.Layout;
        if (ImGui.RadioButton("Vertical", layout == Configuration.ButtonLayout.Vertical))
        {
            config.Layout = Configuration.ButtonLayout.Vertical;
            Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Horizontal", layout == Configuration.ButtonLayout.Horizontal))
        {
            config.Layout = Configuration.ButtonLayout.Horizontal;
            Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Grid", layout == Configuration.ButtonLayout.Grid))
        {
            config.Layout = Configuration.ButtonLayout.Grid;
            Configuration.Save();
        }

        if (layout == Configuration.ButtonLayout.Grid)
        {
            ImGui.PushItemWidth(100);

            // Grid Rows with Up/Down buttons
            var rows = config.GridRows;
            ImGui.Text("Grid Rows:");
            ImGui.SameLine();
            if (ImGui.Button("-##rows") && rows > 1)
            {
                config.GridRows--;
                Configuration.Save();
            }
            ImGui.SameLine();
            if (ImGui.DragInt("##rows", ref rows, 0.1f, 1, 10))
            {
                config.GridRows = rows;
                Configuration.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("+##rows") && rows < 10)
            {
                config.GridRows++;
                Configuration.Save();
            }

            // Grid Columns with Up/Down buttons
            var cols = config.GridColumns;
            ImGui.Text("Grid Columns:");
            ImGui.SameLine();
            if (ImGui.Button("-##cols") && cols > 1)
            {
                config.GridColumns--;
                Configuration.Save();
            }
            ImGui.SameLine();
            if (ImGui.DragInt("##cols", ref cols, 0.1f, 1, 10))
            {
                config.GridColumns = cols;
                Configuration.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("+##cols") && cols < 10)
            {
                config.GridColumns++;
                Configuration.Save();
            }

            ImGui.PopItemWidth();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // Button Management
        if (ImGui.Button("Add New Button"))
        {
            config.Buttons.Add(new Configuration.ButtonData("New Button", "/command WAH!", 85));
            Configuration.Save();
        }

        ImGui.Spacing();

        // Button List Table
        if (ImGui.BeginTable("##buttons", 5, ImGuiTableFlags.BordersInnerH))
        {
            ImGui.TableSetupColumn("##order", ImGuiTableColumnFlags.WidthFixed, 80);
            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("##actions", ImGuiTableColumnFlags.WidthFixed, 30);

            for (int i = 0; i < config.Buttons.Count; i++)
            {
                ImGui.TableNextRow();
                var button = config.Buttons[i];

                // Move/Order Column
                ImGui.TableNextColumn();
                ImGui.PushID($"move_{i}");
                if (i > 0)
                {
                    if (ImGui.Button("▲"))
                    {
                        var temp = config.Buttons[i];
                        config.Buttons[i] = config.Buttons[i - 1];
                        config.Buttons[i - 1] = temp;
                        Configuration.Save();
                    }
                    ImGui.SameLine();
                }
                if (i < config.Buttons.Count - 1)
                {
                    if (ImGui.Button("▼"))
                    {
                        var temp = config.Buttons[i];
                        config.Buttons[i] = config.Buttons[i + 1];
                        config.Buttons[i + 1] = temp;
                        Configuration.Save();
                    }
                }
                ImGui.PopID();

                // Label Column
                ImGui.TableNextColumn();
                ImGui.PushID($"label_{i}");
                var label = button.Label;
                if (ImGui.InputText("##label", ref label, 32))
                {
                    button.Label = label;
                    Configuration.Save();
                }
                ImGui.PopID();

                // Command Column
                ImGui.TableNextColumn();
                ImGui.PushID($"command_{i}");
                var command = button.Command;
                if (ImGui.InputText("##command", ref command, 64))
                {
                    button.Command = command;
                    Configuration.Save();
                }
                ImGui.PopID();

                // Size Column
                ImGui.TableNextColumn();
                ImGui.PushID($"size_{i}");
                var size = new Vector2(button.Width, button.Height);
                if (ImGui.DragFloat2("##size", ref size, 1, 20, 500))
                {
                    button.Width = size.X;
                    button.Height = size.Y;
                    Configuration.Save();
                }
                ImGui.PopID();

                // Delete Column
                ImGui.TableNextColumn();
                ImGui.PushID($"delete_{i}");
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.3f, 0.3f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.7f, 0.1f, 0.1f, 1.0f));
                if (ImGui.Button("X"))
                {
                    config.Buttons.RemoveAt(i);
                    Configuration.Save();
                    i--;
                }
                ImGui.PopStyleColor(3);
                ImGui.PopID();
            }
            ImGui.EndTable();
        }

        ImGui.Spacing();
    }
}
