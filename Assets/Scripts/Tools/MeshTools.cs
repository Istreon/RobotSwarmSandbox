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

    public static System.Tuple<List<Vector3>, List<int>> TranformLineToPolygonUpPoints(Vector3 startPoint, Vector3 endPoint, float width)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //--Face 1--//
        List<Vector3> v = TranformLineToHorizontalFacePoints(startPoint, endPoint, width, false);
        List<int> t = MeshTools.DrawFilledTriangles(v.ToArray());

        vertices.AddRange(v);
        triangles.AddRange(t);

        //--Face 2--//
        v = TranformLineToHorizontalFacePoints(startPoint, endPoint, width, true);
        t = MeshTools.DrawFilledTriangles(v.ToArray());

        //Update triangles indexes before adding them to the triangles list
        for (int k = 0; k < t.Count; k++)
        {
            t[k] += vertices.Count;
        }

        vertices.AddRange(v);
        triangles.AddRange(t);

        //--Face 3--//
        v = TranformLineToVerticalFacePoints(startPoint, endPoint, width, false);
        t = MeshTools.DrawFilledTriangles(v.ToArray());

        //Update triangles indexes before adding them to the triangles list
        for (int k = 0; k < t.Count; k++)
        {
            t[k] += vertices.Count;
        }

        vertices.AddRange(v);
        triangles.AddRange(t);

        //--Face 4--//
        v = TranformLineToVerticalFacePoints(startPoint, endPoint, width, true);
        t = MeshTools.DrawFilledTriangles(v.ToArray());

        //Update triangles indexes before adding them to the triangles list
        for (int k = 0; k < t.Count; k++)
        {
            t[k] += vertices.Count;
        }

        vertices.AddRange(v);
        triangles.AddRange(t);

        return new System.Tuple<List<Vector3>, List<int>>(vertices, triangles);
    }



    private static List<Vector3> TranformLineToHorizontalFacePoints(Vector3 startPoint, Vector3 endPoint, float width, bool side)
    {
        float sideValue = 1;

        if (side)
        {
            sideValue = -1;
        }

        //Calculer la normale à la ligne en paramètre
        Vector3 line = endPoint - startPoint;

        Vector3 perp = new Vector3(-line.z * sideValue, 0.0f, line.x * sideValue);
        perp = perp.normalized;

        Vector3 offset = new Vector3(0.0f, (width / 2) * sideValue, 0.0f);

        List<Vector3> points = new List<Vector3>();
        points.Add(startPoint - (perp * (width / 2)) + offset);
        points.Add(endPoint - (perp * (width / 2)) + offset);
        points.Add(endPoint + (perp * (width / 2)) + offset);
        points.Add(startPoint + (perp * (width / 2)) + offset);

        return points;
    }

    private static List<Vector3> TranformLineToVerticalFacePoints(Vector3 startPoint, Vector3 endPoint, float width, bool side)
    {
        float sideValue = 1;

        if (side)
        {
            sideValue = -1;
        }

        //Calculer la normale à la ligne en paramètre
        Vector3 line = endPoint - startPoint;

        Vector3 perp = new Vector3(-line.y* sideValue, line.x* sideValue, 0.0f) ;
        perp = perp.normalized;

        Vector3 offset = new Vector3(0.0f, 0.0f, -(width / 2)* sideValue);

        List<Vector3> points = new List<Vector3>();
        points.Add(startPoint - (perp * (width / 2)) + offset);
        points.Add(endPoint - (perp * (width / 2)) + offset);
        points.Add(endPoint + (perp * (width / 2)) + offset);
        points.Add(startPoint + (perp * (width / 2)) + offset);

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
