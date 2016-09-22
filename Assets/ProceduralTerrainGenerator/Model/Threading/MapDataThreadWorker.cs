using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ProceduralTerrainGenerator
{
	public class MapDataThreadWorker
	{
		public readonly Queue< ThreadData<MapData> > queue = new Queue< ThreadData<MapData> >();

		private NoiseMapGenerator generator;

		public MapDataThreadWorker ( NoiseMapGenerator generator )
		{
			this.generator = generator;
		}

		public void StartThread( Action<MapData> callback, int chunkSize, Vector2 origin )
		{
			Thread t = new Thread( () => doWork( callback, chunkSize, origin ) );
			t.Start();
		}

		private void doWork( Action<MapData> callback, int chunkSize, Vector2 origin )
		{
			MapData mapData = generator.GenerateMapData( chunkSize, origin );
			lock( queue )
			{
				queue.Enqueue( new ThreadData<MapData>( callback, mapData ) );
			}
		}

		public void Call()
		{
			if( queue.Count > 0 )
			{
				for( int i = 0; i < queue.Count; i++ )
				{
					ThreadData<MapData> threadData = queue.Dequeue();
					threadData.callback( threadData.parameter );
				}
			}
		}
	}
}

