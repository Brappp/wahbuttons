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
            ImGui.Text("Create Aetheryte Teleport Buttons");
            ImGui.Separator();
            
            DrawContent();
            
            // Popups
            DrawPopups();
        }
        
        public void DrawContent()
        {
            // Top controls
            DrawTopControls();
            
            ImGui.Separator();
            
            // Two-panel layout
            if (ImGui.BeginTable("AetheryteLayout", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Regions", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Aetherytes", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();
                
                // Left panel - Regions
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                DrawRegionsPanel();
                
                // Right panel - Aetherytes in selected region
                ImGui.TableSetColumnIndex(1);
                DrawAetherytesPanel();
                
                ImGui.EndTable();
            }
            
            ImGui.Separator();
            
            // Button customization options
            DrawButtonCustomizationPanel();
        }
        
        private void DrawPopups()
        {
            // Popup for no target window selected
            bool noTargetOpen = true;
            if (ImGui.BeginPopupModal("NoTargetWindowPopup", ref noTargetOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Please select a target window first!");
                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
            
            // Popup for button added successfully
            bool buttonAddedOpen = true;
            if (ImGui.BeginPopupModal("ButtonAddedPopup", ref buttonAddedOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Button added successfully!");
                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }
        
        private void DrawTopControls()
        {
            // Search filter
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Search:");
            ImGui.SameLine();
            ImGui.PushItemWidth(200);
            if (ImGui.InputText("##AetheryteSearch", ref searchFilter, 100))
            {
                // Clear region selection if searching
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    selectedRegion = string.Empty;
                }
            }
            ImGui.PopItemWidth();
            
            ImGui.SameLine(0, 20);
            
            // Auto-detect location button
            if (ImGui.Button("Auto-Detect Location"))
            {
                UpdateRegionBasedOnPlayerLocation();
            }
            
            ImGui.SameLine(0, 20);
            
            // Target window selection
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Target Window:");
            ImGui.SameLine();
            ImGui.PushItemWidth(200);
            
            string targetWindowName = targetButtonWindow?.Config.Name ?? "Select Window";
            if (ImGui.BeginCombo("##TargetWindowSelection", targetWindowName))
            {
                foreach (var window in GetButtonWindows())
                {
                    if (ImGui.Selectable(window.Config.Name, targetButtonWindow == window))
                    {
                        targetButtonWindow = window;
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();
        }
        
        private void DrawRegionsPanel()
        {
            if (ImGui.BeginChild("RegionsChild", new Vector2(0, 300), false))
            {
                ImGui.Text("Select Region:");
                ImGui.Separator();
                
                // Show "All" option when searching
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    bool isSelected = string.IsNullOrEmpty(selectedRegion);
                    if (ImGui.Selectable("All Regions", isSelected))
                    {
                        selectedRegion = string.Empty;
                    }
                    ImGui.Separator();
                }
                
                // Get current region
                string currentRegion = LocationHelper.GetCurrentRegion();
                
                // List all regions
                foreach (var region in AetheryteHelper.GetRegions())
                {
                    bool isSelected = region == selectedRegion;
                    
                    // Highlight current region with a different color
                    if (region == currentRegion)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.2f, 0.8f, 0.2f, 1.0f));
                        ImGui.Text("⚡ "); // Add a lightning symbol to indicate current region
                        ImGui.SameLine();
                    }
                    
                    if (ImGui.Selectable(region, isSelected))
                    {
                        selectedRegion = region;
                        selectedAetheryteId = 0; // Reset selection
                    }
                    
                    if (region == currentRegion)
                    {
                        ImGui.PopStyleColor();
                    }
                }
                ImGui.EndChild();
            }
        }
        
        private void DrawAetherytesPanel()
        {
            if (ImGui.BeginChild("AetherytesChild", new Vector2(0, 300), false))
            {
                ImGui.Text("Aetherytes:");
                ImGui.Separator();
                
                Dictionary<uint, string> aetherytes;
                
                // If searching, show filtered results from all regions
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    aetherytes = GetFilteredAetherytes();
                }
                // Otherwise show aetherytes from selected region
                else if (!string.IsNullOrEmpty(selectedRegion))
                {
                    aetherytes = AetheryteHelper.GetAetherytesByRegion(selectedRegion);
                }
                else
                {
                    aetherytes = new Dictionary<uint, string>();
                }
                
                if (aetherytes.Count == 0)
                {
                    ImGui.TextColored(new Vector4(1, 1, 0, 1), "No aetherytes found.");
                }
                else
                {
                    // Display aetherytes in a table
                    if (ImGui.BeginTable("AetherytesList", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 40);
                        ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableSetupColumn("##Action", ImGuiTableColumnFlags.WidthFixed, 80);
                        ImGui.TableHeadersRow();
                        
                        foreach (var aetheryte in aetherytes)
                        {
                            ImGui.TableNextRow();
                            
                            // ID Column
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text(aetheryte.Key.ToString());
                            
                            // Name Column
                            ImGui.TableSetColumnIndex(1);
                            bool isSelected = selectedAetheryteId == aetheryte.Key;
                            if (ImGui.Selectable($"{aetheryte.Value}##select_{aetheryte.Key}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                selectedAetheryteId = aetheryte.Key;
                                buttonLabel = aetheryte.Value; // Set default label
                            }
                            
                            // Button Column
                            ImGui.TableSetColumnIndex(2);
                            ImGui.PushID($"add_{aetheryte.Key}");
                            if (ImGui.SmallButton("Add"))
                            {
                                AddAetheryteButton(aetheryte.Key, aetheryte.Value);
                            }
                            ImGui.PopID();
                        }
                        
                        ImGui.EndTable();
                    }
                }
                
                ImGui.EndChild();
            }
        }
        
        private void DrawButtonCustomizationPanel()
        {
            if (selectedAetheryteId == 0)
            {
                ImGui.TextColored(new Vector4(1, 1, 0, 1), "Select an aetheryte to customize the button.");
                return;
            }
            
            if (targetButtonWindow == null)
            {
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Please select a target window first!");
                return;
            }
            
            ImGui.Text($"Customize Button for {AetheryteHelper.GetAetheryteName(selectedAetheryteId)}");
            ImGui.Separator();
            
            // Button label
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Button Label:");
            ImGui.SameLine();
            ImGui.PushItemWidth(200);
            ImGui.InputText("##ButtonLabel", ref buttonLabel, 100);
            ImGui.PopItemWidth();
            
            // Button dimensions
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Size:");
            ImGui.SameLine();
            ImGui.PushItemWidth(80);
            ImGui.InputFloat("Width##BtnWidth", ref buttonWidth, 5, 10);
            ImGui.SameLine();
            ImGui.InputFloat("Height##BtnHeight", ref buttonHeight, 5, 10);
            ImGui.PopItemWidth();
            
            // Button color
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Color:");
            ImGui.SameLine();
            ImGui.ColorEdit4("##ButtonColor", ref buttonColor);
            
            // Preview
            ImGui.Separator();
            ImGui.Text("Preview:");
            
            ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonColor * 1.2f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, buttonColor * 1.5f);
            
            if (ImGui.Button(buttonLabel, new Vector2(buttonWidth, buttonHeight)))
            {
                // Preview button click - doesn't do anything
            }
            
            ImGui.PopStyleColor(3);
            
            // Add button
            ImGui.Separator();
            if (ImGui.Button("Add to Selected Window", new Vector2(200, 30)))
            {
                AddCustomAetheryteButton();
            }
        }
        
        private Dictionary<uint, string> GetFilteredAetherytes()
        {
            var result = new Dictionary<uint, string>();
            string lowerFilter = searchFilter.ToLowerInvariant();
            
            var allAetherytes = AetheryteHelper.GetAllAetherytes();
            foreach (var aetheryte in allAetherytes)
            {
                if (aetheryte.Value.ToLowerInvariant().Contains(lowerFilter) ||
                    aetheryte.Key.ToString().Contains(lowerFilter))
                {
                    result.Add(aetheryte.Key, aetheryte.Value);
                }
            }
            
            return result;
        }
        
        private void AddAetheryteButton(uint aetheryteId, string aetheryteName)
        {
            if (targetButtonWindow == null)
            {
                ImGui.OpenPopup("NoTargetWindowPopup");
                return;
            }
            
            var teleportCommand = AetheryteHelper.GenerateTeleportCommand(aetheryteId);
            var newButton = new Configuration.ButtonData(aetheryteName, teleportCommand, 100);
            
            targetButtonWindow.Config.Buttons.Add(newButton);
            Plugin.Configuration.Save();
            
            ImGui.OpenPopup("ButtonAddedPopup");
        }
        
        private void AddCustomAetheryteButton()
        {
            if (targetButtonWindow == null || selectedAetheryteId == 0)
                return;
                
            var teleportCommand = AetheryteHelper.GenerateTeleportCommand(selectedAetheryteId);
            var newButton = new Configuration.ButtonData(buttonLabel, teleportCommand, buttonWidth)
            {
                Height = buttonHeight,
                Color = buttonColor
            };
            
            targetButtonWindow.Config.Buttons.Add(newButton);
            Plugin.Configuration.Save();
            
            ImGui.OpenPopup("ButtonAddedPopup");
        }
        
        private List<ButtonWindow> GetButtonWindows()
        {
            var windows = new List<ButtonWindow>();
            foreach (var window in Plugin.WindowSystem.Windows)
            {
                if (window is ButtonWindow buttonWindow)
                {
                    windows.Add(buttonWindow);
                }
            }
            return windows;
        }
        
        private void UpdateRegionBasedOnPlayerLocation()
        {
            try
            {
                // Get the current region based on player location
                selectedRegion = LocationHelper.GetCurrentRegion();
                
                // Get the nearest aetheryte in that region
                selectedAetheryteId = LocationHelper.GetNearestAetheryteId();
                
                // Update label if an aetheryte is selected
                if (selectedAetheryteId > 0)
                {
                    var aetherytes = AetheryteHelper.GetAetherytesByRegion(selectedRegion);
                    if (aetherytes.TryGetValue(selectedAetheryteId, out var name))
                    {
                        buttonLabel = name;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error updating region based on player location: {ex.Message}");
            }
        }

        public void SetRegion(string region)
        {
            // Set the selected region if it exists in available regions
            if (AetheryteHelper.GetRegions().Contains(region))
            {
                selectedRegion = region;
                
                // Try to get the nearest aetheryte ID for this region
                selectedAetheryteId = LocationHelper.GetNearestAetheryteId();
                
                // Update button label if an aetheryte is selected
                if (selectedAetheryteId > 0)
                {
                    var aetherytes = AetheryteHelper.GetAetherytesByRegion(selectedRegion);
                    if (aetherytes.TryGetValue(selectedAetheryteId, out var name))
                    {
                        buttonLabel = name;
                    }
                }
            }
        }
    }
} 