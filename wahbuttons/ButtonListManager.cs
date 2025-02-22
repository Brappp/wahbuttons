using System.Numerics;
using ImGuiNET;
using WahButtons.Windows;
using WahButtons.Helpers;
using System.Collections.Generic;

namespace WahButtons.UI.Components;

public class ButtonListManager
{
    private readonly Configuration Configuration;

    public ButtonListManager(Configuration configuration)
    {
        Configuration = configuration;
    }

    public void DrawButtonList(ButtonWindow window)
    {
        if (ImGui.Button("Add New Button", new Vector2(120, 25)))
        {
            window.Config.Buttons.Add(new Configuration.ButtonData("New Button", "/command WAH!", 75));
            Configuration.Save();
        }

        ImGui.Spacing();

        DrawButtonListHeaders();
        DrawButtonListContent(window);
    }

    private void DrawButtonListHeaders()
    {
        if (ImGui.BeginTable("##buttonHeaders", 6, ImGuiTableFlags.None))
        {
            ImGui.TableSetupColumn("##order", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableSetupColumn("Size (W × H)", ImGuiTableColumnFlags.WidthFixed, 110);
            ImGui.TableSetupColumn("Colors", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("##actions", ImGuiTableColumnFlags.WidthFixed, 10);

            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
            ImGui.TableNextColumn(); ImGui.Text("Order");
            ImGui.TableNextColumn(); ImGui.Text("Label");
            ImGui.TableNextColumn(); ImGui.Text("Command");
            ImGui.TableNextColumn(); ImGui.Text("Size (W × H)");
            ImGui.TableNextColumn(); ImGui.Text("Colors");
            ImGui.EndTable();
        }
    }

    private void DrawButtonListContent(ButtonWindow window)
    {
        if (ImGui.BeginTable("##buttons", 6, ImGuiTableFlags.BordersInnerH))
        {
            ImGui.TableSetupColumn("##order", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthFixed, 120);
            ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed, 110);
            ImGui.TableSetupColumn("Colors", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("##actions", ImGuiTableColumnFlags.WidthFixed, 20);

            for (int i = 0; i < window.Config.Buttons.Count; i++)
            {
                DrawButtonRow(window, i);
            }

            ImGui.EndTable();
        }
    }

    private void DrawButtonRow(ButtonWindow window, int index)
    {
        var button = window.Config.Buttons[index];
        ImGui.PushID(index);

        ImGui.TableNextRow();

        // Move Column
        ImGui.TableNextColumn();
        DrawMoveButtons(window, index);

        // Label Column
        ImGui.TableNextColumn();
        DrawLabelInput(button);

        // Command Column
        ImGui.TableNextColumn();
        DrawCommandInput(button);

        // Size Column
        ImGui.TableNextColumn();
        DrawSizeInput(button);

        // Colors Column
        ImGui.TableNextColumn();
        DrawColorEditors(button);

        // Delete Column
        ImGui.TableNextColumn();
        DrawDeleteButton(window, index);

        ImGui.PopID();
    }

    private void DrawMoveButtons(ButtonWindow window, int index)
    {
        if (index > 0 && ImGui.Button("▲##up"))
        {
            SwapButtons(window.Config.Buttons, index, index - 1);
        }
        if (index < window.Config.Buttons.Count - 1)
        {
            ImGui.SameLine();
            if (ImGui.Button("▼##down"))
            {
                SwapButtons(window.Config.Buttons, index, index + 1);
            }
        }
    }

    private void DrawLabelInput(Configuration.ButtonData button)
    {
        string label = button.Label;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText("##label", ref label, 32))
        {
            button.Label = label;
            Configuration.Save();
        }
    }

    private void DrawCommandInput(Configuration.ButtonData button)
    {
        string command = button.Command;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText("##command", ref command, 64))
        {
            button.Command = command;
            Configuration.Save();
        }
    }

    private void DrawSizeInput(Configuration.ButtonData button)
    {
        var size = new Vector2(button.Width, button.Height);
        ImGui.SetNextItemWidth(-1);
        if (ImGui.DragFloat2("##size", ref size, 0.5f, 20, 500, "%.1f"))
        {
            button.Width = size.X;
            button.Height = size.Y;
            Configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Width × Height in pixels");
        }
    }

    private void DrawColorEditors(Configuration.ButtonData button)
    {
        Vector4 buttonColor = button.Color;
        if (ImGui.ColorEdit4("##btncolor", ref buttonColor,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
        {
            button.Color = buttonColor;
            Configuration.Save();
        }
        ImGui.SameLine();
        Vector4 textColor = button.LabelColor;
        if (ImGui.ColorEdit4("##txtcolor", ref textColor,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
        {
            button.LabelColor = textColor;
            Configuration.Save();
        }
    }

    private void DrawDeleteButton(ButtonWindow window, int index)
    {
        ImGuiHelper.PushButtonColors(ImGuiHelper.DangerButtonColor);
        if (ImGui.Button("X##delete"))
        {
            window.Config.Buttons.RemoveAt(index);
            Configuration.Save();
        }
        ImGuiHelper.PopButtonColors();
    }

    private void SwapButtons(List<Configuration.ButtonData> buttons, int indexA, int indexB)
    {
        (buttons[indexA], buttons[indexB]) = (buttons[indexB], buttons[indexA]);
        Configuration.Save();
    }
}
