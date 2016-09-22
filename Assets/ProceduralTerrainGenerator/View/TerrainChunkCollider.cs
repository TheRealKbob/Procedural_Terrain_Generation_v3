using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	public class TerrainChunkCollider : TerrainChunk
	{
		public TerrainChunkCollider ( float scale, Transform parent ) : base( scale, parent )
		{
		}

		public override void UpdateChunk()
		{
			base.UpdateChunk();
			gameObject.name = "Collider Chunk";
		}
	}
}

