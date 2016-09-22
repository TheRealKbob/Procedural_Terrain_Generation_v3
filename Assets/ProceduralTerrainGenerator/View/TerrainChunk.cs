using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	public class TerrainChunk
	{

		public GameObject gameObject{ get; protected set; }

		public bool inUse{ get; protected set; }

		protected float worldScale;

		public TerrainChunk ( float scale, Transform parent )
		{
			worldScale = scale;

			gameObject = new GameObject();
			gameObject.transform.localScale = Vector3.one * worldScale;
			gameObject.transform.parent = parent;
		}

		public virtual void UpdateChunk()
		{
			
		}

		public virtual void UpdateChunk( Vector3 position, Texture2D texture )
		{
			
		}

		public virtual void UpdateChunk( Vector3 position, Mesh mesh, Texture2D texture )
		{
			
		}

		public virtual void SetActive( bool val )
		{
			inUse = val;
			gameObject.SetActive( val );
			Debug.Log( val );
		}

	}
}

