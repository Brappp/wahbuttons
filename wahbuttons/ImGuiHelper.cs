using ImGuiNET;
using System;
using System.Numerics;

namespace WahButtons.Helpers;

public static class ImGuiHelper
{
    // FFXIV-inspired colors
    public static readonly Vector4 DefaultBgColor = new(0.08f, 0.08f, 0.08f, 0.95f);       // Dark background
    public static readonly Vector4 DefaultButtonColor = new(0.25f, 0.25f, 0.25f, 1.0f);    // Neutral button
    public static readonly Vector4 DangerButtonColor = new(0.6f, 0.1f, 0.1f, 1.0f);       // Red button
    public static readonly Vector4 TextColor = new(0.9f, 0.9f, 0.9f, 1.0f);               // Light text
    public static readonly Vector4 BorderColor = new(0.145f, 0.145f, 0.145f, 1.0f);       // Dark border

    public static void PushWahButtonsStyle()
    {
        var style = ImGui.GetStyle();

        // Push style variables
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12, 12));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 6));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(4, 4));
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 12.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, 12.0f);

        // Push window border styles
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);

        // Push rounding
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 4.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 4.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 4.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 4.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 4.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 4.0f);

        // Push colors
        ImGui.PushStyleColor(ImGuiCol.Text, TextColor);
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, new Vector4(0.5f, 0.5f, 0.5f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.WindowBg, DefaultBgColor);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.08f, 0.08f, 0.08f, 0.94f));
        ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(0.08f, 0.08f, 0.08f, 0.94f));
        ImGui.PushStyleColor(ImGuiCol.Border, BorderColor);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.15f, 0.15f, 0.15f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.20f, 0.20f, 0.20f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.Button, DefaultButtonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.30f, 0.30f, 0.30f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.35f, 0.35f, 0.35f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.20f, 0.20f, 0.20f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.25f, 0.25f, 0.25f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.30f, 0.30f, 0.30f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, new Vector4(0.15f, 0.15f, 0.15f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.Tab, new Vector4(0.15f, 0.15f, 0.15f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(0.25f, 0.25f, 0.25f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(0.20f, 0.20f, 0.20f, 1.00f));
    }

    public static void PopWahButtonsStyle()
    {
        ImGui.PopStyleColor(19); // Pop all colors we pushed
        ImGui.PopStyleVar(18);   // Pop all style variables we pushed
    }

    public static void PushButtonColors(Vector4 color)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, color);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(
            Math.Min(color.X * 1.2f, 1.0f),
            Math.Min(color.Y * 1.2f, 1.0f),
            Math.Min(color.Z * 1.2f, 1.0f),
            color.W
        ));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(
            color.X * 0.8f,
            color.Y * 0.8f,
            color.Z * 0.8f,
            color.W
        ));
    }

    public static void PopButtonColors()
    {
        ImGui.PopStyleColor(3);
    }
}
