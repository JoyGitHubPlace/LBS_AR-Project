using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using MiniJSON;

using GoShared;
using Mapbox.Utils;
using Mapbox.VectorTile;
using Mapbox.VectorTile.ExtensionMethods;
using Mapbox.VectorTile.Geometry;

namespace WaveMap
{
	[ExecuteInEditMode]
	public abstract class GOPBFTile : GOTile
	{

		public abstract GOFeature ParseFeatureData (VectorTileFeature feature, GOLayer layer);
		public abstract string GetLayersStrings (GOLayer layer);

		public override IEnumerator BuildTile(IDictionary mapData, GOLayer layer, bool delayedLoad)
		{ 
			yield return null;
		}

		public override IEnumerator ParseTileData(object m, Coordinates tilecenter, int zoom, GOLayer[] layers, bool delayedLoad, List <string> layerNames)
		{
            if (mapData == null) {
				Debug.LogWarning ("Map Data is null!");
				#if !UNITY_WEBPLAYER
				FileHandler.Remove (gameObject.name);
				#endif
				yield break;
			}

			var decompressed = Compression.Decompress((byte[])mapData);
			VectorTile vt = new VectorTile(decompressed,false);

			foreach (GOLayer layer in layers) {

				string[] lyrs = GetLayersStrings(layer).Split(',');

				foreach (string l in lyrs) {
					VectorTileLayer lyr = vt.GetLayer(l);
					if (lyr != null)
						yield return (StartCoroutine(BuildLayer(lyr,layer,delayedLoad)));
				}
			}
		}

		public IEnumerator BuildLayer(VectorTileLayer layerData, GOLayer layer, bool delayedLoad)
		{

			GameObject parent = null;
			if (transform.FindChild (layer.name) == null) {
				parent = new GameObject ();
				parent.name = layer.name;
				parent.transform.parent = this.transform;
				parent.SetActive (!layer.startInactive);
			} else {
				parent = transform.FindChild (layer.name).gameObject;
			}


			if (layerData.FeatureCount() == 0)
				yield break;


			List<GOFeature> stack = new List<GOFeature> ();

			for (int i = 0; i < layerData.FeatureCount(); i++) {

				//get the feature
				VectorTileFeature feature = layerData.GetFeature(i);
				GOFeature goFeature = ParseFeatureData (feature, layer);

				if (layer.useOnly.Length > 0 && !layer.useOnly.Contains (goFeature.kind)) {
					continue;
				}
				if (layer.avoid.Length > 0 && layer.avoid.Contains (goFeature.kind)) {
					continue;
				}

				if (layer.layerType == GOLayer.GOLayerType.Roads) {
					GORoadFeature grf = (GORoadFeature)goFeature;
					if ((grf.isBridge && !layer.useBridges) || (grf.isTunnel && !layer.useTunnels) || (grf.isLink && !layer.useBridges)) {
						continue;
					}
				}

				//multipart
				List<List<LatLng>> geomWgs84 = feature.GeometryAsWgs84((ulong)map.zoomLevel, (ulong)tileCoordinates.x, (ulong)tileCoordinates.y,0);

				string type = feature.GetFeatureType(geomWgs84);
				if (type == null || feature.GeometryType == GeomType.POINT) {
					continue;
				}
				if (feature.GeometryType == GeomType.POLYGON && layer.layerType == GOLayer.GOLayerType.Roads)
					continue;

				if (type == "MultiLineString" || (type == "Polygon" && !layer.isPolygon)) {

					foreach (IList geometry in geomWgs84) {
						GOFeature gf = ParseFeatureData (feature, layer);
						gf.geometry = geometry;
						gf.type = type;					
						gf.layer = layer;
						gf.parent = parent;
						gf.properties = feature.GetProperties ();
						gf.ConvertGeometries ();
						gf.ConvertAttributes ();
						gf.index = (Int64)i;
						gf.goFeatureType = GOFeature.GOFeatureType.MultiLine;
						stack.Add(gf);
					}
				} 			
				else if (type == "LineString") {
					GOFeature gf = ParseFeatureData (feature, layer);
					gf.geometry = geomWgs84 [0];
					gf.type = type;
					gf.layer = layer;
					gf.parent = parent;
					gf.properties = feature.GetProperties ();
					gf.ConvertGeometries ();
					gf.ConvertAttributes ();
					gf.index = (Int64)i;
					gf.goFeatureType = GOFeature.GOFeatureType.Line;
					if (geomWgs84.Count == 0)
						continue;

					stack.Add(gf);
				} 

				else if (type == "Polygon") {

					GOFeature gf = ParseFeatureData (feature, layer);
					gf.geometry = geomWgs84 [0];
					gf.type = type;
					gf.layer = layer;
					gf.parent = parent;
					gf.properties = feature.GetProperties ();
					gf.ConvertGeometries ();
					gf.ConvertAttributes ();
					gf.index = (Int64)i;
					gf.goFeatureType = GOFeature.GOFeatureType.Polygon;

					stack.Add(gf);	
				}

				else if (type == "MultiPolygon") {

					GameObject multi = new GameObject ("MultiPolygon");
					multi.transform.parent = parent.transform;

					IList subject = null;
					IList clips = new List<List<LatLng>>();

					for (int j = 0; j<geomWgs84.Count; j++) { //Clip ascending

						IList p = geomWgs84 [j];
						if (GOFeature.IsGeoPolygonClockwise (p)) {
							subject = p;
						} 
						else {
							//Add clip
							clips.Add (p);
						}
						//Last one
						if (j == geomWgs84.Count - 1 || (j<geomWgs84.Count-1 && GOFeature.IsGeoPolygonClockwise (geomWgs84 [j + 1]) && subject != null)) {

							GOFeature gf = ParseFeatureData (feature, layer);
							gf.geometry = subject;
							gf.clips = clips;
							gf.type = type;
							gf.layer = layer;
							gf.parent = parent;
							gf.properties = feature.GetProperties ();
							gf.ConvertGeometries ();
							gf.ConvertAttributes ();
							gf.index = (Int64)i;
							gf.goFeatureType = GOFeature.GOFeatureType.MultiPolygon;

							stack.Add (gf);

							subject = null;
							clips = new List<List<LatLng>>();
						}
					}
				}
			}

			IList iStack = (IList)stack;
			if (layer.layerType == GOLayer.GOLayerType.Roads) {
				iStack = GORoadFeature.MergeRoads (iStack);
			}

			int n = 25;
			for (int i = 0; i < iStack.Count; i+=n) {

				for (int k = 0; k<n; k++) {
					if (i + k >= iStack.Count) {
						yield return null;
						break;
					}

					GOFeature r = (GOFeature)iStack [i + k];
					IEnumerator routine = r.BuildFeature (this, delayedLoad);
					if (routine != null) {
                        if (Application.isPlaying) 
                        StartCoroutine (routine);
                        else
                            GORoutine.start(routine, this);
					}
				}
                yield return new WaitForSeconds(0.5f);
            }

			Resources.UnloadUnusedAssets ();

			yield return null;
		}


	}
}
