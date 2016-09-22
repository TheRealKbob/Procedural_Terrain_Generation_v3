using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	public struct MapData
	{
		public float[,] HeightMap;
		public Color[] ColorMap;

		public MapData (float[,] heightMap, Color[] colorMap)
		{
			this.HeightMap = heightMap;
			this.ColorMap = colorMap;
		}
	}
}