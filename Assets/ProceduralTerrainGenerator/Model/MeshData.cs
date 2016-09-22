using UnityEngine;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	public class MeshData
	{

		private Vector3[] Vertices;
		private int[] Triangles;
		private Vector2[] UVs;

		private Vector3[] bakedNormals;

		private Vector3[] borderVertices;
		private int[] borderTris;

		private int triIndex;
		private int borderTriIndex;

		public MeshData( int verticesPerLine )
		{
			Vertices = new Vector3[ verticesPerLine * verticesPerLine ];
			Triangles = new int[ ( verticesPerLine - 1 ) * ( verticesPerLine - 1 ) * 6 ]; 
			UVs = new Vector2[ verticesPerLine * verticesPerLine ];

			borderVertices = new Vector3[ verticesPerLine * 4 + 4 ];
			borderTris = new int[ 24 * verticesPerLine ];
		}

		public void AddVertex( Vector3 vertexPosition, Vector2 vertexUV, int vertexIndex )
		{
			if( vertexIndex < 0 )
			{
				borderVertices[ -vertexIndex - 1 ] = vertexPosition;
			}
			else
			{
				Vertices[ vertexIndex ] = vertexPosition;
				UVs[ vertexIndex ] = vertexUV;
			}
		}

		public void AddTriangle( int a, int b, int c )
		{

			if( a < 0 || b < 0 || c < 0 )
			{
				borderTris[ borderTriIndex ] = a;
				borderTris[ borderTriIndex + 1 ] = b; 
				borderTris[ borderTriIndex + 2 ] = c;
				borderTriIndex += 3;
			}
			else
			{
				Triangles[ triIndex ] = a;
				Triangles[ triIndex + 1 ] = b; 
				Triangles[ triIndex + 2 ] = c;
				triIndex += 3;	
			}
		}

		public Mesh CreateMesh(){
			Mesh m = new Mesh();
			m.name = "Terrain Chunk Mesh";
			m.vertices = Vertices;
			m.triangles = Triangles;
			m.uv = UVs;
			m.normals = bakedNormals;
			return m;
		}

		public void BakeNormals()
		{
			bakedNormals = calculateNormals();
		}

		private Vector3[] calculateNormals()
		{
			Vector3[] vertexNormals = new Vector3[ Vertices.Length ];

			int triCount = Triangles.Length / 3;
			for( int i = 0; i < triCount; i++ )
			{
				int triIndex = i * 3;
				int vertexIndexA = Triangles[ triIndex ];
				int vertexIndexB = Triangles[ triIndex + 1 ];
				int vertexIndexC = Triangles[ triIndex + 2 ];

				Vector3 triNormal = surfaceNormalFromIndices( vertexIndexA, vertexIndexB, vertexIndexC );
				vertexNormals[ vertexIndexA ] += triNormal;
				vertexNormals[ vertexIndexB ] += triNormal;
				vertexNormals[ vertexIndexC ] += triNormal;
			}

			int borderTriCount = borderTris.Length / 3;
			for( int i = 0; i < borderTriCount; i++ )
			{
				int triIndex = i * 3;
				int vertexIndexA = borderTris[ triIndex ];
				int vertexIndexB = borderTris[ triIndex + 1 ];
				int vertexIndexC = borderTris[ triIndex + 2 ];

				Vector3 triNormal = surfaceNormalFromIndices( vertexIndexA, vertexIndexB, vertexIndexC );

				if( vertexIndexA >= 0 )
				{
					vertexNormals[ vertexIndexA ] += triNormal;
				}
				if( vertexIndexB >= 0 )
				{
					vertexNormals[ vertexIndexB ] += triNormal;
				}
				if( vertexIndexC >= 0 )
				{
					vertexNormals[ vertexIndexC ] += triNormal;
				}
			}

			for( int i = 0; i < vertexNormals.Length; i++ )
			{
				vertexNormals[i].Normalize();
			}

			return vertexNormals;

		}

		private Vector3 surfaceNormalFromIndices( int indexA, int indexB, int indexC )
		{
			Vector3 pointA = ( indexA < 0 ) ? borderVertices[ -indexA - 1 ] : Vertices[ indexA ];
			Vector3 pointB = ( indexB < 0 ) ? borderVertices[ -indexB - 1 ] : Vertices[ indexB ];
			Vector3 pointC = ( indexC < 0 ) ? borderVertices[ -indexC - 1 ] : Vertices[ indexC ];

			Vector3 sideAB = pointB - pointA;
			Vector3 sideAC = pointC - pointA;

			return Vector3.Cross( sideAB, sideAC ).normalized;
		}

	}
}

