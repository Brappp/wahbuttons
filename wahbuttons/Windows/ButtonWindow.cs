using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;
using WahButtons.Helpers;

namespace WahButtons.Windows;

public class ButtonWindow : Window, IDisposable
{
    private Plugin Plugin;
    public Configuration.ButtonWindowConfig Config { get; }
    private int editingButtonIndex = -1;
    private string editLabel = string.Empty;
    private string editCommand = string.Empty;
    private bool isEditPopupOpen = false;

    public ButtonWindow(Plugin plugin, Configuration.ButtonWindowConfig config)
        : base(config.Name + "##" + Guid.NewGuid(),
            WindowHelper.GetWindowFlags(config))
    {
        Plugin = plugin;
        Config = config;
        IsOpen = config.IsVisible;
        RespectCloseHotkey = false;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        Flags = WindowHelper.GetWindowFlags(Config);
        IsOpen = Config.IsVisible;

        ImGui.SetNextWindowPos(Config.Position, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(Config.Size, ImGuiCond.FirstUseEver);
    }

    public override void Draw()
    {
        UpdatePositionAndSizeIfNeeded();
        DrawLayout();

        // Process any popups
        DrawButtonEditPopup();
    }

    private void UpdatePositionAndSizeIfNeeded()
    {
        if (Config.IsLocked)
            return;

        var currentPosition = ImGui.GetWindowPos();
        var currentSize = ImGui.GetWindowSize();

        if (WindowHelper.IsPositionDifferent(currentPosition, Config.Position))
        {
            Config.Position = currentPosition;
            Plugin.Configuration.Save();
        }

        if (WindowHelper.IsSizeDifferent(currentSize, Config.Size))
        {
            Config.Size = currentSize;
            Plugin.Configuration.Save();
        }
    }

    private void DrawLayout()
    {
        switch (Config.Layout)
        {
            case Configuration.ButtonLayout.Grid:
                LayoutHelper.HandleGridLayout(Config.Buttons, Config.Size, Config.GridRows, Config.GridColumns, RenderButton);
                break;
            case Configuration.ButtonLayout.Vertical:
                LayoutHelper.HandleVerticalLayout(Config.Buttons, Config.Size, RenderButton);
                break;
            case Configuration.ButtonLayout.Horizontal:
                LayoutHelper.HandleHorizontalLayout(Config.Buttons, Config.Size, RenderButton);
                break;
        }
    }

    private void RenderButton(Configuration.ButtonData button, float width, float height)
    {
        // Get the index of the button
        int buttonIndex = Config.Buttons.IndexOf(button);
        
        // Check if the button should be rendered based on conditions
        if (!ButtonHelper.ShouldRenderButton(button))
            return;

        // Create a unique ID for the button
        ImGui.PushID($"button_{buttonIndex}");

        // Apply button styles
        ButtonHelper.ApplyButtonStyles(button);

        // Check if the button should be enabled
        bool isEnabled = ButtonHelper.IsButtonEnabled(button);
        if (!isEnabled)
        {
            ImGui.BeginDisabled();
        }

        // Get the current label (may be changed by rules for smart buttons)
        string displayLabel = button.IsSmartButton ? ButtonHelper.GetButtonLabel(button) : button.Label;

        // Render the button
        if (ImGui.Button(displayLabel, new Vector2(width, height)))
        {
            ExecuteButtonCommand(button);
        }

        // Context menu for button
        if (ImGui.BeginPopupContextItem($"context_{buttonIndex}"))
        {
            ImGui.Text(button.Label);
            ImGui.Separator();
            
            if (ImGui.MenuItem("Edit Button"))
            {
                OpenEditButtonPopup(buttonIndex);
            }
            
            if (ImGui.MenuItem("Delete Button"))
            {
                Config.Buttons.RemoveAt(buttonIndex);
                Plugin.Configuration.Save();
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.Separator();
            
            if (ImGui.MenuItem("Configure Smart Button Rules", null, false, true))
            {
                // Open the Smart Button Rules window
                SmartButtonUIManager.OpenSmartButtonRulesWindow(Plugin, this, button);
            }
            
            if (ImGui.MenuItem("Apply Smart Button Template", null, false, true))
            {
                // Open the Template Selector window
                SmartButtonUIManager.OpenTemplateWindow(Plugin, button);
            }
            
            ImGui.EndPopup();
        }

        if (!isEnabled)
        {
            ImGui.EndDisabled();
        }

        ImGui.PopStyleColor(4); // Pop all pushed styles
        ImGui.PopID();
    }
    
    private void OpenEditButtonPopup(int buttonIndex)
    {
        editingButtonIndex = buttonIndex;
        editLabel = Config.Buttons[buttonIndex].Label;
        editCommand = Config.Buttons[buttonIndex].Command;
        isEditPopupOpen = true;
        ImGui.OpenPopup("EditButtonPopup");
    }
    
    private void DrawButtonEditPopup()
    {
        if (isEditPopupOpen && ImGui.BeginPopup("EditButtonPopup"))
        {
            ImGui.Text("Edit Button");
            ImGui.Separator();
            
            ImGui.InputText("Label", ref editLabel, 100);
            ImGui.InputText("Command", ref editCommand, 100);
            
            if (ImGui.Button("Save"))
            {
                if (editingButtonIndex >= 0 && editingButtonIndex < Config.Buttons.Count)
                {
                    Config.Buttons[editingButtonIndex].Label = editLabel;
                    Config.Buttons[editingButtonIndex].Command = editCommand;
                    Plugin.Configuration.Save();
                }
                
                isEditPopupOpen = false;
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Cancel"))
            {
                isEditPopupOpen = false;
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }
    }

    private void ExecuteButtonCommand(Configuration.ButtonData button)
    {
        // Get the current command (may be changed by rules for smart buttons)
        string command = button.IsSmartButton ? ButtonHelper.GetButtonCommand(button) : button.Command;
        
        // Execute the command
        if (!string.IsNullOrEmpty(command))
        {
            try
            {
                Plugin.CommandManager.ProcessCommand(command);
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error executing command: {command}. Error: {ex.Message}");
            }
        }
    }
}
