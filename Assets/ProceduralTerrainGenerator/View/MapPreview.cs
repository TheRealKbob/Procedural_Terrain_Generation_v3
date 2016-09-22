using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	public enum DrawMode
	{
		NOISE_MAP,
		COLOR_MAP,
		MESH
	}

	[RequireComponent( typeof( MeshRenderer ) )]
	[RequireComponent( typeof( MeshFilter ) )]
	public class MapPreview : MonoBehaviour 
	{

		private MeshRenderer mRenderer;
		private MeshFilter mFilter;

		void Start()
		{
			gameObject.SetActive( false );
		}

		public void DrawTexture( Texture2D texture )
		{
			mRenderer = GetComponent<MeshRenderer>();

			mRenderer.sharedMaterial.mainTexture = texture;
			mRenderer.transform.localScale = Vector3.one;//new Vector3( texture.width, 1, texture.height );
		}

		public void DrawMesh( MeshData meshData, Texture2D texture )
		{
			mRenderer = GetComponent<MeshRenderer>();
			mRenderer.transform.localScale = Vector3.one;
			mFilter = GetComponent<MeshFilter>();

			mFilter.sharedMesh = meshData.CreateMesh();
			mRenderer.sharedMaterial.mainTexture = texture;
		}
								
	}
}