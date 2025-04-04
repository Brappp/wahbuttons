using ImGuiNET;
using System.Numerics;
using System;

namespace WahButtons.Helpers
{
    public static class WindowHelper
    {
        public static ImGuiWindowFlags GetWindowFlags(Configuration.ButtonWindowConfig config)
        {
            ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

            if (config.IsLocked)
            {
                flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;
            }

            if (config.TransparentBackground)
            {
                flags |= ImGuiWindowFlags.NoBackground;
            }

            return flags;
        }

        public static bool IsPositionDifferent(Vector2 a, Vector2 b, float tolerance = 0.1f)
        {
            return Math.Abs(a.X - b.X) > tolerance || Math.Abs(a.Y - b.Y) > tolerance;
        }

        public static bool IsSizeDifferent(Vector2 a, Vector2 b, float tolerance = 0.1f)
        {
            return Math.Abs(a.X - b.X) > tolerance || Math.Abs(a.Y - b.Y) > tolerance;
        }

        // Added method for section headers
        public static void DrawSectionHeader(string title)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.6f, 0.1f, 1.0f));
            ImGui.Separator();
            ImGui.Text(title);
            ImGui.Separator();
            ImGui.PopStyleColor();
        }

        // Added helper for tooltips
        public static void DrawHelpMarker(string desc)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        // Added styling helpers
        public static void ApplyHeaderStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.6f, 0.1f, 1.0f));
        }

        public static void ResetHeaderStyle()
        {
            ImGui.PopStyleColor();
        }
    }
}