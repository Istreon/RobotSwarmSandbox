using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTools
{
    public static List<Color> GetColorPalette(int nbColor)
    {
        
        List<Color> colorPalette = new List<Color>();

        colorPalette.Add(new Color(1.0f, 0.0f, 0.0f));
        colorPalette.Add(new Color(0.0f, 0.0f, 1.0f));
        colorPalette.Add(new Color(0.0f, 1.0f, 0.0f));

        colorPalette.Add(new Color(1.0f, 1.0f, 0.0f));
        colorPalette.Add(new Color(1.0f, 0.0f, 1.0f));
        colorPalette.Add(new Color(0.0f, 1.0f, 1.0f));


        int colorMissing = nbColor - 6;
        float t = Mathf.Ceil((float) colorMissing / 6.0f);


        for (int i = 1; i <= t; i++)
        {
            float val = (float)i / (float)(t+1.0f);
            colorPalette.Add(new Color(1.0f, val, 0.0f));
            colorPalette.Add(new Color(1.0f, 0.0f, val));

            colorPalette.Add(new Color(val, 1.0f, 0.0f));
            colorPalette.Add(new Color(0.0f, 1.0f, val));

            colorPalette.Add(new Color(val, 0.0f, 1.0f));
            colorPalette.Add(new Color(0.0f, val, 1.0f));
        }



        //Removing surplus generated color
        int nbSurplus = colorPalette.Count - nbColor;
        colorPalette.RemoveRange(colorPalette.Count - nbSurplus, nbSurplus);

        return colorPalette;
    }
}
