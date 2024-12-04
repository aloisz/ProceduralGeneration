using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "PlanetSO", menuName = "Scriptable Objects/PlanetSO")]
public class PlanetSO : ScriptableObject
{
    [Header("Property")] 
    public string planetName;
    [Range(0,50)]public int planetSize = 1;
    [Range(0,6)]public int planetSubdivision = 1;
    public Material _material;

    [Header("Noise")] 
    public bool enableNoise = true;
    public bool useSeed;
    [ShowIf("useSeed")]public int seed;
    [ShowIf("enableNoise")][Range(0,100)]public float noiseScale = 1f;
    [ShowIf("enableNoise")][Range(0,10)]public float frequency = 1;
    [ShowIf("enableNoise")][Range(0,20)]public float amplitude = 1;
    [ShowIf("enableNoise")][Range(0,10)]public float persistence = 1;
    [ShowIf("enableNoise")][Range(0,50)]public int octave = 1;
    
    
    [Header("VertexColor")]
    public Gradient gradient;

    [Header("Decoration")]
    public List<Decoration> Decorations = new List<Decoration>();
}

[System.Serializable]
public class Decoration
{
    public string name;
    public GameObject gameObject;
    [Range(0,1)] public float probability;
    [MinMaxSlider(0, 1)] public Vector2 minMaxSpawnPos;
    [MinMaxSlider(.01f, .2f)] public Vector2 SizeOfObj;
    [MinMaxSlider(0, 1)]public Vector2 minMaxObjectNormalRotation;
    public Color color;
    public float colorTolerance;
}
