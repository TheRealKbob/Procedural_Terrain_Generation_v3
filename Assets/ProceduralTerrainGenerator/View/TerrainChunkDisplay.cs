using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	public class TerrainChunkDisplay : TerrainChunk
	{

		private MeshRenderer mRenderer;
		private MeshFilter mFilter;

		public TerrainChunkDisplay ( float scale, Transform parent ) : base( scale, parent )
		{
			mRenderer = gameObject.AddComponent<MeshRenderer>();
			mFilter = gameObject.AddComponent<MeshFilter>();
		}

		public override void UpdateChunk( Vector3 position, Mesh mesh, Texture2D texture )
		{
			base.UpdateChunk();
			gameObject.transform.position = position * worldScale;
			gameObject.name = "Display Chunk";
			mRenderer.material.mainTexture = texture;
			mRenderer.material.color = Color.white;
			mFilter.mesh = mesh;
		}
	}
}

