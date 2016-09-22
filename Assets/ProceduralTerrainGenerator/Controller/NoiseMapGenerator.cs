using UnityEngine;
using System;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	public class NoiseMapGenerator : MonoBehaviour {

		[Range(1f, 100)]
		public float scale = 1;

		[Range(0, 6)]
		public int previewSimplification = 0;

		[Range(0, 255)]
		public int seed = 0;

		[Range(1, 12)]
		public int octaves = 4;

		[Range(0, 1)]
		public float persistance = 0.5f;

		[Range(1, 20)]
		public float lacunarity = 2;

		public float heightMultiplier = 1;
		public AnimationCurve heightCurve;

		public Vector2 offset;

		public NormalizeMode normalizeMode;

		public TerrainType[] regions;

		public DrawMode drawMode = DrawMode.NOISE_MAP;

		private MapDataThreadWorker mapThreadWorker;
		private MeshDataThreadWorker meshThreadWorker;

		private static NoiseMapGenerator instance;
		public static NoiseMapGenerator Instance
		{
			get
			{
				return instance;
			}
		}

		public void DrawMapInEditor()
		{
			int chunkSize = 240 + 1 - 2;
			MapData mapData = GenerateMapData( chunkSize, Vector2.zero );

			MapPreview preview = FindObjectOfType<MapPreview>();
			if( preview != null )
			{
				switch( drawMode )
				{
					case DrawMode.NOISE_MAP:
					preview.DrawTexture( TextureGenerator.TextureFromHeightMap( mapData.HeightMap ) );
					break;

					case DrawMode.COLOR_MAP:
					preview.DrawTexture( TextureGenerator.TextureFromColorMap( mapData.ColorMap, chunkSize, chunkSize ) );
					break;

					case DrawMode.MESH:
					preview.DrawMesh( MeshGenerator.GenerateTerrainMesh( mapData.HeightMap, heightMultiplier, heightCurve, previewSimplification ), TextureGenerator.TextureFromColorMap( mapData.ColorMap, chunkSize, chunkSize )  );
					break;
				}
			}
		}

		public MapData GenerateMapData( int chunkSize, Vector2 origin )
		{
			float[,] noiseMap = Noise.NoiseMap( chunkSize + 2, chunkSize + 2, scale, seed, octaves, persistance, lacunarity, offset + origin, normalizeMode );

			Color[] colorMap = new Color[ chunkSize * chunkSize ];
			for (int y = 0; y < chunkSize; y++) {
				for (int x = 0; x < chunkSize; x++) {
					float currentHeight = noiseMap [x, y];
					for (int i = 0; i < regions.Length; i++) {
						if (currentHeight >= regions[i].height) {
							colorMap [y * chunkSize + x] = regions[i].color;
						}
						else
						{
							break;
						}
					}
				}
			}

			return new MapData( noiseMap, colorMap );
		}

		public void RequestMapData( Action<MapData> callback, int chunkSize, Vector2 origin )
		{
			if( mapThreadWorker == null ) mapThreadWorker = new MapDataThreadWorker( this );
			mapThreadWorker.StartThread( callback, chunkSize, origin );
		}

		public void RequestMeshData( Action<MeshData> callback, MapData mapData, int lod )
		{
			if( meshThreadWorker == null ) meshThreadWorker = new MeshDataThreadWorker( heightMultiplier, heightCurve );
			meshThreadWorker.StartThread( callback, mapData, lod );
		}

		void Awake()
		{
			instance = this;
		}

		void Update()
		{
			if( mapThreadWorker != null ) mapThreadWorker.Call();
			if( meshThreadWorker != null ) meshThreadWorker.Call();
		}
	}
}