using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using WahButtons.Helpers;
using System.Linq;

namespace WahButtons.Windows
{
    public class AetheryteWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private string searchFilter = string.Empty;
        private string selectedRegion = string.Empty;
        private uint selectedAetheryteId = 0;
        private ButtonWindow targetButtonWindow = null;
        
        // For button customization
        private string buttonLabel = "";
        private Vector4 buttonColor = new Vector4(0.26f, 0.59f, 0.98f, 1f);
        private float buttonWidth = 75;
        private float buttonHeight = 30;

        public AetheryteWindow(Plugin plugin)
            : base("Aetheryte Teleport Buttons##AetheryteWindow",
                  ImGuiWindowFlags.AlwaysAutoResize)
        {
            Plugin = plugin;
            Size = new Vector2(600, 600);
            SizeCondition = ImGuiCond.FirstUseEver;
            
            // By default, use player's current region
            UpdateRegionBasedOnPlayerLocation();
            
            // Fall back to first region if needed
            if (string.IsNullOrEmpty(selectedRegion))
            {
                var regions = AetheryteHelper.GetRegions();
                if (regions.Count > 0)
                {
                    selectedRegion = regions[0];
                }
            }
        }

        public void Dispose() { }

        public override void Draw()
        {
            // Always try to update the current region first
            UpdateRegionBasedOnPlayerLocation();
            
            // Simple header with auto-region detection
            DrawSimplifiedHeader();
            
            ImGui.Separator();
            
            // Two-column layout
            if (ImGui.BeginTable("AetheryteLayout", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Regions", ImGuiTableColumnFlags.WidthFixed, 180);
                ImGui.TableSetupColumn("Aetherytes", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();
                
                // Left column - Regions
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                DrawRegionSelector();
                
                // Right column - Aetherytes
                ImGui.TableSetColumnIndex(1);
                DrawAetheryteList();
                
                ImGui.EndTable();
            }
        }
        
        private void DrawSimplifiedHeader()
        {
            string regionName = selectedRegion ?? "Unknown";
            ImGui.Text($"Current Region: {regionName}");
            
            ImGui.SameLine(ImGui.GetWindowWidth() - 120);
            if (ImGui.Button("Auto-Detect", new Vector2(100, 0)))
            {
                UpdateRegionBasedOnPlayerLocation();
            }
            
            // Search input
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 20);
            if (ImGui.InputTextWithHint("##Search", "Search aetherytes...", ref searchFilter, 100))
            {
                // SearchTerm is updated by ImGui
            }
        }
        
        private void DrawRegionSelector()
        {
            ImGui.BeginChild("RegionsChild", new Vector2(0, 400), false);
            
            // Favorites at the top
            if (ImGui.CollapsingHeader("Favorites", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent(10);
                DrawFavoritesList();
                ImGui.Unindent(10);
            }
            
            ImGui.Separator();
            
            // Regions list
            if (ImGui.BeginChild("RegionList", new Vector2(0, 300), true))
            {
                foreach (var region in AetheryteHelper.GetRegions())
                {
                    bool isSelected = region == selectedRegion;
                    if (ImGui.Selectable(region, isSelected))
                    {
                        selectedRegion = region;
                    }
                }
                ImGui.EndChild();
            }
            
            ImGui.EndChild();
        }
        
        private void DrawFavoritesList()
        {
            if (AetheryteHelper.GetFavorites().Count == 0)
            {
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "No favorites yet.");
                ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "Click ★ to add.");
                return;
            }
            
            foreach (var favorite in AetheryteHelper.GetFavorites())
            {
                var aetheryte = WahButtons.Helpers.AetheryteHelper.AetheryteData.GetAetheryteById(favorite);
                if (aetheryte != null)
                {
                    if (ImGui.Selectable($"{aetheryte.Name} ★"))
                    {
                        // Select the aetheryte's region and scroll to that aetheryte
                        selectedRegion = aetheryte.Region;
                        selectedAetheryteId = aetheryte.Id;
                    }
                    
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"Region: {aetheryte.Region}");
                        ImGui.Text($"ID: {aetheryte.Id}");
                        ImGui.EndTooltip();
                    }
                    
                    ImGui.SameLine(ImGui.GetWindowWidth() - 30);
                    if (ImGui.SmallButton($"×##{aetheryte.Id}"))
                    {
                        AetheryteHelper.RemoveFavorite(aetheryte.Id);
                    }
                }
            }
        }
        
        private void DrawAetheryteList()
        {
            ImGui.BeginChild("AetherytesChild", new Vector2(0, 400), false);
            
            // Filter and get aetherytes for the current region - use full namespace to avoid ambiguity
            var aetherytes = WahButtons.Helpers.AetheryteHelper.AetheryteData.GetAetherytesByRegion(selectedRegion ?? string.Empty);
            
            // Apply search filter if needed
            if (!string.IsNullOrWhiteSpace(searchFilter))
            {
                aetherytes = aetherytes.Where(a => 
                    a.Name.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            
            if (aetherytes.Count == 0)
            {
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "No aetherytes found.");
                if (!string.IsNullOrWhiteSpace(searchFilter))
                {
                    ImGui.Text("Try different search terms.");
                }
                ImGui.EndChild();
                return;
            }
            
            // Group by zone for easier navigation
            var byZone = aetherytes.GroupBy(a => a.Zone).OrderBy(g => g.Key);
            
            foreach (var group in byZone)
            {
                if (ImGui.CollapsingHeader(group.Key, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Indent(10);
                    
                    // Show aetherytes for this zone
                    foreach (var aetheryte in group.OrderBy(a => a.Name))
                    {
                        ImGui.PushID($"aetheryte_{aetheryte.Id}");
                        
                        bool isFavorite = AetheryteHelper.IsFavorite(aetheryte.Id);
                        if (isFavorite)
                        {
                            ImGui.TextColored(new Vector4(1, 0.8f, 0, 1), "★");
                        }
                        else
                        {
                            if (ImGui.SmallButton($"★##fav{aetheryte.Id}"))
                            {
                                AetheryteHelper.AddFavorite(aetheryte.Id);
                            }
                        }
                        
                        ImGui.SameLine();
                        ImGui.Text(aetheryte.Name);
                        
                        ImGui.SameLine(ImGui.GetWindowWidth() - 150);
                        
                        // Create Button
                        if (ImGui.Button($"Create Button##btn{aetheryte.Id}", new Vector2(120, 0)))
                        {
                            OpenTargetWindowSelector(aetheryte);
                        }
                        
                        ImGui.PopID();
                    }
                    
                    ImGui.Unindent(10);
                    ImGui.Separator();
                }
            }
            
            ImGui.EndChild();
        }
        
        private void OpenTargetWindowSelector(WahButtons.Helpers.AetheryteHelper.AetheryteData.Aetheryte aetheryte)
        {
            selectedAetheryteId = aetheryte.Id;
            ImGui.OpenPopup("SelectTargetWindow");
        }

        public override void PreDraw()
        {
            base.PreDraw();
            
            // Target window selection popup
            bool popupOpen = true;
            if (ImGui.BeginPopupModal("SelectTargetWindow", ref popupOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                var aetheryte = WahButtons.Helpers.AetheryteHelper.AetheryteData.GetAetheryteById(selectedAetheryteId);
                if (aetheryte != null)
                {
                    ImGui.Text($"Add teleport button for {aetheryte.Name}");
                    ImGui.Separator();
                    
                    ImGui.Text("Select target window:");
                    
                    if (ImGui.BeginListBox("##WindowsList", new Vector2(300, 200)))
                    {
                        foreach (var windowConfig in Plugin.Configuration.Windows)
                        {
                            if (ImGui.Selectable(windowConfig.Name))
                            {
                                AddTeleportButtonToWindow(windowConfig, aetheryte);
                                ImGui.CloseCurrentPopup();
                            }
                        }
                        ImGui.EndListBox();
                    }
                    
                    if (ImGui.Button("Cancel", new Vector2(100, 0)))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }
                
                ImGui.EndPopup();
            }
        }
        
        private void AddTeleportButtonToWindow(Configuration.ButtonWindowConfig windowConfig, WahButtons.Helpers.AetheryteHelper.AetheryteData.Aetheryte aetheryte)
        {
            var teleportCommand = $"/teleport {aetheryte.Id}";
            
            // Create a new button with sensible defaults
            var newButton = new Configuration.ButtonData
            {
                Label = aetheryte.Name,
                Command = teleportCommand,
                Width = 100,
                Height = 30,
                Color = new Vector4(0.1f, 0.4f, 0.8f, 1f),
                LabelColor = new Vector4(1f, 1f, 1f, 1f)
            };
            
            // Add the button to the specified window
            windowConfig.Buttons.Add(newButton);
            
            // Save the configuration
            Plugin.Configuration.Save();
            
            // Show confirmation
            Plugin.ChatGui.Print($"Added teleport button for {aetheryte.Name} to {windowConfig.Name}");
        }

        // Helper method to update region based on player location
        private void UpdateRegionBasedOnPlayerLocation()
        {
            try
            {
                if (Plugin.ClientState != null)
                {
                    uint territoryId = Plugin.ClientState.TerritoryType;
                    string region = LocationHelper.GetCurrentRegion();
                    
                    if (!string.IsNullOrEmpty(region))
                    {
                        selectedRegion = region;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors quietly
                Plugin.ChatGui.PrintError($"Failed to detect region: {ex.Message}");
            }
        }

        public void SetRegion(string region)
        {
            // Set the selected region if it exists in available regions
            if (WahButtons.Helpers.AetheryteHelper.AetheryteData.GetRegions().Contains(region))
            {
                selectedRegion = region;
                
                // Try to get the nearest aetheryte ID for this region
                selectedAetheryteId = LocationHelper.GetNearestAetheryteId();
                
                // Update button label if an aetheryte is selected
                if (selectedAetheryteId > 0)
                {
                    var aetheryte = WahButtons.Helpers.AetheryteHelper.AetheryteData.GetAetheryteById(selectedAetheryteId);
                    if (aetheryte != null)
                    {
                        buttonLabel = aetheryte.Name;
                    }
                }
            }
        }
    }
} 