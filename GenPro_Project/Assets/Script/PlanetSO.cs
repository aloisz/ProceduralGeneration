using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "PlanetSO", menuName = "Scriptable Objects/PlanetSO")]
public class PlanetSO : ScriptableObject
{
    [Header("Property")] 
    public string planetName;
    [Range(0,6)]public int planetSubdivision = 1;
    public Material _material;

    [Header("Noise")] 
    public bool enableNoise = true;
    public bool useSeed;
    [ShowIf("useSeed")]public int seed;
    [ShowIf("enableNoise")][Range(0,0.01f)]public float frequency = 1;
    [ShowIf("enableNoise")][Range(0,200)]public float amplitude = 1;
    [ShowIf("enableNoise")][Range(0,100)]public float persistence = 1;
    [ShowIf("enableNoise")][Range(0,25)]public int octave = 1;

    [Header("VertexColor")] 
    public List<VertexColor> VertexColors;
    public Gradient gradient;
}

[System.Serializable]
public class VertexColor
{
    public Color color;
    public float dist;
}
