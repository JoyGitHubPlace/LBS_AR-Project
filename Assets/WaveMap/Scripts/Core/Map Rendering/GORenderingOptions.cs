using UnityEngine;
using GoShared;
using UnityEngine.Serialization;

namespace WaveMap
{

	[System.Serializable]
	public class GORenderingOptions
	{

		public GOFeatureKind kind;
		public Material material;
		public Material outlineMaterial;
		public Material roofMaterial;

		public Material[] materials;

		public float lineWidth;
		public float outlineWidth;
		public bool useStreetNames;
		public float polygonHeight;

		public string tag;

	}

}