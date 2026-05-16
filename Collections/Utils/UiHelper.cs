using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;

namespace Collections;

public class ColorsPalette
{
    public static readonly Vector4 WHITE = new(1.000f, 1.000f, 1.000f, 1.000f);
    public static readonly Vector4 GREY = new(0.378f, 0.378f, 0.352f, 1.000f);
    public static readonly Vector4 GREY2 = new(0.682f, 0.682f, 0.682f, 0.95f);
    public static readonly Vector4 BLACK = new(0.000f, 0.000f, 0.000f, 1.000f);
    public static readonly Vector4 YELLOW = new(0.915f, 0.904f, 0.155f, 1f);
    public static readonly Vector4 DARKER_YELLOW = new(0.821f, 0.788f, 0.082f, 0.706f);
    public static readonly Vector4 PURPLE = new(0.306f, 0.143f, 0.532f, 0.811f);
    public static readonly Vector4 BLUE = new(0.344f, 0.436f, 0.960f, 0.786f);
    public static readonly Vector4 GREEN = new(0.008f, 0.593f, 0.140f, 0.5f);
    public static readonly Vector4 RED = new(0.719f, 0.109f, 0.109f, 0.9f);
    public static readonly Vector4 LIME_GREEN = new Vector4(0f, 1f, 0f, 1f);
}

public class UiHelper
{
    public static float UnitHeight()
    {
        return ImGui.CalcTextSize("A").Y;
    }

    public static float UnitWidth()
    {
        return ImGui.CalcTextSize("A").X;
    }

    // Returns the scaling that should be used based on font
    public static float ScaleForFontSize(float original)
    {
        // 17 is the default font size for 100% scaling
        return ImGui.GetFontSize() / 17 * original;
    }

    public static float GetLengthToBottomOfWindow()
    {
        var cursorY = ImGui.GetCursorPos().Y;
        var windowY = ImGui.GetWindowSize().Y;
        return windowY - cursorY - UnitHeight() * 0.3f;
    }

    public static float GetLengthToRightOfWindow()
    {
        var cursorX = ImGui.GetCursorPos().X;
        var windowX = ImGui.GetWindowSize().X;
        // var unitX = ImGui.CalcTextSize(" ").X;
        return windowX - cursorX;
    }

    public static float GetScrollPosition()
    {
        return (ImGui.GetScrollMaxY() - ImGui.GetScrollY()) / (ImGui.GetScrollMaxY() + 1);
    }

    public static void GroupWithMinWidth(System.Action draw, float minWidth)
    {
        ImGui.BeginGroup();

        //var originalPosX = ImGui.GetCursorPosX();
        var origPos = ImGui.GetCursorPos();
        ImGui.InvisibleButton("ignore", new Vector2(minWidth <= 0 ? 1 : minWidth, 1));
        ImGui.SetCursorPos(origPos);

        draw();
        //var drawnWidth = ImGui.GetCursorPosX() - originalPosX;
        //if (minWidth > drawnWidth)
        //{
        //    ImGui.SameLine();
        //    ImGui.InvisibleButton("ignore", new Vector2(minWidth - drawnWidth, 0.01f));
        //}
        ImGui.EndGroup();
    }

    //public static void WithColor(ImGuiCol col, Vector4 color, System.Action draw)
    //{
    //    ImGui.PushStyleColor(col, color);
    //    try
    //    {
    //        draw();
    //    }
    //    catch (Exception)
    //    {
    //        ImGui.PopStyleColor();
    //        throw;
    //    }
    //    ImGui.PopStyleColor();
    //}

