using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;

namespace WahButtons.Helpers
{
    public static class LayoutHelper
    {
        public static void HandleGridLayout(List<Configuration.ButtonData> buttons, Vector2 containerSize, int rows, int columns, RenderButtonDelegate renderButton)
        {
            float buttonWidth = ButtonHelper.CalculateDefaultButtonWidth(containerSize, columns);
            float buttonHeight = ButtonHelper.CalculateDefaultButtonHeight(containerSize, rows);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int buttonIndex = row * columns + col;
                    if (buttonIndex >= buttons.Count)
                        break;

                    var button = buttons[buttonIndex];
                    DrawButtonInGrid(button, buttonWidth, buttonHeight, col, columns, renderButton);
                }
            }
        }

        public static void HandleVerticalLayout(List<Configuration.ButtonData> buttons, Vector2 containerSize, RenderButtonDelegate renderButton)
        {
            Vector2 currentPos = new(10, 10);
            foreach (var button in buttons)
            {
                ImGui.SetCursorPos(currentPos);

                float finalWidth = ButtonHelper.GetButtonWidth(button, containerSize.X - 20);
                float finalHeight = ButtonHelper.GetButtonHeight(button, 30);

                renderButton(button, finalWidth, finalHeight);

                currentPos.Y += finalHeight + 10;
            }
        }

        public static void HandleHorizontalLayout(List<Configuration.ButtonData> buttons, Vector2 containerSize, RenderButtonDelegate renderButton)
        {
            Vector2 currentPos = new(10, 10);
            foreach (var button in buttons)
            {
                ImGui.SetCursorPos(currentPos);

                float finalWidth = ButtonHelper.GetButtonWidth(button, 100);
                float finalHeight = ButtonHelper.GetButtonHeight(button, containerSize.Y - 20);

                renderButton(button, finalWidth, finalHeight);

                currentPos.X += finalWidth + 10;
            }
        }

        private static void DrawButtonInGrid(Configuration.ButtonData button, float defaultWidth, float defaultHeight, int col, int columns, RenderButtonDelegate renderButton)
        {
            float finalWidth = ButtonHelper.GetButtonWidth(button, defaultWidth);
            float finalHeight = ButtonHelper.GetButtonHeight(button, defaultHeight);

            renderButton(button, finalWidth, finalHeight);

            if (col < columns - 1)
            {
                ImGui.SameLine();
            }
        }
    }

    public delegate void RenderButtonDelegate(Configuration.ButtonData button, float width, float height);
} 