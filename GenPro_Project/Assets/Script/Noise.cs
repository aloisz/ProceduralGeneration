using UnityEngine;

public static class Noise 
{
    public static float Noise3D(float x, float y, float z, float frequency, float amplitude, float persistence, int octave, int seed)
    {
        float noise = 0.0f;

        for (int i = 0; i < octave; ++i)
        {
            // Get all permutations of noise for each individual axis
            float noiseXY = Mathf.PerlinNoise(x * frequency + seed, y * frequency + seed) * amplitude;
            float noiseXZ = Mathf.PerlinNoise(x * frequency + seed, z * frequency + seed) * amplitude;
            float noiseYZ = Mathf.PerlinNoise(y * frequency + seed, z * frequency + seed) * amplitude;

            // Reverse of the permutations of noise for each individual axis
            /*float noiseYX = Mathf.PerlinNoise(y * frequency + seed, x * frequency + seed) * amplitude;
            float noiseZX = Mathf.PerlinNoise(z * frequency + seed, x * frequency + seed) * amplitude;
            float noiseZY = Mathf.PerlinNoise(z * frequency + seed, y * frequency + seed) * amplitude;*/

            // Use the average of the noise functions
            noise += (noiseXY + noiseXZ + noiseYZ ) / 3.0f; //+ noiseYX + noiseZX + noiseZY

            amplitude *= persistence;
            frequency *= 2.0f;
        }

        // Use the average of all octaves
        return noise / octave;
    }
}
