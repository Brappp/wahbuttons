using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Configuration;

namespace WahButtons;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public List<ButtonWindowConfig> Windows { get; set; } = new();

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    [Serializable]
    public class ButtonWindowConfig
    {
        public string Name { get; set; } = "New Window";
        public bool IsVisible { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public bool TransparentBackground { get; set; } = false;
        public ButtonLayout Layout { get; set; } = ButtonLayout.Vertical;
        public List<ButtonData> Buttons { get; set; } = new();
        public Vector2 Position { get; set; } = new Vector2(100, 100);
        public Vector2 Size { get; set; } = new Vector2(300, 200);

        // Grid-specific settings
        public int GridRows { get; set; } = 6;
        public int GridColumns { get; set; } = 6;
    }

    [Serializable]
    public class ButtonData
    {
        public string Label { get; set; }
        public string Command { get; set; }
        public float Width { get; set; } = 75;
        public float Height { get; set; } = 30;
        public Vector4 Color { get; set; } = new Vector4(0.26f, 0.59f, 0.98f, 1f);
        public Vector4 LabelColor { get; set; } = new Vector4(1f, 1f, 1f, 1f);

        public ButtonData(string label, string command, float width)
        {
            Label = label;
            Command = command;
            Width = width;
        }

        public ButtonData() { }
    }

    public enum ButtonLayout
    {
        Vertical,
        Horizontal,
        Grid
    }
}
