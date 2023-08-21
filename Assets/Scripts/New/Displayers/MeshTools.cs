using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTools
{
    public static List<Vector3> TranformLineToRectanglePoints(Vector3 startPoint, Vector3 endPoint, float width)
    {
        //Calculer la normale à la ligne en paramètre

        Vector3 line = endPoint - startPoint;

        Vector3 perp = new Vector3(-line.z, 0.0f, line.x);
        perp = perp.normalized;


        List<Vector3> points = new List<Vector3>();
        points.Add(startPoint - (perp * (width / 2)));
        points.Add(endPoint - (perp * (width / 2)));
        points.Add(endPoint + (perp * (width / 2)));
        points.Add(startPoint + (perp * (width / 2)));

        return points;
    }

    /// <summary>
    /// From a list of vertices, prepare the triangles array that will be use in a mesh. 
    /// .......
    /// </summary>
    /// <param name="points"> The list of vertices, with at 0 the origin of each triangles</param>
    /// <returns> The list of index to form mesh triangles</returns>
    public static List<int> DrawFilledTriangles(Vector3[] points)
    {
        int triangleAmount = points.Length - 2;
        List<int> newTriangles = new List<int>();
        for (int i = 0; i < triangleAmount; i++)
        {
            newTriangles.Add(0);
            newTriangles.Add(i + 2);
            newTriangles.Add(i + 1);
        }
        return newTriangles;
    }
}
