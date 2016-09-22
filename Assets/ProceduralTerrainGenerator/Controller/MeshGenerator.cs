using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	public static class MeshGenerator
	{

		public static MeshData GenerateTerrainMesh( float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail )
		{
			AnimationCurve curve = new AnimationCurve( heightCurve.keys );

			int simplificationIncrement = ( levelOfDetail == 0 ) ? 1 : levelOfDetail * 2;

			int borderedSize = heightMap.GetLength(0);
			int meshSize = borderedSize - 2 * simplificationIncrement;
			int unsimplifiedMeshSize = borderedSize - 2;

			float topLeftX = (unsimplifiedMeshSize - 1) / -2f;
			float topLeftZ = (unsimplifiedMeshSize - 1) / 2f;

			int vertsPerLine = ( meshSize - 1 ) / simplificationIncrement + 1;

			MeshData meshData = new MeshData( vertsPerLine );

			int[,] vertexIndicesMap = new int[ borderedSize, borderedSize ];
			int meshVertexIndex = 0;
			int borderVertexIndex = -1;

			for( int x = 0; x < borderedSize; x += simplificationIncrement )
			{
				for( int y = 0; y < borderedSize; y += simplificationIncrement )
				{
					bool isBorderIndex = x == 0 || x == borderedSize - 1 || y == 0 || y == borderedSize - 1;

					if( isBorderIndex )
					{
						vertexIndicesMap[ x, y ] = borderVertexIndex;
						borderVertexIndex--;
					}
					else
					{
						vertexIndicesMap[ x, y ] = meshVertexIndex;
						meshVertexIndex++;
					}
				}
			}

			for( int x = 0; x < borderedSize; x += simplificationIncrement )
			{
				for( int y = 0; y < borderedSize; y += simplificationIncrement )
				{
					int vertexIndex = vertexIndicesMap[ x, y ];
					Vector2 percent = new Vector2( ( x - simplificationIncrement ) / (float)meshSize, ( y - simplificationIncrement ) / (float)meshSize );
					float height = curve.Evaluate( heightMap[ x, y ] ) * heightMultiplier;
					Vector3 vertexPosition = new Vector3( topLeftX + percent.x * unsimplifiedMeshSize, height, topLeftZ - percent.y * unsimplifiedMeshSize );

					meshData.AddVertex( vertexPosition, percent, vertexIndex );

					if( x < borderedSize - 1 && y < borderedSize - 1 )
					{
						int a = vertexIndicesMap[ x, y ];
						int b = vertexIndicesMap[ x + simplificationIncrement, y ];
						int c = vertexIndicesMap[ x, y + simplificationIncrement ];
						int d = vertexIndicesMap[ x + simplificationIncrement, y + simplificationIncrement ];

						meshData.AddTriangle( a, d, c );
						meshData.AddTriangle( d, a, b );
					}

					vertexIndex++;
				}
			}

			meshData.BakeNormals();

			return meshData;

		}

	}
}

