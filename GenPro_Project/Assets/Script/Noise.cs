using UnityEngine;

public static class Noise 
{
    public static float Noise3D(Vector3 position, float frequency, int subdivision, float amplitude, float persistence, int octave, int seed)
    {
        float noise = 0.0f;

        for (int i = 0; i < octave; ++i)
        {
            // Get all permutations of noise for each individual axis
            float noiseXY = Mathf.PerlinNoise(position.x * frequency + seed, position.y * (frequency / subdivision) + seed) * amplitude;
            float noiseXZ = Mathf.PerlinNoise(position.x * frequency + seed, position.z * (frequency / subdivision) + seed) * amplitude;
            float noiseYZ = Mathf.PerlinNoise(position.y * frequency + seed, position.z * (frequency / subdivision) + seed) * amplitude;

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
    
    private static readonly int[] Permutation = {
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7,
        225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247,
        120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
        88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134,
        139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220,
        105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80,
        73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
        164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38,
        147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189,
        28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101,
        155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232,
        178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12,
        191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181,
        199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236,
        205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
    };

    private static int SafeIndex(int index)
    {
        return Mathf.Abs(index) % 256; // Ensure index is within bounds
    }

    private static int Hash(int x, int y, int z)
    {
        x = SafeIndex(x);
        y = SafeIndex(y);
        z = SafeIndex(z);

        return Permutation[(Permutation[(Permutation[x] + y) % 256] + z) % 256];
    }

    private static float Gradient(int hash, float x, float y, float z)
    {
        // Use the lower 4 bits of the hash to choose a gradient direction
        int h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    public static float Generate3DNoise(Vector3 position, float noiseScale, float frequency, float amplitude, float persistence, int octaves, int seed)
    {
        float total = 0;
        float maxAmplitude = 0;
        float currentFrequency = frequency;
        float currentAmplitude = amplitude;

        for (int i = 0; i < octaves; i++)
        {
            float noiseValue = Perlin3D(position.x * currentFrequency + seed, position.y * currentFrequency + seed, position.z * currentFrequency + seed);
            total += noiseValue * currentAmplitude;

            maxAmplitude += currentAmplitude;
            currentAmplitude *= persistence;
            currentFrequency *= 2.0f;
        }

        return total / maxAmplitude * noiseScale;
    }

    private static float Perlin3D(float x, float y, float z)
    {
        // Find unit cube that contains the point
        int X = Mathf.FloorToInt(x) & 255;
        int Y = Mathf.FloorToInt(y) & 255;
        int Z = Mathf.FloorToInt(z) & 255;

        // Find relative position in the unit cube
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);

        // Compute fade curves for each axis
        float u = Fade(x);
        float v = Fade(y);
        float w = Fade(z);

        // Hash the corners of the cube
        int aaa = Hash(X, Y, Z);
        int aba = Hash(X, Y + 1, Z);
        int aab = Hash(X, Y, Z + 1);
        int abb = Hash(X, Y + 1, Z + 1);
        int baa = Hash(X + 1, Y, Z);
        int bba = Hash(X + 1, Y + 1, Z);
        int bab = Hash(X + 1, Y, Z + 1);
        int bbb = Hash(X + 1, Y + 1, Z + 1);

        // Add blended results from the corners
        float x1, x2, y1, y2;
        x1 = Lerp(Gradient(aaa, x, y, z), Gradient(baa, x - 1, y, z), u);
        x2 = Lerp(Gradient(aba, x, y - 1, z), Gradient(bba, x - 1, y - 1, z), u);
        y1 = Lerp(x1, x2, v);

        x1 = Lerp(Gradient(aab, x, y, z - 1), Gradient(bab, x - 1, y, z - 1), u);
        x2 = Lerp(Gradient(abb, x, y - 1, z - 1), Gradient(bbb, x - 1, y - 1, z - 1), u);
        y2 = Lerp(x1, x2, v);

        return Lerp(y1, y2, w);
    }
}