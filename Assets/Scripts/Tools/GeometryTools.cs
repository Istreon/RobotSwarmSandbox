using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryTools
{
    /// <summary>
    /// Check if two lines (both composed of 2 vectors) collide.
    /// Original script methods from : https://www.reddit.com/r/gamedev/comments/7ww4yx/whats_the_easiest_way_to_check_if_two_line/
    /// </summary>
    /// <param name="lineOneA">First point of the first line</param>
    /// <param name="lineOneB">Second point of the first line</param>
    /// <param name="lineTwoA">First point of the second line</param>
    /// <param name="lineTwoB">Second point of the second line</param>
    /// <returns>Return true if the two lines collide, false otherwise </returns>
    public static bool LineSegmentsIntersect(Vector2 lineOneA, Vector2 lineOneB, Vector2 lineTwoA, Vector2 lineTwoB)
    {
        return (((lineTwoB.y - lineOneA.y) * (lineTwoA.x - lineOneA.x) > (lineTwoA.y - lineOneA.y) * (lineTwoB.x - lineOneA.x)) != ((lineTwoB.y - lineOneB.y) * (lineTwoA.x - lineOneB.x) > (lineTwoA.y - lineOneB.y) * (lineTwoB.x - lineOneB.x)) && ((lineTwoA.y - lineOneA.y) * (lineOneB.x - lineOneA.x) > (lineOneB.y - lineOneA.y) * (lineTwoA.x - lineOneA.x)) != ((lineTwoB.y - lineOneA.y) * (lineOneB.x - lineOneA.x) > (lineOneB.y - lineOneA.y) * (lineTwoB.x - lineOneA.x)));
    }
}