    public static Vector2 OffsetStart(Vector2 vec)
    {
        var currentPos = ImGui.GetCursorPos();
        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPos().X - vec.X, ImGui.GetCursorPos().Y - vec.Y));
        return currentPos;
    }

    public static void OffsetEnd(Vector2 vec)
    {
        ImGui.SetCursorPos(vec);
    }

    public static bool IconButtonStateful(string id, FontAwesomeIcon fontAwesomeIcon, ref bool state, Vector4 offColor, Vector4 onColor, string tooltip = "")
    {
        var originalState = state;

        ImGui.PushStyleColor(ImGuiCol.Text, state ? onColor : offColor);
        if (ImGuiComponents.IconButton(id, fontAwesomeIcon))
        {
            state = !state;
        }
        ImGui.PopStyleColor();

        if (tooltip != "")
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(tooltip);
            }
        }

        return originalState != state;
    }

    public static unsafe void DisabledIconButton(FontAwesomeIcon fontAwesomeIcon, string text)
    {
        var buttonColor = *ImGui.GetStyleColorVec4(ImGuiCol.Button);
        ImGuiComponents.IconButtonWithText(fontAwesomeIcon, text, null, buttonColor, buttonColor);
    }

    public static void IconButtonWithOffset(int id, FontAwesomeIcon fontAwesomeIcon, float Xoffset, float Yoffset, ref bool state, float scale)
    {
        IconButtonWithOffset(id, fontAwesomeIcon, Xoffset, Yoffset, ref state, scale, ImGuiColors.DalamudYellow, ImGuiColors.DalamudGrey);
    }

    public static void IconButtonWithOffset(int id, FontAwesomeIcon fontAwesomeIcon, float Xoffset, float Yoffset, ref bool state, float scale, Vector4 enabledColor, Vector4 disabledColor)
    {
        // ID
        ImGui.PushID(id);

        // Handle pos offset
        var previousPos = ImGui.GetCursorPos();
        ImGui.SameLine();
        ImGui.SetItemAllowOverlap(); // Makes this button take precedence
        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPos().X - Xoffset, ImGui.GetCursorPos().Y - Yoffset));

        // Font
        ImGui.PushStyleColor(ImGuiCol.Text, state ? enabledColor : disabledColor);

        // Button
        ImGui.SetWindowFontScale(scale);
        if (ImGuiComponents.IconButton(fontAwesomeIcon.ToIconString(), new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0)))
        {
            state = !state;
        }
        ImGui.SetWindowFontScale(1f);

        // Reset state
        ImGui.SetCursorPos(previousPos);
        ImGui.PopStyleColor();
        ImGui.PopID();
        
    }

    public static void IconButtonWithPopUpInputText(FontAwesomeIcon fontAwesomeIcon, Action<string> onInput)
    {
        ImGuiComponents.IconButton(fontAwesomeIcon.ToIconString());
        InputText(fontAwesomeIcon.ToString(), onInput);
    }

    public static void InputText(string key, Action<string> onInput)
    {
        var input = String.Empty;
        if (onInput is not null)
        {
            if (ImGui.BeginPopupContextItem($"##{key}", ImGuiPopupFlags.MouseButtonLeft))
            {
                if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                    ImGui.CloseCurrentPopup();

                if (ImGui.IsWindowAppearing())
                    ImGui.SetKeyboardFocusHere();

                var enterPressed = ImGui.InputTextWithHint("##newName", "Enter New Name...", ref input, 50, ImGuiInputTextFlags.EnterReturnsTrue);

                if (enterPressed)
                {
                    if (input != "")
                    {
                        onInput(input);
                        ImGui.CloseCurrentPopup();
                    }
                }
                ImGui.EndPopup();
            }
        }
    }

    public static bool ImageToggleButton(ISharedImmediateTexture icon, Vector2 size, bool state)
    {
        var tintColor = new Vector4(1f, 1f, 1f, 1f);
        if (!state)
        {
            tintColor = new Vector4(0.5f, 0.5f, 0.5f, 0.85f);
        }
        return ImGui.ImageButton(icon.GetWrapOrEmpty().Handle, size, Vector2.Zero, Vector2.One, 1, Vector4.Zero, tintColor);
    }


    public static void ItemBox()
    {
        var tabs = new List<string> { "1", "2", "3", "4" };
        var y = 3;
        ImGui.ListBox("x", ref y, tabs.ToArray(), 4);
    }

    public static void scaledText(string text, float scale)
    {
        //ImGui.SetWindowFontScale((float)1.3);
        var old_scale = ImGui.GetFont().Scale;
        ImGui.GetFont().Scale *= scale;
        ImGui.PushFont(ImGui.GetFont());
        ImGui.Text(text);
        ImGui.GetFont().Scale = old_scale;
        ImGui.PopFont();
        //ImGui.SetWindowFontScale(1);
    }

    public static void RectOverLastItem()
    {
        ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), 0xFF00FF00, 0.5f);
    }

    public static void WriteTextOverlay(string text, Vector4? textColor = null, Vector4? backgroundColor = null, float padding = 4f)
    {
        var drawList = ImGui.GetWindowDrawList();

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();

        var textSize = ImGui.CalcTextSize(text);

        var pos = new Vector2(min.X + padding, max.Y - textSize.Y - padding);

        if (backgroundColor.HasValue)
        {
            drawList.AddRectFilled(pos - new Vector2(3, 2), pos + textSize + new Vector2(3, 2), ImGui.GetColorU32(backgroundColor.Value));
        }

        drawList.AddText(pos, ImGui.GetColorU32(textColor ?? Vector4.One), text);
    }
}
