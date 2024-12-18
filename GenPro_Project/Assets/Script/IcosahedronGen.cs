using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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
    
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Dictionary<long, int> midTriangles;

    private List<GameObject> ObjData = new List<GameObject>();
    private int countDensity;
    
    private RotatingPlanet _rotatingPlanet;

    private void Start()
    {
        GetComponent();
        if(GameManager.Instance != null)
            GameManager.Instance.planets.Add(this);
        Generate();
    }

    [Button]
    public void GetComponent()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        _rotatingPlanet = GetComponent<RotatingPlanet>();
    }


    [Button]
    public async void Generate()
    {
        //StartCoroutine(ScalePlanetCoroutine(false));
        meshRenderer.enabled = false;
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        
        gameObject.name = _planetSo.planetName;
        
        vertices = new List<Vector3>();
        triangles = new List<int>();
        midTriangles = new Dictionary<long, int>();
        
        
        if(ObjData.Count != 0 && ObjData != null) ClearData();
        ObjData = new List<GameObject>();
        
        randomSeed = 0;
        randomSeed = _planetSo.useSeed ? _planetSo.seed : Random.Range(-1000, 1000);
        
        GetAllVertex();
        GetAllTriangle();
        _rotatingPlanet.ResetRotation();
        
        for (int i = 0; i < _planetSo.planetSubdivision; i++)
        {
            await Task.Run(() => Subdivision());
        }

        if(_planetSo.enableNoise)
            await Task.Run(() => ApplyNoise());

        AssembleMesh(mesh);
        //await ScalePlanet();
        
        if(enableVertexPainting)
            VertexColor(mesh);
        
        if (enableDecoration)
            SpawnDecoration();
        
        StartCoroutine(ScalePlanetCoroutine(true));
    }

    public void GenerateDecoration()
    {
        if(ObjData.Count != 0 && ObjData != null) ClearData();
        SpawnDecoration();
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

    private async Task ScalePlanet()
    {
        transform.localScale = Vector3.zero;
        
        float end = Time.time + Random.Range(1f, 1.25f);
        while (Time.time < end)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(_planetSo.planetSize, _planetSo.planetSize, _planetSo.planetSize), Time.deltaTime * 3);

            await Task.Yield();
        } 
        meshRenderer.enabled = true;
    }
    
    private IEnumerator ScalePlanetCoroutine(bool ScaleUP)
    {
        transform.localScale = ScaleUP ? Vector3.zero : new Vector3(_planetSo.planetSize, _planetSo.planetSize, _planetSo.planetSize);
        meshRenderer.enabled = ScaleUP;
        float duration = Random.Range(0.125f, 2.125f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            transform.localScale = Vector3.Lerp(transform.localScale, ScaleUP ? 
                new Vector3(_planetSo.planetSize, _planetSo.planetSize, _planetSo.planetSize) : Vector3.zero, t);
            yield return null;
        }
        transform.localScale = Vector3.one * _planetSo.planetSize;
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
        
        // Vertices Coordinate
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

    #region Noise

    private void ApplyNoise()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            float noiseValue = Noise.Generate3DNoise(vertices[i], _planetSo.noiseScale, _planetSo.frequency,
                _planetSo.amplitude, _planetSo.persistence, _planetSo.octave, randomSeed);
            
            vertices[i] = vertices[i].normalized * (1 + noiseValue); // Adjust vertex position
        }
    }

    #endregion
    
    #region Vertex Color

    private void VertexColor(Mesh mesh)
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        
        foreach (var vertex in vertices)
        {
            float distance = vertex.magnitude;
            if (distance < min) min = distance;
            if (distance > max) max = distance;
        }
        
        Color[] vertexColors = new Color[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float distance = (vertices[i]).magnitude; 
            float normalizedDist = (distance - min) / (max - min); 
            vertexColors[i] = _planetSo.gradient.Evaluate(normalizedDist); 
        }

        mesh.colors = vertexColors;
        mesh.colors = vertexColors;
    }

    #endregion

    #region Decoration
    
    private bool IsColorClose(Color color1, Color color2, float tolerance)
    {
        float diffR = Mathf.Abs(color1.r - color2.r);
        float diffG = Mathf.Abs(color1.g - color2.g);
        float diffB = Mathf.Abs(color1.b - color2.b);
        return (diffR + diffG + diffB) <= tolerance;
    }   
    
    private void SpawnDecoration()
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        
        foreach (var vertex in vertices)
        {
            float distance = vertex.magnitude;
            if (distance < min) min = distance;
            if (distance > max) max = distance;
        }
        RaycastHit hit;
        
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 dir = vertices[i].normalized; 
            
            if (!Physics.Raycast(transform.position, dir, out hit, 50)) continue;
            if (hit.transform.GetComponent<MeshCollider>() == null) continue;
            
            float normalizedHeight = Mathf.InverseLerp(min, max, vertices[i].magnitude);
            Color sampledColor = _planetSo.gradient.Evaluate(normalizedHeight);
            
            for (var j = 0; j < _planetSo.Decorations.Count; j++)
            {
                var decoration = _planetSo.Decorations[j];
                
                if(Random.value >= decoration.probability) break;
                    
                if(IsColorClose(sampledColor, decoration.color, decoration.colorTolerance))
                {
                    int randomGO = Random.Range(0, decoration.ObjDecorations.Count);
                    GameObject obj = Instantiate(decoration.ObjDecorations[randomGO].gameObject, hit.point, Quaternion.identity, transform);
                    
                    float randomScale = Random.Range(decoration.SizeOfObj.x, decoration.SizeOfObj.y);
                    obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
                    
                    ObjSpawing spawn = obj.GetComponent<ObjSpawing>();
                    spawn.SetBaseScale(randomScale);

                    Quaternion spawnRot = Quaternion.SlerpUnclamped(
                        Quaternion.FromToRotation(Vector3.up, hit.normal),
                        Quaternion.identity,
                        Random.Range(decoration.minMaxObjectNormalRotation.x, decoration.minMaxObjectNormalRotation.y)
                    );
                    obj.transform.rotation *= spawnRot;

                    obj.name = decoration.name;
                    ObjData.Add(obj);
                }
            }
        }
    }

    #endregion
    
    
    [Button("Clear Decoration")]
    public void ClearData()
    {
        foreach (var obj in ObjData)
        {
            DestroyImmediate(obj);
        }
        ObjData.Clear();
        countDensity = 0;
    }

    #region Debug

#if UNITY_EDITOR
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

    #endregion
}
