using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class IcosahedronGen : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    [Header("Property")] 
    //[Range(0,50)][SerializeField] private int planetSizeMutl = 1;
    [Range(0,6)][SerializeField] private int planetSubdivision = 1;
    [SerializeField] private Material _material;

    [Header("Noise")] 
    [SerializeField] private bool enableNoise = true;
    [SerializeField] private bool useSeed;
    [ShowIf("useSeed")][SerializeField] private int seed;
    [ShowIf("enableNoise")][Range(0,0.01f)][SerializeField] private float frequency = 1;
    [ShowIf("enableNoise")][Range(0,200)][SerializeField] private float amplitude = 1;
    [ShowIf("enableNoise")][Range(0,100)][SerializeField] private float persistence = 1;
    [ShowIf("enableNoise")][Range(0,25)][SerializeField] private int octave = 1;
    private int randomSeed;
    
    [Header("Debug")] 
    [SerializeField] private bool debugVertices = false;
    [SerializeField] private bool debugEdges = false;
    [ShowIf("debugEdges")][Range(0,3)][SerializeField] private int thickness = 2;
    [ShowIf("debugEdges")][SerializeField] private Color lineColor = Color.green;

    private List<Vector3> vertices;
    private List<int> triangles;
    private Dictionary<long, int> midTriangles;

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }


    [Button]
    private void Generate()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        vertices = new List<Vector3>();
        triangles = new List<int>();
        midTriangles = new Dictionary<long, int>();
        
        randomSeed = 0;
        randomSeed = useSeed ? seed : Random.Range(-1000, 1000);
        
        GetAllVertex();
        GetAllTriangle();
        
        for (int i = 0; i < planetSubdivision; i++)
        {
            Subdivision();
        }
        
        
        AssembleMesh();
    }

    #region Vertex 

    private void AddVertex(float x, float y, float z)
    {
        // normalize the vector for a unit sphere
        Vector3 vertex = new Vector3(x, y, z).normalized;

        Vector3 noiseVertex = Vector3.zero;
        if (enableNoise)
        {
            noiseVertex = vertex * (Noise.Noise3D(x, y, z, frequency, amplitude, persistence, octave, randomSeed));
        }
            

        vertices.Add(enableNoise ? noiseVertex : vertex);
    }
    
    private void GetAllVertex()
    {
        // Golden ratio
        var phi = (1 + Mathf.Sqrt(5) / 2);
        
        // 12 Vertices 
        AddVertex(-1, phi, 0);
        AddVertex(1, phi, 0);
        AddVertex(-1, -phi, 0);
        AddVertex(1, -phi, 0);
        
        AddVertex(0, -1, phi);
        AddVertex(0, 1, phi);
        AddVertex(0, -1, -phi);
        AddVertex(0, 1, -phi);
        
        AddVertex(phi, 0, -1);
        AddVertex(phi, 0, 1);
        AddVertex(-phi, 0, -1);
        AddVertex(-phi, 0, 1);
    }
    

    #endregion
    
    #region Triangle

    private void AddTriangles(int x, int y, int z)
    {
        triangles.Add(x);
        triangles.Add(y);
        triangles.Add(z);
    }
    
    
    private void GetAllTriangle()
    {
        // 20 triangles
        AddTriangles(0,11,5);
        AddTriangles(0,5,1);
        AddTriangles(0,1,7);
        AddTriangles(0,7,10);
        AddTriangles(0,10,11);
        
        AddTriangles(1,5,9);
        AddTriangles(5,11,4);
        AddTriangles(11,10,2);
        AddTriangles(10,7,6);
        AddTriangles(7,1,8);
        
        AddTriangles(3,9,4);
        AddTriangles(3,4,2);
        AddTriangles(3,2,6);
        AddTriangles(3,6,8);
        AddTriangles(3,8,9);

        AddTriangles(4,9,5);
        AddTriangles(2,4,11);
        AddTriangles(6,2,10);
        AddTriangles(8,6,7);
        AddTriangles(9,8,1);
    }

    #endregion

    #region Subdivision

    private void Subdivision()
    {
        var newTriangle = new List<int>();
        
        for (int i = 0; i < triangles.Count; i += 3) // Move between each triangle
        {
            // get the triangle
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];
            
            // get a new vertices
            var a = GetMidPoint(v1, v2);
            var b = GetMidPoint(v2, v3);
            var c = GetMidPoint(v3, v1);
            
            newTriangle.AddRange(new int[]
            {
                v1, a, c,
                v2, b, a, 
                v3, c, b,
                a, b, c
            });
        }
        triangles = newTriangle;
    }

    int GetMidPoint(int v1,int v2)
    {
        long min = Mathf.Min(v1, v2);
        long max = Mathf.Max(v1, v2);
        // Unique Key
        long key = (min*32*32) + max; 

        if (midTriangles.TryGetValue(key, out int midpointIndex))
        {
            return midpointIndex;
        }
        
        // Creating new vertex
        Vector3 midpoint = (vertices[v1] + vertices[v2]) * 0.5f;
        AddVertex(midpoint.x, midpoint.y, midpoint.z);
        
        midpointIndex = vertices.Count - 1;
        midTriangles[key] = midpointIndex; 
        
        return midpointIndex;
    }

    #endregion

    private void AssembleMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        
        meshRenderer.sharedMaterial = new Material(_material);
    }
    
    #if UNITY_EDITOR
    [ExecuteAlways]
    private void OnDrawGizmos()
    {
        if(!Application.isPlaying) return;

        if (debugVertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                GUIStyle style = new GUIStyle()
                {
                    fontSize = 25,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.red
                    }
                };
                Handles.Label(new Vector3(vertices[i].x, vertices[i].y, vertices[i].z), i.ToString(), style);
            }
        }

        if (debugEdges)
        {
            Handles.color = lineColor;
            for (int i = 0; i < triangles.Count; i += 3) // Move between each triangle
            {
                // get the triangle
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];
            
                // Get the vertices
                Handles.DrawLine(vertices[v1], vertices[v2], thickness);
                Handles.DrawLine(vertices[v2], vertices[v3], thickness);
                Handles.DrawLine(vertices[v3], vertices[v1], thickness);
            }
        }
    }
    #endif
}
