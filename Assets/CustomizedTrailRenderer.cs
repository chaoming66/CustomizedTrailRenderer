using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizedTrailRenderer : MonoBehaviour
{
    public float lifeTime = 0.13f;
    public Material material;
    public Color color = Color.white;
    public float width = 0.5f;
    public float minControlPtnDistance = 0.1f;
    
    private GameObject trail = null;
    private Mesh mesh = null;
    private Vector3[] controlPoints;
    private DisplayPoint[] displayPoints;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private Color[] vertexColors;
    private int[] triangles;
    private int displayCnt = 0;
    private int controlCnt = 0;

    void Start()
    {
        trail = new GameObject("Trail");
               
        controlPoints = new Vector3[4];
        displayPoints = new DisplayPoint[400];

        MeshFilter meshFilter = (MeshFilter)trail.AddComponent(typeof(MeshFilter));
        mesh = meshFilter.mesh;
        trail.AddComponent(typeof(MeshRenderer));
        trail.GetComponent<Renderer>().material = material;

        controlPoints[controlCnt] = transform.position;
        controlCnt++;
    }

    void Update()
    {
        // Eliminates expired displayPoints
        for (int i = displayCnt - 1; i >= 0; i--)
        {
            DisplayPoint point = displayPoints[i];
            if (point == null || point.age > lifeTime)
            {
                displayPoints[i] = null;
                displayCnt--;
            }
        }

        // Create a new control point using object's current position if the distance in between object and the last control point is greater than minControlPtnDistance
        if ((controlPoints[0] - transform.position).magnitude > minControlPtnDistance)
        {
            for (int j = 3; j > 0; j--)
                controlPoints[j] = controlPoints[j - 1];
            controlPoints[0] = transform.position;
            controlCnt++;

            // Camule-Spline need excatly 4 control points in order to generate display points
            if (controlCnt < 3)
                return;

            AddNewDisplayPoints();
        }

        RenderTrail();

    }

    void OnDisable()
    {
        controlCnt = 0;
        displayCnt = 0;
        mesh.Clear();
    }

    /// <summary>    
    /// Calculate new control displayPoints based on input and generate display points based on control points
    /// </summary>
    private void AddNewDisplayPoints()
    {
        int j = displayCnt + 3;

        // We can have as most 400 display points as the size of displayPoints array is 400
        if (displayCnt + 3 > 399)
        {
            j = 399;
        }

        displayCnt = j;

        try
        {
            // Free spaces for new display points
            for (; j > 2; j--)
                displayPoints[j] = displayPoints[j - 3];
        }
        catch (Exception e)
        {
            Debug.Log(j);
        }
        
        // Generating display points in between 4 control points
        displayPoints[2] = new DisplayPoint(CRSpline(0.33f, controlPoints[3], controlPoints[2], controlPoints[1], controlPoints[0]));
        displayPoints[1] = new DisplayPoint(CRSpline(0.66f, controlPoints[3], controlPoints[2], controlPoints[1], controlPoints[0]));
        displayPoints[0] = new DisplayPoint(CRSpline(1f, controlPoints[3], controlPoints[2], controlPoints[1], controlPoints[0]));
    }

    /// <summary>    
    /// Create vertices of trail as well as generate meshs and shade them
    /// </summary>
    private void RenderTrail()
    {
        if (displayCnt == 0)
        {
            mesh.Clear();
            return;
        }

        // For each display point, we genereate two vertices to it's right and left.
        // For every two consecutive displayPoints, we generate 2 triangles 
        // Therefore we get ((# of display displayPoints) - 1) * 2 triangles and each triangles need 3 vertices 
        // as a result we need ((# of display displayPoints) * 6 vertices for all the triangles we need    
        vertices = new Vector3[displayCnt * 2];
        uvs = new Vector2[displayCnt * 2];
        triangles = new int[(displayCnt - 1) * 6];
        vertexColors = new Color[displayCnt * 2];

        float pointRatio = 1f / (displayCnt - 1);
        Vector3 cameraPos = Camera.main.transform.position;

        for (int i = 0; i < displayCnt; i++)
        {
            float ratio = i * pointRatio;

            vertexColors[i * 2] = color;
            vertexColors[(i * 2) + 1] = color;

            // We want the trail to face the camera
            Vector3 from;
            Vector3 to;

            if (i != (displayCnt - 1))
            {
                from = displayPoints[i].position;
                to = displayPoints[i + 1].position;
            }
            else
            {
                from = displayPoints[i - 1].position;
                to = displayPoints[i].position;
            }

            Vector3 pointDir = to - from;
            Vector3 vectorToCamera = cameraPos - displayPoints[i].position;
            Vector3 perpendicular = Vector3.Cross(pointDir, vectorToCamera).normalized;

            // For each display point, we create two vertices on its' right and left side
            vertices[i * 2 + 0] = displayPoints[i].position - perpendicular * width * 0.5f;
            vertices[i * 2 + 1] = displayPoints[i].position + perpendicular * width * 0.5f;

            uvs[i * 2 + 0] = new Vector2(ratio, 0);
            uvs[i * 2 + 1] = new Vector2(ratio, 1);

            if (i > 0)
            {
                int triIndex = (i - 1) * 6;
                int vertIndex = i * 2;
                triangles[triIndex + 0] = vertIndex - 2;
                triangles[triIndex + 1] = vertIndex - 1;
                triangles[triIndex + 2] = vertIndex - 0;

                triangles[triIndex + 3] = vertIndex + 0;
                triangles[triIndex + 4] = vertIndex - 1;
                triangles[triIndex + 5] = vertIndex + 1;
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = vertexColors;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    /// <summary>    
    /// Catmule Spline
    /// </summary>
    /// <param name="t"></param>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p4"></param>
    private Vector3 CRSpline(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 _A = Vector3.zero;
        Vector3 _B = Vector3.zero;
        Vector3 _C = Vector3.zero;

        _A = 3.0f * p1 - p0 - 3.0f * p2 + p3;
        _B = 2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3;
        _C = p2 - p0;

        return p1 + (0.5f * t) * (_C + t * (_B + t * _A));
    }   

    /// <summary>
    /// This is the wrapper class of interpolated point 
    /// </summary>
    class DisplayPoint
    {
        public float timeCreated = 0;
        public float age
        {
            get { return Time.time - timeCreated; }
        }
        public float fadeAlpha = 0;
        public Vector3 position = Vector3.zero;

        public DisplayPoint(Vector3 vec)
        {
            position = vec;
            timeCreated = Time.time;
        }
    }
}