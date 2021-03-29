using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using MiniJSON;

using GoShared;

namespace WaveMap
{
	[ExecuteInEditMode]
	public abstract class GOTile : MonoBehaviour
	{
		public Coordinates tileCenter;
		public Vector2 tileCoordinates;
		//public string tileUrl;
        public int seed;
		[HideInInspector] public List<Vector3> vertices;
		[HideInInspector] public object mapData;
		[HideInInspector] public GOMap map;

		ParseJob job;
		IList buildingsIds = new List<object>();

		bool TileLoaded = false;

		public abstract IEnumerator BuildTile (IDictionary mapData, GOLayer layer, bool delayedLoad);
		public abstract IEnumerator ParseTileData (object m, Coordinates tilecenter, int zoom, GOLayer[] layers, bool delayedLoad, List <string> layerNames);
		public abstract string GetTileUrl ();


		#region BUILDINGS

		private Dictionary < Vector3,GameObject> buildingCenters = new Dictionary<Vector3, GameObject>();
		private float mdc = 60*Global.tilesizeRank; // Group buildings every 50meters
		public GameObject findNearestCenter (Vector3 center, GameObject parent, Material material) {
			foreach (var c in buildingCenters) {
				float d = Mathf.Abs(Vector3.Distance (center, c.Key));
				if (d <= mdc) {
                    return c.Value;
				}
			}
            string name = "Container " + center.x + " " + center.z;
            GameObject container = new GameObject (name);
			container.transform.parent = parent.transform;
			container.AddComponent<GOMatHolder> ().material = material;
            buildingCenters.Add (center, container);
			return container;
		}

		public Material GetMaterial (GORenderingOptions rendering, Vector3 center) {
		
			if (rendering.materials.Length > 0) {
				seed = (center.x +"/"+ center.z).GetHashCode();
                UnityEngine.Random.InitState(seed);

				int pick = UnityEngine.Random.Range(0, rendering.materials.Length);
				Material material = rendering.materials [pick];
				return material;
			} else
				return rendering.material;
		}

		void OnDestroy() {
			removeIds ();
		}

		public bool idCheck (object id, GOLayer layer) {
			if (map.buildingsIds.Contains (id)) {
				return false;
			} else {
				buildingsIds.Add(id);
				map.buildingsIds.Add(id);
				return true;
			}
		}

		private void removeIds () {
			foreach (object id in buildingsIds) {
				map.buildingsIds.Remove (id);
			}
		}

		#endregion

		public void AddObjectToLayerMask (GOLayer layer, GameObject obj) {
			LayerMask mask = LayerMask.NameToLayer (layer.name);
			if (mask.value > 0 && mask.value < 31) {
				obj.layer = LayerMask.NameToLayer (layer.name);
			} else {
				Debug.LogWarning ("[GOMap] Please create layer masks before running GoMap. A layer mask must have the same name declared in GoMap inspector, for example \""+layer.name+"\".");
			}
		}

		#region Network

		public IEnumerator LoadTileData(GOMap m, Coordinates tilecenter, int zoom, GOLayer[] layers, bool delayedLoad) {

			map = m;

			tileCoordinates = tileCenter.tileCoordinates (map.zoomLevel);

			if (Application.isPlaying) {
				yield return StartCoroutine (DownloadData (m, tileCenter, zoom, layers, delayedLoad));
				List <string> layerNames = map.layerNames();
				yield return StartCoroutine(ParseTileData(map,tileCenter,zoom,layers,delayedLoad,layerNames));
			} 
			else {
				GORoutine.start (DownloadData (m, tileCenter, zoom, layers, delayedLoad), this);
			}
		}

		#if UNITY_EDITOR
		public void Update () {

			if (Application.isPlaying || TileLoaded || !map)
				return;
			
			if(mapData!=null)
			{
				TileLoaded = true;
				GORoutine.start (ParseTileData(map,tileCenter,map.locationManager.zoomLevel,map.layers,false,map.layerNames()),this);
			}
		}
		#endif


		public virtual IEnumerator DownloadData(object m, Coordinates tilecenter, int zoom, GOLayer[] layers, bool delayedLoad) {

			string completeUrl = GetTileUrl();

			string filename = "[Mapzen]" + gameObject.name;
			return GOUrlRequest.getRequest (this, completeUrl, map.useCache, filename, (byte[] bytes, string text, string error) => {

				if (error == null) {
					mapData = bytes;

					#if UNITY_EDITOR
					if (!Application.isPlaying)
						Update();
					#endif
				} 
			});

		}


		public IEnumerator ParseJson (string data) {

			job = new ParseJob();
			job.InData = data;
			job.Start();

			yield return StartCoroutine(job.WaitFor());
		}

		#endregion

	}
}
