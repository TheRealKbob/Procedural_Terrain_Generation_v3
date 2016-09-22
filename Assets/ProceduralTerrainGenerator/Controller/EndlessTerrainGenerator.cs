using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralTerrainGenerator
{

	public class EndlessTerrainGenerator : MonoBehaviour
	{

		public enum TerrainChunkCategory
		{
			DISPLAY,
			COLLIDER
		}		

		public float MAX_VIEW_DISTANCE{ get; private set; }
		public const float DISTANCE_UPDATE_THRESHOLD = 25;

		public Transform viewer;
		public static Vector2 viewerPosition;
		private Vector2 previousViewerPosition;

		[ Range( 1, 241 ) ]
		public int chunkSize = 240;
		public static int requestChunkSize;

		[ Range( 1, 5 ) ]
		public int scale = 1;
		public static int chunkScale;

		public LevelOfDetailData[] detailLevels;

		public TerrainChunkCategory terrainChunkCategory = TerrainChunkCategory.DISPLAY;
		public static TerrainChunkCategory category;

		private int chunkRange;

		public static Dictionary< Vector2, ChunkData > terrainChunkDictionary = new Dictionary<Vector2, ChunkData>();
		public List<ChunkData> chunksVisibleLastFrame = new List<ChunkData>();
		public List<TerrainChunk> terrainChunkQueue = new List<TerrainChunk>();

		void Start()
		{
			MAX_VIEW_DISTANCE = detailLevels[ detailLevels.Length - 1 ].distanceThreshhold;
			chunkRange = Mathf.RoundToInt( MAX_VIEW_DISTANCE / chunkSize );
			updateVisibleChunks();
		}

		void Update()
		{
			viewerPosition = new Vector2( viewer.position.x, viewer.position.z ) / scale;
			if( ( previousViewerPosition - viewerPosition ).sqrMagnitude > (DISTANCE_UPDATE_THRESHOLD * DISTANCE_UPDATE_THRESHOLD) )
			{
				updateVisibleChunks();
				previousViewerPosition = viewerPosition;
			}
		}

		void OnValidate()
		{
			chunkSize = MathUtilities.RoundToFactor( chunkSize, 8 );
			requestChunkSize = chunkSize + 1;
			chunkScale = scale;
			category = terrainChunkCategory;
		}

		private void updateVisibleChunks()
		{
			Vector2 currentChunkCoord = new Vector2(Mathf.RoundToInt( viewerPosition.x / chunkSize ), 
													Mathf.RoundToInt( viewerPosition.y / chunkSize ) );

			
			for( int i = 0; i < chunksVisibleLastFrame.Count; i++ )
			{
				chunksVisibleLastFrame[i].SetVisible( false );
			}
			chunksVisibleLastFrame.Clear();

			for( int x = -chunkRange; x <= chunkRange; x++ )
			{
				for( int y = -chunkRange; y <= chunkRange; y++ )
				{
					Vector2 viewedChunkCoord = new Vector2( currentChunkCoord.x + x, currentChunkCoord.y + y );

					if( terrainChunkDictionary.ContainsKey( viewedChunkCoord ) )
					{
						terrainChunkDictionary[ viewedChunkCoord ].Update();
					}
					else
					{
						ChunkData cData = new ChunkData( viewedChunkCoord, chunkSize, detailLevels, this );
						terrainChunkDictionary.Add( viewedChunkCoord, cData );
					}
				}
			}
		}
	}

	public class ChunkData
	{

		private Vector2 position;
		private Vector3 worldPosition;
		private Bounds bounds;

		private NoiseMapGenerator mapGenerator;
		private EndlessTerrainGenerator terrainGenerator;

		private LevelOfDetailData[] detailLevels;
		private LevelOfDetailMesh[] detailMeshes;
		private LevelOfDetailMesh currentMesh;

		private TerrainChunk chunkObject;

		private Texture2D texture;

		private Transform parent;

		private MapData mapData;
		public bool mapDataRecieved{ get; private set; }

		private int previousLODIndex = -1;

		public ChunkData( Vector2 coordinate, int size, LevelOfDetailData[] detailLevels, EndlessTerrainGenerator terrainGenerator )
		{
			position = coordinate * size;
			worldPosition = new Vector3( position.x, 0, position.y );
			bounds = new Bounds( position, Vector2.one * size );

			mapGenerator = NoiseMapGenerator.Instance;
			this.terrainGenerator = terrainGenerator;

			this.parent = terrainGenerator.transform;

			this.detailLevels = detailLevels;
			detailMeshes = new LevelOfDetailMesh[ detailLevels.Length ];
			for( int i = 0; i < detailLevels.Length; i++ )
			{
				detailMeshes[i] = new LevelOfDetailMesh( detailLevels[i].simplification, Update );
			}

			SetVisible( false );

			mapGenerator.RequestMapData( onRecievedMapData, EndlessTerrainGenerator.requestChunkSize, position );
		}

		public void Update()
		{
			if( mapDataRecieved )
			{
				float viewDistanceFromEdge = Mathf.Sqrt( bounds.SqrDistance( EndlessTerrainGenerator.viewerPosition ) );
				bool visible = viewDistanceFromEdge <= terrainGenerator.MAX_VIEW_DISTANCE;

				if( visible )
				{
					int lodIndex = 0;
					for( int i = 0; i < detailLevels.Length - 1; i++ )
					{
						if( viewDistanceFromEdge > detailLevels[i].distanceThreshhold )
							lodIndex = i + 1;
						else
							break;
					}

					if( lodIndex != previousLODIndex )
					{
						LevelOfDetailMesh lodMesh = detailMeshes[ lodIndex ];
						if( lodMesh.meshLoaded )
						{
							currentMesh = lodMesh;
							previousLODIndex = lodIndex;
						}
						else if( !lodMesh.requestedMesh )
						{
							lodMesh.RequestMesh( mapData );
						}
					}

					terrainGenerator.chunksVisibleLastFrame.Add( this );
				}

				SetVisible( visible );
			}
		}

		public void SetVisible( bool visible )
		{
			if( visible )
			{
				enable();
			}
			else
			{
				disable();
			}
		}

		private void enable()
		{
			if( currentMesh == null || !currentMesh.meshLoaded ) return;

			if( chunkObject == null )
			{
				chunkObject = terrainGenerator.terrainChunkQueue.FirstOrDefault( c => c.inUse == false );
				if( chunkObject == null )
				{
					chunkObject = new TerrainChunkDisplay( terrainGenerator.scale, parent );
					terrainGenerator.terrainChunkQueue.Add( chunkObject );
					parent.gameObject.name = "Endless Terrain (" + terrainGenerator.terrainChunkQueue.Count + ")";
				}
			}
			chunkObject.UpdateChunk( worldPosition, currentMesh.mesh, texture );

			chunkObject.SetActive( true );
		}

		private void disable()
		{
			if( chunkObject != null )
			{
				chunkObject.SetActive( false );
				chunkObject = null;
			}
		}

		private void onRecievedMapData( MapData mapData )
		{
			mapDataRecieved = true;
			this.mapData = mapData;

			texture = TextureGenerator.TextureFromColorMap( mapData.ColorMap, EndlessTerrainGenerator.requestChunkSize, EndlessTerrainGenerator.requestChunkSize );

			Update();
		}

	}

	public class LevelOfDetailMesh
	{
		public Mesh mesh;
		public bool requestedMesh;
		public bool meshLoaded;
		public int levelOfDetail;

		private NoiseMapGenerator mapGenerator;

		private System.Action updateCallback;

		public LevelOfDetailMesh ( int levelOfDetail, System.Action updateCallback )
		{
			this.levelOfDetail = levelOfDetail;
			this.updateCallback = updateCallback;

			mapGenerator = NoiseMapGenerator.Instance;
		}

		public void RequestMesh( MapData mapData )
		{
			requestedMesh = true;
			mapGenerator.RequestMeshData( onMeshDataRecieved, mapData, levelOfDetail );
		}

		private void onMeshDataRecieved( MeshData meshData )
		{
			mesh = meshData.CreateMesh();
			meshLoaded = true;

			updateCallback();
		}
	}

	[System.Serializable]
	public struct LevelOfDetailData
	{
		public int simplification;
		public float distanceThreshhold;
	}
}

