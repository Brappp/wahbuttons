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
            // Add a header
            ImGui.TextColored(new Vector4(0.3f, 0.7f, 0.9f, 1.0f), "🔍 Game Condition Tracker");
            ImGui.TextWrapped("Use this to check active game conditions for use with Smart Buttons");
            ImGui.Separator();
            
            // Filter controls with better styling
            ImGui.BeginGroup();
            
            // Filter buttons with more visual appeal
            ImGui.PushStyleColor(ImGuiCol.Button, showActiveOnly ? 
                new Vector4(0.2f, 0.6f, 0.3f, 1.0f) : new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, showActiveOnly ? 
                new Vector4(0.3f, 0.7f, 0.4f, 1.0f) : new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            
            if (ImGui.Button("All Conditions", new Vector2(120, 24)))
            {
                showActiveOnly = false;
            }
            ImGui.PopStyleColor(2);
            
            ImGui.SameLine();
            
            ImGui.PushStyleColor(ImGuiCol.Button, !showActiveOnly ? 
                new Vector4(0.2f, 0.6f, 0.3f, 1.0f) : new Vector4(0.4f, 0.4f, 0.4f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, !showActiveOnly ? 
                new Vector4(0.3f, 0.7f, 0.4f, 1.0f) : new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            
            if (ImGui.Button("Active Only", new Vector2(120, 24)))
            {
                showActiveOnly = true;
            }
            ImGui.PopStyleColor(2);
            
            ImGui.SameLine();
            
            // Search with icon and better formatting
            ImGui.AlignTextToFramePadding();
            ImGui.Text("🔍");
            ImGui.SameLine();
            ImGui.PushItemWidth(200);
            ImGui.InputTextWithHint("##Search", "Search conditions...", ref searchFilter, 100);
            ImGui.PopItemWidth();
            
            ImGui.EndGroup();
            
            ImGui.Separator();

            // Draw table with better styling
            if (ImGui.BeginTable("ConditionsTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
            {
                // Table setup
                ImGui.TableSetupColumn("Condition", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("State", ImGuiTableColumnFlags.WidthFixed, 80);
                
                // Style the header
                ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, new Vector4(0.2f, 0.2f, 0.5f, 1.0f));
                ImGui.TableHeadersRow();
                ImGui.PopStyleColor();

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
                    
                    // More prominent and colorful status indicators
                    if (isActive)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 0.8f, 0.0f, 1.0f));
                        ImGui.Text("✓ ACTIVE");
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
                        ImGui.Text("INACTIVE");
                        ImGui.PopStyleColor();
                    }
                }

                ImGui.EndTable();
            }
        }
    }
} 