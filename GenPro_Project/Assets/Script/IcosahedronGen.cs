using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IcosahedronGen : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private List<Vector3> vertices;
    private List<int> triangles;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        GetAllVertex();
        GetAllTriangle();
        AssembleMesh();
    }

    private void AddVertex(float x, float y, float z)
    {
        vertices.Add(new Vector3(x, y, z).normalized); // normalize the vector for a unit sphere
    }

    private void AddTriangles(int x, int y, int z)
    {
        triangles.Add(x);
        triangles.Add(y);
        triangles.Add(z);
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


    private void AssembleMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
    }

    private void OnDrawGizmos()
    {
        Handles.color = Color.red;
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
}
