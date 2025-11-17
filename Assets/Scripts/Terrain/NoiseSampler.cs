using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public static class NoiseSampler
{   
    public static (float min, float max) SampleNoiseRange(
        FastNoise noise,
        int sampleWidth,
        float frequency,
        float amplitude,
        float scale,
        float lacunarity,
        float persistance,
        float octaves)
    {
        float maxNoiseValue = float.MinValue;
        float minNoiseValue = float.MaxValue;

        for(int x = 0; x < sampleWidth; x++)
        for(int z = 0; z < sampleWidth; z++)
        {
            float amp = amplitude;
            float freq = frequency;
            float noiseHeight = 0;

            for(int i = 0; i < octaves; i++)
            {
                float noiseValue = (noise.GetSimplex(x / scale * freq, z / scale * freq));

                noiseHeight += noiseValue * amp;
                freq *= lacunarity;
                amp *= persistance;
            }

            if(maxNoiseValue < noiseHeight) maxNoiseValue = noiseHeight;
            if(minNoiseValue > noiseHeight) minNoiseValue = noiseHeight;
        }
        return (minNoiseValue , maxNoiseValue);
    }
}