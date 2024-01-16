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

    /// <summary>
    /// This method checks whether the point lies inside the cicumscribed circle of the triangle defined by A, B and C.
    /// It takes vectors3 as parameters, but the calculations are only performed on the x and z coordinates, as the analysis is performed on a plane.
    /// The formulas used are taken from : https://ics.uci.edu/~eppstein/junkyard/circumcenter.html (the second proposition)
    /// </summary>
    /// <param name="point"> The point whose position is being tested.</param>
    /// <param name="A"> First vertex of the triangle. </param>
    /// <param name="B"> Second vertex of the triangle. </param>
    /// <param name="C"> Third vertex of the triangle. </param>
    /// <returns>True if the point lies in the circle, false otherwise.</returns>
    public static bool PointInTheCircumscribedCircle(Vector3 point, Vector3 A, Vector3 B, Vector3 C)
    {
        //Calcul of the value used in the two next formulas
        float d = (A.x - C.x) * (B.z - C.z) - (B.x - C.x) * (A.z - C.z);

        //Calcul of the x coordinate of the circumcenter
        float h = (((A.x - C.x) * (A.x + C.x) + (A.z - C.z) * (A.z + C.z)) / 2.0f * (B.z - C.z)
            - ((B.x - C.x) * (B.x + C.x) + (B.z - C.z) * (B.z + C.z)) / 2.0f * (A.z - C.z)) / d;

        //Calcul of the z coordinate of the circumcenter
        float k = (((B.x - C.x) * (B.x + C.x) + (B.z - C.z) * (B.z + C.z)) / 2.0f * (A.x - C.x)
            - ((A.x - C.x) * (A.x + C.x) + (A.z - C.z) * (A.z + C.z)) / 2.0f * (B.x - C.x)) / d;

        //Calcul of the radius of the circle (squared)
        float rx = (h - A.x);
        float rz = (k - A.z);
        float rSquared = rx * rx + rz * rz;

        //Calcul of the distance between the point and the circumcenter (squared)
        Vector2 point2D = new Vector2(point.x, point.z);
        Vector2 center = new Vector2(h, k);

        float distSquared = Vector2.SqrMagnitude(point2D - center);

        //Check if the distance is less than the radius
        return distSquared < rSquared;
    }


    /// <summary>
    /// This methods checks whether the point lies into a triangle defined by A, B and C.
    /// WARNING : Before use this method, tests must be done to check if it work properly
    /// </summary>
    /// <param name="point">The point whose position is being tested.</param>
    /// <param name="A"> First vertex of the triangle.</param>
    /// <param name="B"> Second vertex of the triangle.</param>
    /// <param name="C"> Third vertex of the triangle.</param>
    /// <returns>True if the point lies in the triangle, false otherwise.</returns>
    public static bool PointInsideTriangle(Vector3 point, Vector3 A, Vector3 B, Vector3 C)
    {
        Vector3 AB = B - A;
        Vector3 AM = point - A;
        Vector3 AC = C - A;
        Vector3 BA = A - B;
        Vector3 BM = point - B;
        Vector3 BC = C - B;
        Vector3 CA = A - C;
        Vector3 CM = point - C;
        Vector3 CB = B - C;

        float condA = Vector3.Dot(Vector3.Cross(AB, AM), Vector3.Cross(AM, AC));

        if (condA < 0) { return false; }


        float condB = Vector3.Dot(Vector3.Cross(BA, BM), Vector3.Cross(BM, BC));

        if (condB < 0) { return false; }


        float condC = Vector3.Dot(Vector3.Cross(CA, CM), Vector3.Cross(CM, CB));

        if (condC < 0) { return false; }

        return true;
    }
}
