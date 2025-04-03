using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using WahButtons.Helpers;

namespace WahButtons.Windows
{
    public class ConditionWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private bool showActiveOnly = false;
        private string searchFilter = string.Empty;

        public ConditionWindow(Plugin plugin)
            : base("Condition Tracker##ConditionTracker",
                  ImGuiWindowFlags.AlwaysAutoResize)
        {
            Plugin = plugin;
            Size = new Vector2(400, 500);
            SizeCondition = ImGuiCond.FirstUseEver;
        }

        public void Dispose() { }

        public override void Draw()
        {
            DrawContent();
        }

        // This method contains the actual drawing logic, separated for reuse
        public void DrawContent()
        {
            // Buttons at the top
            if (ImGui.Button("All Conditions"))
            {
                showActiveOnly = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Active Only"))
            {
                showActiveOnly = true;
            }
            ImGui.SameLine();
            
            // Search filter
            ImGui.PushItemWidth(200);
            ImGui.InputText("Search", ref searchFilter, 100);
            ImGui.PopItemWidth();

            ImGui.Separator();

            // Draw table
            if (ImGui.BeginTable("ConditionsTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("State", ImGuiTableColumnFlags.WidthFixed, 60);
                ImGui.TableHeadersRow();

                var flags = showActiveOnly 
                    ? ConditionHelper.GetActiveConditionFlags() 
                    : ConditionHelper.GetAllConditionFlags();

                foreach (var flag in flags)
                {
                    var description = ConditionHelper.GetConditionDescription(flag);
                    
                    // Apply search filter if needed
                    if (!string.IsNullOrEmpty(searchFilter) && 
                        !description.Contains(searchFilter, StringComparison.OrdinalIgnoreCase) && 
                        !flag.ToString().Contains(searchFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(description);

                    ImGui.TableSetColumnIndex(1);
                    var isActive = ConditionHelper.IsConditionActive(flag);
                    ImGui.TextColored(
                        isActive ? new Vector4(0.2f, 0.8f, 0.2f, 1.0f) : new Vector4(0.8f, 0.2f, 0.2f, 1.0f),
                        isActive ? "ON" : "OFF");
                }

                ImGui.EndTable();
            }
        }
    }
} 