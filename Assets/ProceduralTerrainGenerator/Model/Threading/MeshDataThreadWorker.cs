using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ProceduralTerrainGenerator
{
	public class MeshDataThreadWorker
	{
		public readonly Queue< ThreadData<MeshData> > queue = new Queue< ThreadData<MeshData> >();

		private Action<MeshData> callback;

		private float heightMultiplier;
		private AnimationCurve heightCurve;

		public MeshDataThreadWorker ( float heightMultiplier, AnimationCurve heightCurve )
		{
			this.heightMultiplier = heightMultiplier;
			this.heightCurve = heightCurve;
		}

		public void StartThread( Action<MeshData> callback, MapData mapData, int lod )
		{
			Thread t = new Thread( () => doWork( callback, mapData, heightMultiplier, heightCurve, lod ) );
			t.Start();
		}

		private void doWork( Action<MeshData> callback, MapData mapData, float heightMultiplier, AnimationCurve heightCurve, int lod )
		{
			MeshData meshData = MeshGenerator.GenerateTerrainMesh( mapData.HeightMap, heightMultiplier, heightCurve, lod );
			lock( queue )
			{
				queue.Enqueue( new ThreadData<MeshData>( callback, meshData ) );
			}
		}

		public void Call()
		{
			if( queue.Count > 0 )
			{
				for( int i = 0; i < queue.Count; i++ )
				{
					ThreadData<MeshData> threadData = queue.Dequeue();
					threadData.callback( threadData.parameter );
				}
			}
		}
	}
}

