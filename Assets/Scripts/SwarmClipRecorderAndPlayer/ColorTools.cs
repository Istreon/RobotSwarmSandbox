using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ColorTools
{
    #region Private fields
    private static float sectionSize = 1.0f / 6.0f;
    #endregion

    #region Methods - Private
    /**
     * This method is an equation corresponding to a trapezoidale
     * Take in entry a float value belonging to the interval [O.Of,1.0f]
     * If a value greater than 1.0f is set in parameter, a modulo will be done on it
     * If a value less than 0.0f, a logErrror will appear and the value returned will be -1;
     * 
     * Return value :
     * -A (float) value between [0.0f,1.0f]
     * -1 if there is a wrong parameter value
     **/
    private static float Equation(float x)
    {
        if (x < 0.0f)
        {
            Debug.LogError("Equation method can't take value less than 0.0f in parameter. Allowed interval [0.0f,1.0f]");
            return -1;
        }

        x = x % 1.0f; //Protection against greater values than 1.0f
        float res = -1.0f;

        if (x >= 0.0f && x < sectionSize)
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
    #endregion

    #region Methods - Public
    /**
     * This method return a color based on the parameter value
     * The parameter value must be in the interval [0.0f,1.0f]
     * 
     * Return value :
     * (Color) The corresponding color depending on the parameter value. Starting whith green color for a parameter value at 0.0f
     **/
    public static Color GetColor(float val)
    {
        return new Color(Equation(val), Equation((2 * sectionSize + val) % 1.0f), Equation((4 * sectionSize + val) % 1.0f));
    }

    /**
     * This method create a list of colors, spaced at a fixed distance from each other
     * 
     * Parameter value :
     * -The number of color generated is define by the parameter "nbColor"
     * -"Start" parameter define the first color of the list, based on the same model of the method getColor
     * 
     * Return value :
     * -Return a List of colors (List<Color>)
     **/
    public static List<Color> GetColorPalette(int nbColor, float start = 0.0f)
    {
        List<Color> colorPalette = new List<Color>();

        float palier = 1.0f / nbColor;
        for (float i = 0.0f; i < 1.0f; i += palier)
        {
            //colorPalette.Add(new Color(Equation(i + start), Equation((i + 2 * sectionSize + start) % 1.0f), Equation((i + 4 * sectionSize + start) % 1.0f)));
            colorPalette.Add(GetColor(i+start));        
        }

        return colorPalette;
    }


    /**
     * This method create a list of colors, shuffled, based on the method "GetColorPalette"
     * 
     * Parameter value :
     * -The number of color generated is define by the parameter "nbColor"
     * -"Start" parameter define the first color of the list, based on the same model of the method getColor
     * 
     * Return value :
     * -Return a shuffled list of colors (List<Color>)
     **/
    public static List<Color> GetShuffledColorPalette(int nbColor, float start = 0.0f)
    {
        List<Color> colorPalette = GetColorPalette(nbColor, start);
        var rnd = new System.Random();
        colorPalette = colorPalette.OrderBy(item => rnd.Next()).ToList<Color>();

        return colorPalette;
    }
    #endregion
}
