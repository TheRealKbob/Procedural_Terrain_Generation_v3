using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProceduralTerrainGenerator
{
	[ CustomEditor( typeof( NoiseMapGenerator ) ) ]
	public class NoiseMapGeneratorEditor : Editor {

		public override void OnInspectorGUI()
		{
			NoiseMapGenerator generator = (NoiseMapGenerator)target;

			if( DrawDefaultInspector() || GUILayout.Button( "Generate" ) )
			{
				generator.DrawMapInEditor();
			}
		}
	}
}