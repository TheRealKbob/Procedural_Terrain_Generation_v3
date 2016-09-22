using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{

	public enum NormalizeMode
	{
		LOCAL,
		GLOBAL
	}

	public static class Noise {

		public static float[,] NoiseMap( int width, int height, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode )
		{
			float[,] noiseMap = new float[ width, height];

			float amplitude = 1;
			float frequency = 1;

			System.Random prng = new System.Random( seed );
			Vector2[] octaveOffsets = new Vector2[ octaves ];
			float maxPosibleHeight = 0;
			for( int i = 0; i < octaves; i++ )
			{
				float offsetX = prng.Next( -100000, 100000 ) + offset.x;
				float offsetY = prng.Next( -100000, 100000 ) - offset.y;
				octaveOffsets[i] = new Vector2( offsetX, offsetY );

				maxPosibleHeight += amplitude;
				amplitude *= persistance;
			}

			if( scale <= 0 ) scale = 0.0001f;

			float halfWidth = width * 0.5f;
			float halfHeight = height * 0.5f;

			float localMaxNoiseHeight = float.MinValue;
			float localMinNoiseHeight = float.MaxValue;

			for( int x = 0; x < width; x++ )
			{
				for( int y = 0; y < height; y++ )
				{
					amplitude = 1;
					frequency = 1;
					float noiseHeight = 0;
					
					for( int i = 0; i < octaves; i++ )
					{
						float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
						float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

						float perlinValue = Mathf.PerlinNoise( sampleX, sampleY ) * 2 - 1;
						noiseHeight += perlinValue * amplitude;

						amplitude *= persistance;
						frequency *= lacunarity;
					}

					if( noiseHeight > localMaxNoiseHeight ) localMaxNoiseHeight = noiseHeight;
					else if( noiseHeight < localMinNoiseHeight ) localMinNoiseHeight = noiseHeight;

					noiseMap[ x, y ] = noiseHeight;
				}
			}

			for( int x = 0; x < width; x++ )
			{
				for( int y = 0; y < height; y++ )
				{
					if( normalizeMode == NormalizeMode.LOCAL )
					{
						noiseMap[ x, y ] = Mathf.InverseLerp( localMinNoiseHeight, localMaxNoiseHeight, noiseMap[ x, y ] );
					}
					else
					{
						float normalizedHeight = ( noiseMap[ x, y ] + 1 ) / ( maxPosibleHeight / 0.9f );
						noiseMap[ x, y ] = Mathf.Clamp( normalizedHeight, 0, int.MaxValue );
					}
				}
			}

			return noiseMap;

		}
		
	}
}