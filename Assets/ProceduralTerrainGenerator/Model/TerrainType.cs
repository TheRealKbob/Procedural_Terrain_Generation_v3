using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	[System.Serializable]
	public struct TerrainType
	{
		public string name;
		public float height;
		public Color color;
	}
}