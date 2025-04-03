using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using WahButtons.Helpers;
using Dalamud.Game.ClientState.Conditions;

namespace WahButtons.Windows
{
    public class AdvancedWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private ConditionWindow conditionWindow;
        private AetheryteWindow aetheryteWindow;

        public AdvancedWindow(Plugin plugin, ConditionWindow conditionWindow, AetheryteWindow aetheryteWindow)
            : base("Advanced Features##AdvancedWindow", ImGuiWindowFlags.NoScrollbar)
        {
            Plugin = plugin;
            this.conditionWindow = conditionWindow;
            this.aetheryteWindow = aetheryteWindow;
            
            Size = new Vector2(800, 600);
            SizeCondition = ImGuiCond.FirstUseEver;
            
            // Set aetheryte region based on player location when window is created
            try
            {
                string currentRegion = LocationHelper.GetCurrentRegion();
                if (!string.IsNullOrEmpty(currentRegion))
                {
                    this.aetheryteWindow.SetRegion(currentRegion);
                }
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error setting initial region in AdvancedWindow: {ex.Message}");
            }
        }

        public void Dispose() { }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("AdvancedTabBar"))
            {
                DrawConditionTab();
                DrawAetheryteTab();
                ImGui.EndTabBar();
            }
        }
        
        public void DrawConditionTab()
        {
            if (ImGui.BeginTabItem("Conditions"))
            {
                // Draw the condition content
                conditionWindow.DrawContent();
                ImGui.EndTabItem();
            }
        }
        
        private void DrawAetheryteTab()
        {
            if (ImGui.BeginTabItem("Aetherytes"))
            {
                // Add auto-detect location button at the top of the aetheryte tab
                if (ImGui.Button("Auto-Detect Location"))
                {
                    string currentRegion = LocationHelper.GetCurrentRegion();
                    aetheryteWindow.SetRegion(currentRegion);
                }
                ImGui.SameLine();
                ImGui.Text("Current region: " + LocationHelper.GetCurrentRegion());
                
                ImGui.Separator();
                
                // Draw the aetheryte content
                aetheryteWindow.DrawContent();
                ImGui.EndTabItem();
            }
        }
    }
} 