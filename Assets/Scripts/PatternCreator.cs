using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternCreator : MonoBehaviour
{
    Camera mainCamera;

    List<Vector2> patternVertices;

    List<LineRenderer> patternRenderer;


    private bool isDrawing = false;


    private float MapSize = 5.0f;
    private int cutNumber = 50;

    private bool[,] vectorFieldState;
    private Vector2[,] vectorField;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = FindObjectOfType<Camera>();
        patternRenderer = new List<LineRenderer>();

        vectorFieldState = new bool[cutNumber,cutNumber];
        vectorField = new Vector2[cutNumber,cutNumber];

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!isDrawing)
            {
                isDrawing = true;
                patternVertices = new List<Vector2>();
                Debug.Log("I start drawing!");
            }

            Vector3 mousePos = Input.mousePosition;
            {
                Ray temp = mainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));
                
                float scale=temp.origin.y/ temp.direction.y;

                Vector3 res = temp.GetPoint(-scale);

                //Round to the second decimal place
                res.x = Mathf.Round(res.x * 100f) / 100f;
                res.z = Mathf.Round(res.z * 100f) / 100f;

                
                Vector2 vertex = new Vector2(res.x, res.z);

                if(patternVertices.Count>0)
                {
                    Vector2 lastVertex = patternVertices[patternVertices.Count - 1];
                    if (lastVertex.x != vertex.x || lastVertex.y != vertex.y)
                    {
                        patternVertices.Add(vertex);
                        Debug.Log(vertex);
                    }
                } else
                {
                    patternVertices.Add(vertex);
                    Debug.Log(vertex);
                }

                
                
            }
        } else
        {
            if (isDrawing)
            {
                isDrawing = false;
                Debug.Log("I finished!");
                Debug.Log(patternVertices.Count);
                DrawPattern();
                CalculateVectorFieldState();
                CalculateVectorField();
                SmoothingVectorField();
                DrawVectorField();
            }
        }
    }

    private void CalculateVectorFieldState()
    {
        float step = MapSize / cutNumber;
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                float x = step * i;
                float y = step * j;

                Vector2 lineOneA = new Vector2(x, y);
                Vector2 lineOneB = new Vector2(-10, 10);

                Vector2 lineTwoA = patternVertices[patternVertices.Count - 1];

                int count = 0;
                foreach (Vector2 v in patternVertices)
                {
                    if (lineSegmentsIntersect(lineOneA, lineOneB, lineTwoA, v)) count++;
                    lineTwoA = v;
                }
                if (count % 2 == 1) vectorFieldState[i, j] = true;
                else vectorFieldState[i, j] = false;
            }
        }
    }

    private void CalculateVectorField()
    {
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                if(vectorFieldState[i,j])
                {
                    vectorField[i, j] = Vector2.zero;
                } else
                {
                    if(NeighboringCaseIncludedInTheForm(i, j))
                    {
                        vectorField[i, j]=GetNeighboringInTheFormDirection(i, j);
                    } else
                    {
                        vectorField[i, j] = Vector2.zero; //Will be updated during the smoothing phase
                    }
                }
            }
        }
    }

    private void SmoothingVectorField()
    {
        int iterationNb = 20;
        for(int k=0; k<iterationNb; k++)
        {
            Debug.Log("OK");
            for (int i = 0; i < cutNumber; i++)
            {
                for (int j = 0; j < cutNumber; j++)
                {
                    if (vectorFieldState[i, j])
                    {
                        //Do nothing
                    }
                    else
                    {
                        if (NeighboringCaseIncludedInTheForm(i, j))
                        {
                            //Do nothing
                        }
                        else
                        {
                            vectorField[i, j] = GetNeighboringMeanForce(i, j); 
                        }
                    }
                }
            }
        }
    }

    private bool NeighboringCaseIncludedInTheForm(int x, int y)
    {
        bool res = false;
        for(int i=-1; i<2; i++)
        {
            for(int j=-1; j<2; j++)
            {
                int x2 = x + i;
                int y2 = y + j;

                if (x2 < 0 || x2 >= cutNumber || y2 < 0 || y2 >= cutNumber) break;
                else if (vectorFieldState[x2, y2]) res = true;
            }
        }
        return res;
    }

    private Vector2 GetNeighboringInTheFormDirection(int x, int y)
    {
        Vector2 res = Vector2.zero;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int x2 = x + i;
                int y2 = y + j;

                if (x2 < 0 || x2 >= cutNumber || y2 < 0 || y2 >= cutNumber) break;
                else if (vectorFieldState[x2, y2]) res += new Vector2(i, j);
            }
        }
        return res.normalized;
    }

    private Vector2 GetNeighboringMeanForce(int x, int y)
    {
        Vector2 res = Vector2.zero;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int x2 = x + i;
                int y2 = y + j;

                if (x2 < 0 || x2 >= cutNumber || y2 < 0 || y2 >= cutNumber) break;
                else res += vectorField[x2, y2];
            }
        }
        return res.normalized;
    }


    private void DrawVectorFieldState()
    {
        float step = MapSize / cutNumber;
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                float x = step * i;
                float y = step * j;

                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                if(vectorFieldState[i,j]) lineRenderer.material.color = Color.red;
                else lineRenderer.material.color = Color.blue;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 4;
                lineRenderer.useWorldSpace = true;

                lineRenderer.SetPosition(0, new Vector3(x, 0.1f, y)); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(1, new Vector3(x+0.01f, 0.1f, y + 0.01f)); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(2, new Vector3(x, 0.1f, y + 0.01f)); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(3, new Vector3(x, 0.1f, y)); //x,y and z position of the starting point of the line

            }
        }
    }

    private void DrawVectorField()
    {
        float step = MapSize / cutNumber;
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                float x = step * i;
                float y = step * j;

                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                if (vectorFieldState[i, j]) lineRenderer.material.color = Color.red;
                else lineRenderer.material.color = Color.blue;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true;

                lineRenderer.SetPosition(0, new Vector3(x, 0.1f, y)); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(1, new Vector3(x + vectorField[i, j].x / 20.0f, 0.1f, y + vectorField[i, j].y / 20.0f)); ; //x,y and z position of the starting point of the line
            }
        }
    }

    private void DrawPattern()
    {
        LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
        lineRenderer.material.color = Color.black;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = patternVertices.Count+1;
        lineRenderer.useWorldSpace = true;

        int count = 0;
        foreach (Vector2 v in patternVertices)
        {
            lineRenderer.SetPosition(count, new Vector3(v.x, 0.1f, v.y)); //x,y and z position of the starting point of the line
            count++;
        }
        lineRenderer.SetPosition(count, new Vector3(patternVertices[0].x, 0.1f, patternVertices[0].y)); //x,y and z position of the starting point of the line
        patternRenderer.Add(lineRenderer);
    }




    //Original script methods from : https://www.reddit.com/r/gamedev/comments/7ww4yx/whats_the_easiest_way_to_check_if_two_line/
    private bool lineSegmentsIntersect(Vector2 lineOneA, Vector2 lineOneB, Vector2 lineTwoA, Vector2 lineTwoB) 
    { 
        return (((lineTwoB.y - lineOneA.y) * (lineTwoA.x - lineOneA.x) > (lineTwoA.y - lineOneA.y) * (lineTwoB.x - lineOneA.x)) != ((lineTwoB.y - lineOneB.y) * (lineTwoA.x - lineOneB.x) > (lineTwoA.y - lineOneB.y) * (lineTwoB.x - lineOneB.x)) && ((lineTwoA.y - lineOneA.y) * (lineOneB.x - lineOneA.x) > (lineOneB.y - lineOneA.y) * (lineTwoA.x - lineOneA.x)) != ((lineTwoB.y - lineOneA.y) * (lineOneB.x - lineOneA.x) > (lineOneB.y - lineOneA.y) * (lineTwoB.x - lineOneA.x))); 
    }

    public Vector2 GetEnvironmentalForce(Vector2 position)
    {
        float step = MapSize / cutNumber;
        int x = (int)(position.x / step);
        int y = (int)(position.y / step);

        return vectorField[x, y];
    }
}
