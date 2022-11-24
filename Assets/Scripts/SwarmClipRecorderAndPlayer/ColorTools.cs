using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ColorTools
{
    private static float sectionSize = 1.0f / 6.0f;

    private static float Equation(float x)
    {

        x = (x + 1.0f) % 1.0f; //Protection against greater values than 1.0f and partial protection against value under 0.0f;
        float res = -1.0f;
        if (x < sectionSize)
        {
            float a = 1.0f / 0.17f;

            res = a * x;
        }

        if (x >= sectionSize && x <= sectionSize * 3.0f)
        {
            res = 1.0f;
        }

        if (x > sectionSize * 3.0f && x < sectionSize * 4.0f)
        {
            float a = -(1.0f / 0.17f);
            float b = 1.0f;
            float xTemp = x - sectionSize * 3.0f;
            res = a * xTemp + b;
        }

        if (x >= sectionSize * 4.0f && x <= 1.0f)
        {
            res = 0.0f;
        }


        return res;
    }

    public static List<Color> GetColorPalette(int nbColor, float start = 0.0f)
    {
        List<Color> colorPalette = new List<Color>();

        float palier = 1.0f / nbColor;
        for (float i = 0.0f; i < 1.0f; i += palier)
        {
            colorPalette.Add(new Color(Equation(i + start), Equation((i + 2 * sectionSize + start) % 1.0f), Equation((i + 4 * sectionSize + start) % 1.0f)));
        }

        return colorPalette;
    }

    public static List<Color> GetShuffledColorPalette(int nbColor, float start = 0.0f)
    {
        List<Color> colorPalette = GetColorPalette(nbColor, start);
        var rnd = new System.Random();
        colorPalette = colorPalette.OrderBy(item => rnd.Next()).ToList<Color>();

        return colorPalette;
    }
}
