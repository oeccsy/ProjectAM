using System.Collections.Generic;
using UnityEngine;

public static class ColorUtils
{
    private static readonly Dictionary<NpcColor, Color> ColorTable = new Dictionary<NpcColor, Color>
    {
        { NpcColor.Red,    GetColorFromHex("#E63946") },
        { NpcColor.Blue,   GetColorFromHex("#457B9D") },
        { NpcColor.Green,  GetColorFromHex("#2A9D8F") },
        { NpcColor.Purple, GetColorFromHex("#7B2CBF") },
        { NpcColor.Orange, GetColorFromHex("#F4A261") },
        { NpcColor.Brown,  GetColorFromHex("#6F4E37") },
        { NpcColor.White,  GetColorFromHex("#F1FAEE") },
        { NpcColor.Yellow, GetColorFromHex("#E9C46A") },
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
