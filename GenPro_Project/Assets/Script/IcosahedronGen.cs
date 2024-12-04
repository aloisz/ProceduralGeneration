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
    private MeshCollider meshCollider;

    [Header("SO")] 
    [Expandable] [SerializeField] private PlanetSO _planetSo;
    
    private int randomSeed;

    [Header("Vertex painting")] [SerializeField]
    private bool enableVertexPainting;
    
    [Header("Decoration")] [SerializeField]
    private bool enableDecoration;
    
    [Header("Debug")] 
    [SerializeField] private bool debugVertices = false;
    [SerializeField] private bool debugEdges = false;
    [ShowIf("debugEdges")][Range(0,3)][SerializeField] private int thickness = 2;
    [ShowIf("debugEdges")][SerializeField] private Color lineColor = Color.green;

    private List<Vector3> vertices;
    private List<int> triangles;
    private Dictionary<long, int> midTriangles;

    private List<GameObject> allDecorativeOBJ;

    private void Awake()
    {
        
    }

    private void Start()
    {
        Generate();
    }


    [Button]
    private void Generate()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        transform.localScale = new Vector3(_planetSo.planetSize, _planetSo.planetSize, _planetSo.planetSize);
        
        Mesh mesh = new Mesh();

        gameObject.name = _planetSo.planetName;
        
        vertices = new List<Vector3>();
        triangles = new List<int>();
        midTriangles = new Dictionary<long, int>();
        allDecorativeOBJ = new List<GameObject>();
        
        randomSeed = 0;
        randomSeed = _planetSo.useSeed ? _planetSo.seed : Random.Range(-1000, 1000);
        
        GetAllVertex();
        GetAllTriangle();
        for (int i = 0; i < _planetSo.planetSubdivision; i++)
        {
            Subdivision();
        }
        
        ApplyNoise();

        AssembleMesh(mesh);
        
        if(enableVertexPainting)
            VertexColor(mesh);
        
        if(enableDecoration)
            SpawnDecoration();
    }

    #region Vertex 

    private void AddVertex(float x, float y, float z)
    {
        // normalize the vector for a unit sphere
        Vector3 vertex = new Vector3(x, y, z).normalized;
        vertices.Add(vertex);
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
        long key = (min*128*128) + max; 

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

    private void ApplyNoise()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            float noiseValue = Noise.Generate3DNoise(vertices[i], _planetSo.noiseScale, _planetSo.frequency, _planetSo.amplitude, _planetSo.persistence, _planetSo.octave, randomSeed);
            vertices[i] = vertices[i].normalized * (1 + noiseValue); // Adjust vertex position
        }
    }
    

    private void AssembleMesh(Mesh mesh)
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
        
        meshRenderer.sharedMaterial = new Material(_planetSo._material);
        meshCollider.sharedMesh = mesh;
    }

    private void VertexColor(Mesh mesh)
    {
        /*Color[] colors = new Color[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = 0; j < _planetSo.VertexColors.Count; j++)
            {
                if (Vector3.Distance(vertices[i] + transform.position, transform.localPosition) >= _planetSo.VertexColors[j].dist)
                {
                    colors[i] = _planetSo.VertexColors[j].color;
                }
            }
        }
        mesh.colors = colors;*/
        
        /*float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (var vertex in vertices)
        {
            if (vertex.y < minY) minY = vertex.y;
            if (vertex.y > maxY) maxY = vertex.y;
        }

        Color[] vertexColors = new Color[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float normalizedY = Mathf.InverseLerp(minY, maxY, );
            vertexColors[i] = _planetSo.gradient.Evaluate(normalizedY);
        }

        mesh.colors = vertexColors;*/

        float min = float.MaxValue;
        float max = float.MinValue;
        
        foreach (var vertex in vertices)
        {
            float distance = vertex.magnitude;
            if (vertex.y < min) min = distance;
            if (vertex.y > max) max = distance;
        }

        Color[] vertexColors = new Color[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            var distance = Mathf.Sqrt((vertices[i].x * vertices[i].x) + (vertices[i].y * vertices[i].y) +
                                      (vertices[i].z * vertices[i].z));
                
            var normalizedDist = (distance - min) / (max - min);

            vertexColors[i] = _planetSo.gradient.Evaluate(normalizedDist);
        }
        mesh.colors = vertexColors;
    }

    private void SpawnDecoration()
    {
        if (allDecorativeOBJ.Count != 0)
        {
            foreach (var obj in allDecorativeOBJ)
            {
                Destroy(obj);
            }
            allDecorativeOBJ.Clear();
        }
        
        
        RaycastHit hit;
        for (int i = 0; i < _planetSo.Decorations.Count; i++)
        {
            for (int j = 0; j < _planetSo.Decorations[i].amountToSpawn; j++)
            {
                Vector3 dir = Random.onUnitSphere;
                Debug.DrawRay(transform.position, dir * 50, Color.red, 1);
                if (Physics.Raycast(transform.position, dir, out hit, 50))
                {
                    if (hit.transform.GetComponent<Collider>() != null)
                    {
                        Debug.DrawLine(transform.position, hit.point, Color.green, 1);
                    }
                }
            }
        }
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
