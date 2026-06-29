using System.Collections.Generic;
using UnityEngine;

public static class ColorUtils
{
    private static readonly Dictionary<NpcColor, Color> ColorTable = new Dictionary<NpcColor, Color>
    {
        { NpcColor.Red,    GetColorFromHex("#F4A3A3") },
        { NpcColor.Blue,   GetColorFromHex("#A5C8E4") },
        { NpcColor.Green,  GetColorFromHex("#A8D5B5") },
        { NpcColor.Purple, GetColorFromHex("#C5B3E6") },
        { NpcColor.Orange, GetColorFromHex("#F6C49A") },
        { NpcColor.Brown,  GetColorFromHex("#C9A98C") },
        { NpcColor.White,  GetColorFromHex("#EFEDE6") },
        { NpcColor.Yellow, GetColorFromHex("#F7E1A0") },
    };

    public static Color GetColor(NpcColor color)
    {
        return ColorTable.TryGetValue(color, out var value) ? value : Color.magenta;
    }

    public static Color GetColorFromHex(string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
    }

    public static List<NpcColor> GetNpcColorList()
    {
        List<NpcColor> npcColors = new List<NpcColor>();

        for (int i = 0; i < (int)NpcColor.Count; i++)
        {
            npcColors.Add((NpcColor)i);
        }

        return npcColors;
    }
}
