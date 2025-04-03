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
    }
} 