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

    public static System.Tuple<List<Vector3>, List<int>> TranformLineToPolygonUpPoints(Vector3 startPoint, Vector3 endPoint, float width, float height)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //--Face 1--//
        List<Vector3> v = TranformLineToHorizontalFacePoints(startPoint, endPoint, width, height, false);
        List<int> t = MeshTools.DrawFilledTriangles(v.ToArray());

        vertices.AddRange(v);
        triangles.AddRange(t);

        //--Face 2--//
        v = TranformLineToHorizontalFacePoints(startPoint, endPoint, width, height, true);
        t = MeshTools.DrawFilledTriangles(v.ToArray());

        //Update triangles indexes before adding them to the triangles list
        for (int k = 0; k < t.Count; k++)
        {
            t[k] += vertices.Count;
        }

        vertices.AddRange(v);
        triangles.AddRange(t);

        //--Face 3--//
        v = TranformLineToVerticalFacePoints(startPoint, endPoint, width, height, false);
        t = MeshTools.DrawFilledTriangles(v.ToArray());

        //Update triangles indexes before adding them to the triangles list
        for (int k = 0; k < t.Count; k++)
        {
            t[k] += vertices.Count;
        }

        vertices.AddRange(v);
        triangles.AddRange(t);

        //--Face 4--//
        v = TranformLineToVerticalFacePoints(startPoint, endPoint, width, height, true);
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



    private static List<Vector3> TranformLineToHorizontalFacePoints(Vector3 startPoint, Vector3 endPoint, float width, float heigth,  bool side)
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

        Vector3 offset = new Vector3(0.0f, (heigth / 2) * sideValue, 0.0f);

        List<Vector3> points = new List<Vector3>();
        points.Add(startPoint - (perp * (width / 2)) + offset);
        points.Add(endPoint - (perp * (width / 2)) + offset);
        points.Add(endPoint + (perp * (width / 2)) + offset);
        points.Add(startPoint + (perp * (width / 2)) + offset);

        return points;
    }

    private static List<Vector3> TranformLineToVerticalFacePoints(Vector3 startPoint, Vector3 endPoint, float width, float height,  bool side)
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
        points.Add(startPoint - (perp * (height / 2)) + offset);
        points.Add(endPoint - (perp * (height / 2)) + offset);
        points.Add(endPoint + (perp * (height / 2)) + offset);
        points.Add(startPoint + (perp * (height / 2)) + offset);

        return points;
    }

    #region Methods - Cuboid mesh
    public static System.Tuple<List<Vector3>, List<int>> CuboidPoints(float width, float length)
    {
        List<Vector3> points = new List<Vector3>();

        points.Add(new Vector3(-width / 2, 0.0f, -width/2));
        points.Add(new Vector3(-width / 2, length, -width/2));
        points.Add(new Vector3(-width / 2, length, width / 2));
        points.Add(new Vector3(-width / 2, 0.0f, width/2));
        points.Add(new Vector3(width / 2, 0.0f, width/2));
        points.Add(new Vector3(width / 2, length, width/2));
        points.Add(new Vector3(width / 2, length, -width / 2));
        points.Add(new Vector3(width / 2, 0.0f, -width/2));


        List<int> triangles = new List<int>();

        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(1);
                
        triangles.Add(2);
        triangles.Add(0);
        triangles.Add(3);        
        
        triangles.Add(3);
        triangles.Add(5);
        triangles.Add(2);        
        
        triangles.Add(3);
        triangles.Add(4);
        triangles.Add(5);

        triangles.Add(4);
        triangles.Add(6);
        triangles.Add(5);        
        
        triangles.Add(4);
        triangles.Add(7);
        triangles.Add(6);        
        
        triangles.Add(7);
        triangles.Add(1);
        triangles.Add(6);

        triangles.Add(7);
        triangles.Add(0);
        triangles.Add(1);

        triangles.Add(5);
        triangles.Add(6);
        triangles.Add(1);

        triangles.Add(5);
        triangles.Add(1);
        triangles.Add(2);

        triangles.Add(4);
        triangles.Add(0);
        triangles.Add(7);

        triangles.Add(4);
        triangles.Add(3);
        triangles.Add(0);

        return new System.Tuple<List<Vector3>,List<int>>(points,triangles);
    }

    public static System.Tuple<List<Vector3>, List<int>> TranformLineToCuboidPoints(Vector3 startPoint, Vector3 endPoint, float width)
    {   
        Vector3 newDirection = endPoint - startPoint;

        List<Vector3> vertices;
        List<int> triangles;

        float length = newDirection.magnitude;

        System.Tuple<List<Vector3>, List<int>> m= CuboidPoints(width, length);

        vertices=m.Item1;
        triangles= m.Item2;


        Vector3 cuboidNormal = Vector3.up;

        newDirection.Normalize();

        Quaternion rotation = Quaternion.FromToRotation(cuboidNormal, newDirection);

        for(int i=0; i<vertices.Count; i++)
        {
            vertices[i] += startPoint;
        }

        List<Vector3> temp = new List<Vector3>();
        foreach (Vector3 p in vertices)
        {
            temp.Add(RotatePointAroundPivot(p, startPoint, rotation));
        }

        vertices = temp;


        return new System.Tuple<List<Vector3>, List<int>>(vertices, triangles);
    }

    #endregion

    #region Methods - Spring mesh
    public static System.Tuple<List<Vector3>, List<int>> GetSpringMesh(Vector3 position1, Vector3 position2, int nbLoops, int nbVerticesPerLoops, float length, float width, float wireWidth)
    {
        Vector3 circleNormal = Vector3.up;

        Vector3 springNormal = position2 - position1;
        springNormal.Normalize();

        Quaternion rotation = Quaternion.FromToRotation(circleNormal, springNormal);

        float angle = 2 * Mathf.PI / (float)nbVerticesPerLoops;

        float heightStep = length / (nbVerticesPerLoops * nbLoops);

        List<Vector3> circlePoints = new List<Vector3>();

        for (int i = 0; i < nbVerticesPerLoops; i++)
        {
            Vector3 vertex = new Vector3(Mathf.Cos(angle * i) * width/2, 0.0f, Mathf.Sin(angle * i) * width/2);
            circlePoints.Add(vertex);
        }

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < nbLoops * nbVerticesPerLoops; i++)
        {
            points.Add(circlePoints[i % nbVerticesPerLoops] + new Vector3(0.0f, heightStep * i, 0.0f) + position1); ;
        }

        List<Vector3> temp = new List<Vector3>();
        foreach (Vector3 p in points)
        {
            temp.Add(RotatePointAroundPivot(p, position1, rotation));
        }

        points = temp;

        //Initialise lists of vertices and triangles for the mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            //Get the vertices of the line
            System.Tuple<List<Vector3>, List<int>> m = MeshTools.TranformLineToCuboidPoints(points[i], points[(i + 1)], wireWidth);
            List<Vector3> v = m.Item1;
            List<int> t = m.Item2;

            //Update triangles indexes before adding them to the triangles list
            for (int k = 0; k < t.Count; k++)
            {
                t[k] += vertices.Count;
            }

            //Updathe vertices and triangles list with the new line
            vertices.AddRange(v);
            triangles.AddRange(t);
        }

        return new System.Tuple<List<Vector3>, List<int>>(vertices, triangles);
    }
    #endregion


    //https://discussions.unity.com/t/rotate-a-vector-around-a-certain-point/81225/2
    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return rotation * (point - pivot) + pivot;
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
