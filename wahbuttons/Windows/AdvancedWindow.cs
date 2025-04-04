using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using WahButtons.Helpers;
using Dalamud.Game.ClientState.Conditions;
using System.Threading.Tasks;

namespace WahButtons.Windows
{
    public class AdvancedWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private ConditionWindow conditionWindow;
        private AetheryteWindow aetheryteWindow;
        
        // Configuration management
        private string backupFolderPath = string.Empty;
        private List<string> backupFiles = new List<string>();
        private DateTime lastBackupCheck = DateTime.MinValue;
        private string backupStatusMessage = string.Empty;
        private string backupStatusColor = "white"; // "green", "yellow", "red"
        private string selectedBackupFile = string.Empty;
        private bool showResetConfirmation = false;
        
        public AdvancedWindow(Plugin plugin, ConditionWindow conditionWindow, AetheryteWindow aetheryteWindow)
            : base("Advanced Features##AdvancedWindow", ImGuiWindowFlags.NoScrollbar)
        {
            Plugin = plugin;
            this.conditionWindow = conditionWindow;
            this.aetheryteWindow = aetheryteWindow;
            
            Size = new Vector2(800, 600);
            SizeCondition = ImGuiCond.FirstUseEver;
            
            // Initialize backup folder
            try
            {
                string pluginConfigPath = Plugin.PluginInterface.GetPluginConfigDirectory();
                backupFolderPath = Path.Combine(pluginConfigPath, "Backups");
                
                // Create backup directory if it doesn't exist
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }
                
                // Load the list of backup files
                RefreshBackupFilesList();
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error initializing backup directory: {ex.Message}");
                backupStatusMessage = "Error initializing backup directory";
                backupStatusColor = "red";
            }
            
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
            // Add a decorative header
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0.8f, 1));
            ImGui.TextWrapped("Advanced Features for WahButtons");
            ImGui.PopStyleColor();
            ImGui.Separator();
            
            // Set tab style
            ImGui.PushStyleColor(ImGuiCol.Tab, new Vector4(0.2f, 0.2f, 0.3f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(0.3f, 0.4f, 0.7f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(0.4f, 0.5f, 0.8f, 1.0f));
            
            if (ImGui.BeginTabBar("AdvancedTabBar", ImGuiTabBarFlags.FittingPolicyScroll))
            {
                DrawConditionTab();
                DrawAetheryteTab();
                DrawConfigurationTab();
                ImGui.EndTabBar();
            }
            
            ImGui.PopStyleColor(3);
        }
        
        public void DrawConditionTab()
        {
            if (ImGui.BeginTabItem("Game Conditions"))
            {
                // Add some explanatory text at the top
                ImGui.TextWrapped("View and test game condition states for creating Smart Buttons");
                ImGui.Separator();
                
                // Draw the condition content
                conditionWindow.DrawContent();
                ImGui.EndTabItem();
            }
        }
        
        private void DrawAetheryteTab()
        {
            if (ImGui.BeginTabItem("Aetheryte Teleport"))
            {
                // Add an icon and better formatted header
                ImGui.TextColored(new Vector4(0.3f, 0.7f, 1.0f, 1.0f), "✧ Aetheryte Teleport Buttons");
                ImGui.TextWrapped("Create buttons to quickly teleport to any aetheryte in the game");
                
                // Current region with better formatting
                ImGui.Separator();
                ImGui.Text("Current region: ");
                ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.3f, 0.9f, 0.3f, 1.0f), LocationHelper.GetCurrentRegion());
                
                ImGui.Separator();
                
                // Center the button
                float windowWidth = ImGui.GetWindowWidth();
                float buttonWidth = 200;
                ImGui.SetCursorPosX((windowWidth - buttonWidth) / 2);
                
                // Make the button more visually appealing
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.4f, 0.8f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.2f, 0.5f, 0.9f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.3f, 0.6f, 1.0f, 1.0f));
                
                if (ImGui.Button("Open Aetheryte Window", new Vector2(buttonWidth, 40)))
                {
                    aetheryteWindow.IsOpen = true;
                }
                
                ImGui.PopStyleColor(3);
                
                ImGui.EndTabItem();
            }
        }
        
        private void DrawConfigurationTab()
        {
            if (ImGui.BeginTabItem("Configuration Management"))
            {
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.15f, 0.15f, 0.15f, 0.5f));
                ImGui.BeginChild("ConfigManagementChild", new Vector2(ImGui.GetWindowWidth() - 20, ImGui.GetWindowHeight() - 80), true);
                
                // Check for auto-backup needs (once a day)
                CheckDailyBackupNeeded();
                
                // Section 1: Create Backup
                if (ImGui.CollapsingHeader("Create Backup", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.TextWrapped("Create a backup of your current configuration. Backups are stored in the plugin's backup folder.");
                    ImGui.Separator();
                    
                    // Create backup button
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.6f, 0.3f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.7f, 0.4f, 1.0f));
                    if (ImGui.Button("Create Manual Backup", new Vector2(200, 30)))
                    {
                        CreateConfigBackup("manual");
                    }
                    ImGui.PopStyleColor(2);
                    
                    // Display status message with color
                    if (!string.IsNullOrEmpty(backupStatusMessage))
                    {
                        ImGui.SameLine();
                        switch (backupStatusColor)
                        {
                            case "green":
                                ImGui.TextColored(new Vector4(0.0f, 0.8f, 0.0f, 1.0f), backupStatusMessage);
                                break;
                            case "yellow":
                                ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.0f, 1.0f), backupStatusMessage);
                                break;
                            case "red":
                                ImGui.TextColored(new Vector4(0.8f, 0.0f, 0.0f, 1.0f), backupStatusMessage);
                                break;
                            default:
                                ImGui.Text(backupStatusMessage);
                                break;
                        }
                    }
                    
                    // Display backup folder path
                    ImGui.Text("Backup Folder: ");
                    ImGui.SameLine();
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 1.0f, 1.0f), backupFolderPath);
                    
                    // Open backup folder button
                    if (ImGui.Button("Open Backup Folder", new Vector2(150, 25)))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start("explorer.exe", backupFolderPath);
                        }
                        catch (Exception ex)
                        {
                            backupStatusMessage = "Error opening backup folder";
                            backupStatusColor = "red";
                            Plugin.PluginLog.Error($"Error opening backup folder: {ex.Message}");
                        }
                    }
                    
                    ImGui.Separator();
                    
                    // Auto-backup settings
                    ImGui.TextColored(new Vector4(0.9f, 0.7f, 0.3f, 1.0f), "Auto-Backup Settings");
                    
                    bool autoBackupEnabled = Plugin.Configuration.AutoBackupEnabled;
                    if (ImGui.Checkbox("Enable Daily Auto-Backup", ref autoBackupEnabled))
                    {
                        Plugin.Configuration.AutoBackupEnabled = autoBackupEnabled;
                        Plugin.Configuration.Save();
                    }
                    
                    ImGui.TextWrapped("When enabled, a backup will be automatically created once per day when you load the plugin.");
                    
                    int backupRetention = Plugin.Configuration.BackupRetentionDays;
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Backup Retention (Days)", ref backupRetention))
                    {
                        Plugin.Configuration.BackupRetentionDays = Math.Max(1, Math.Min(backupRetention, 90));
                        Plugin.Configuration.Save();
                    }
                    ImGui.TextWrapped("Backups older than this many days will be automatically deleted.");
                }
                
                // Section 2: Restore Backup
                if (ImGui.CollapsingHeader("Restore Backup", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.TextWrapped("Restore a previously created backup. This will replace your current configuration.");
                    ImGui.Separator();
                    
                    // Refresh backup list button
                    if (ImGui.Button("Refresh Backup List", new Vector2(150, 25)))
                    {
                        RefreshBackupFilesList();
                    }
                    
                    ImGui.SameLine();
                    ImGui.Text($"Available Backups: {backupFiles.Count}");
                    
                    // Backup list
                    if (ImGui.BeginChild("BackupList", new Vector2(ImGui.GetWindowWidth() - 40, 200), true))
                    {
                        if (backupFiles.Count == 0)
                        {
                            ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.0f, 1.0f), "No backups found.");
                        }
                        else
                        {
                            foreach (var file in backupFiles)
                            {
                                string displayName = Path.GetFileName(file);
                                bool isSelected = selectedBackupFile == file;
                                
                                if (ImGui.Selectable(displayName, isSelected))
                                {
                                    selectedBackupFile = file;
                                }
                                
                                // Show tooltip with file info on hover
                                if (ImGui.IsItemHovered())
                                {
                                    try
                                    {
                                        var fileInfo = new FileInfo(file);
                                        ImGui.BeginTooltip();
                                        ImGui.Text($"Created: {fileInfo.CreationTime}");
                                        ImGui.Text($"Size: {fileInfo.Length / 1024} KB");
                                        ImGui.EndTooltip();
                                    }
                                    catch { /* Silently ignore file info errors */ }
                                }
                            }
                        }
                        ImGui.EndChild();
                    }
                    
                    // Restore button
                    ImGui.BeginDisabled(string.IsNullOrEmpty(selectedBackupFile));
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.4f, 0.4f, 0.8f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.9f, 1.0f));
                    if (ImGui.Button("Restore Selected Backup", new Vector2(200, 30)))
                    {
                        ImGui.OpenPopup("RestoreConfirmationPopup");
                    }
                    ImGui.PopStyleColor(2);
                    ImGui.EndDisabled();
                    
                    // Delete backup button
                    ImGui.SameLine();
                    ImGui.BeginDisabled(string.IsNullOrEmpty(selectedBackupFile));
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.3f, 0.3f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.4f, 0.4f, 1.0f));
                    if (ImGui.Button("Delete", new Vector2(80, 30)))
                    {
                        ImGui.OpenPopup("DeleteBackupConfirmationPopup");
                    }
                    ImGui.PopStyleColor(2);
                    ImGui.EndDisabled();
                    
                    // Restore confirmation popup
                    if (ImGui.BeginPopup("RestoreConfirmationPopup"))
                    {
                        ImGui.TextColored(new Vector4(1, 0.3f, 0.3f, 1), "Warning!");
                        ImGui.TextWrapped("This will replace your current configuration with the selected backup. Any unsaved changes will be lost.");
                        ImGui.Separator();
                        
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.3f, 0.3f, 1.0f));
                        if (ImGui.Button("Restore", new Vector2(120, 0)))
                        {
                            RestoreConfigBackup(selectedBackupFile);
                            ImGui.CloseCurrentPopup();
                        }
                        ImGui.PopStyleColor();
                        
                        ImGui.SameLine();
                        
                        if (ImGui.Button("Cancel", new Vector2(120, 0)))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                        
                        ImGui.EndPopup();
                    }
                    
                    // Delete backup confirmation popup
                    if (ImGui.BeginPopup("DeleteBackupConfirmationPopup"))
                    {
                        ImGui.TextColored(new Vector4(1, 0.3f, 0.3f, 1), "Warning!");
                        ImGui.TextWrapped("Are you sure you want to delete this backup file?");
                        ImGui.Text(Path.GetFileName(selectedBackupFile));
                        ImGui.Separator();
                        
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.3f, 0.3f, 1.0f));
                        if (ImGui.Button("Delete", new Vector2(120, 0)))
                        {
                            DeleteBackupFile(selectedBackupFile);
                            ImGui.CloseCurrentPopup();
                        }
                        ImGui.PopStyleColor();
                        
                        ImGui.SameLine();
                        
                        if (ImGui.Button("Cancel", new Vector2(120, 0)))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                        
                        ImGui.EndPopup();
                    }
                }
                
                // Section 3: Reset Configuration
                if (ImGui.CollapsingHeader("Reset Configuration"))
                {
                    ImGui.TextWrapped("Reset your configuration to default settings. This will delete all your button windows and settings.");
                    ImGui.TextColored(new Vector4(1, 0.3f, 0.3f, 1), "Warning: This cannot be undone unless you have a backup!");
                    ImGui.Separator();
                    
                    // Reset button
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.1f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.2f, 0.2f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 0.3f, 0.3f, 1.0f));
                    if (ImGui.Button("Reset to Default Settings", new Vector2(200, 30)))
                    {
                        showResetConfirmation = true;
                        ImGui.OpenPopup("ResetConfirmationPopup");
                    }
                    ImGui.PopStyleColor(3);
                }
                
                // Reset confirmation popup (must be outside the collapsing header)
                if (showResetConfirmation && ImGui.BeginPopup("ResetConfirmationPopup"))
                {
                    ImGui.TextColored(new Vector4(1, 0.3f, 0.3f, 1), "WARNING!");
                    ImGui.TextWrapped("This will permanently delete ALL your button windows and settings!");
                    ImGui.TextWrapped("Are you absolutely sure you want to reset to default settings?");
                    ImGui.Separator();
                    
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.1f, 1.0f));
                    if (ImGui.Button("Yes, Reset Everything", new Vector2(180, 0)))
                    {
                        // Create a backup before reset
                        CreateConfigBackup("pre-reset");
                        
                        // Reset configuration
                        ResetConfiguration();
                        
                        showResetConfirmation = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.PopStyleColor();
                    
                    ImGui.SameLine();
                    
                    if (ImGui.Button("Cancel", new Vector2(100, 0)))
                    {
                        showResetConfirmation = false;
                        ImGui.CloseCurrentPopup();
                    }
                    
                    ImGui.EndPopup();
                }
                
                ImGui.EndChild();
                ImGui.PopStyleColor();
                
                ImGui.EndTabItem();
            }
        }
        
        private void RefreshBackupFilesList()
        {
            try
            {
                backupFiles.Clear();
                
                if (Directory.Exists(backupFolderPath))
                {
                    var files = Directory.GetFiles(backupFolderPath, "*.json");
                    backupFiles.AddRange(files);
                    backupFiles.Sort((a, b) => File.GetCreationTime(b).CompareTo(File.GetCreationTime(a))); // Sort newest first
                }
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error refreshing backup files: {ex.Message}");
                backupStatusMessage = "Error refreshing backup list";
                backupStatusColor = "red";
            }
        }
        
        private void CheckDailyBackupNeeded()
        {
            // Only check once per session
            if (lastBackupCheck != DateTime.MinValue)
                return;
                
            lastBackupCheck = DateTime.Now;
            
            try
            {
                if (!Plugin.Configuration.AutoBackupEnabled)
                    return;
                
                // Check if we already created a backup today
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                bool foundTodayBackup = false;
                
                foreach (var file in backupFiles)
                {
                    if (Path.GetFileName(file).Contains($"auto_{today}"))
                    {
                        foundTodayBackup = true;
                        break;
                    }
                }
                
                // If no backup for today, create one
                if (!foundTodayBackup)
                {
                    Task.Run(() => 
                    {
                        // Add a small delay to ensure plugin is fully loaded
                        System.Threading.Thread.Sleep(2000);
                        CreateConfigBackup("auto");
                        CleanupOldBackups();
                    });
                }
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error in daily backup check: {ex.Message}");
            }
        }
        
        private void CleanupOldBackups()
        {
            try
            {
                if (Plugin.Configuration.BackupRetentionDays <= 0)
                    return;
                
                var cutoffDate = DateTime.Now.AddDays(-Plugin.Configuration.BackupRetentionDays);
                var filesToDelete = new List<string>();
                
                foreach (var file in backupFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.CreationTime < cutoffDate)
                        {
                            filesToDelete.Add(file);
                        }
                    }
                    catch { /* Skip this file if there's an error */ }
                }
                
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { /* Skip if delete fails */ }
                }
                
                // Refresh the list after cleanup
                RefreshBackupFilesList();
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error cleaning up old backups: {ex.Message}");
            }
        }
        
        private void CreateConfigBackup(string prefix)
        {
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }
                
                // Get the source file path - changed to use GetPluginConfigPath
                string configSourcePath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "wahbuttons.json");
                
                // Create a timestamped filename
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string backupFileName = $"{prefix}_{timestamp}_wahbuttons.json";
                string backupFilePath = Path.Combine(backupFolderPath, backupFileName);
                
                // Copy the file
                File.Copy(configSourcePath, backupFilePath, true);
                
                // Update UI
                backupStatusMessage = "Backup created successfully";
                backupStatusColor = "green";
                
                // Refresh backup list
                RefreshBackupFilesList();
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error creating backup: {ex.Message}");
                backupStatusMessage = "Error creating backup";
                backupStatusColor = "red";
            }
        }
        
        private void RestoreConfigBackup(string backupFilePath)
        {
            try
            {
                // Create a backup of current config before restoring
                CreateConfigBackup("pre-restore");
                
                // Get the destination file path - changed to use GetPluginConfigPath
                string configDestPath = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "wahbuttons.json");
                
                // Copy the backup file over the current config
                File.Copy(backupFilePath, configDestPath, true);
                
                // Update UI
                backupStatusMessage = "Backup restored successfully. Restart the plugin to apply changes.";
                backupStatusColor = "green";
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error restoring backup: {ex.Message}");
                backupStatusMessage = "Error restoring backup";
                backupStatusColor = "red";
            }
        }
        
        private void DeleteBackupFile(string backupFilePath)
        {
            try
            {
                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
                    selectedBackupFile = "";
                    RefreshBackupFilesList();
                    
                    backupStatusMessage = "Backup deleted successfully";
                    backupStatusColor = "green";
                }
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error deleting backup: {ex.Message}");
                backupStatusMessage = "Error deleting backup";
                backupStatusColor = "red";
            }
        }
        
        private void ResetConfiguration()
        {
            try
            {
                // Clear all windows
                foreach (var window in Plugin.Configuration.Windows.ToArray())
                {
                    Plugin.Configuration.Windows.Remove(window);
                }
                
                // Reset other settings
                Plugin.Configuration.Version = 1;
                Plugin.Configuration.ShowConditionWindow = false;
                
                // Save the empty configuration
                Plugin.Configuration.Save();
                
                // Create a default window
                Plugin.AddDefaultWindow();
                
                // Update UI
                backupStatusMessage = "Configuration reset successfully. Restart the plugin to apply changes.";
                backupStatusColor = "green";
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error resetting configuration: {ex.Message}");
                backupStatusMessage = "Error resetting configuration";
                backupStatusColor = "red";
            }
        }
    }
} 