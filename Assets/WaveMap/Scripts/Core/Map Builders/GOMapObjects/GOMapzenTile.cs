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
using Mapbox.VectorTile.ExtensionMethods;

namespace WaveMap
{
	[ExecuteInEditMode]
	public class GOMapzenTile : GOTile
	{
		
		public override IEnumerator BuildTile(IDictionary mapData, GOLayer layer, bool delayedLoad)
		{ 

			GameObject parent = new GameObject ();
			parent.name = layer.name;
			parent.transform.parent = this.transform;
			parent.SetActive (!layer.startInactive);

			if (mapData == null) {
				Debug.LogWarning ("Map Data is null!");
				#if !UNITY_WEBPLAYER
				FileHandler.Remove (gameObject.name);
				#endif
				yield break;
			}

			IList features = (IList)mapData ["features"];

			if (features == null)
				yield break;

			IList stack = new List<GOFeature> ();

			foreach (IDictionary geo in features) {

				IDictionary geometry = (IDictionary)geo ["geometry"];
				IDictionary properties = (IDictionary)geo ["properties"];
				string type = (string)geometry ["type"];

				string kind = (string)properties ["kind"];
				if (properties.Contains("kind_detail")) {
					kind = (string)properties ["kind_detail"];
				}

				var id = properties ["id"]; 
				if (idCheck (id,layer) == false && layer.layerType == GOLayer.GOLayerType.Buildings) {
					continue;
				}

				if (layer.useOnly.Length > 0 && !layer.useOnly.Contains (GOEnumUtils.MapzenToKind(kind))) {
					continue;
				}
				if (layer.avoid.Length > 0 && layer.avoid.Contains (GOEnumUtils.MapzenToKind(kind))) {
					continue;
				}

				if (type == "MultiLineString" || (type == "Polygon" && !layer.isPolygon)) {
					IList lines = new List<object>();
					lines = (IList)geometry ["coordinates"];
					foreach (IList coordinates in lines) {
						GOFeature gf = ParseFeatureData (properties, layer);
						gf.geometry = coordinates;
						gf.type = type;					
						gf.layer = layer;
						gf.parent = parent;
						gf.properties = properties;
						gf.ConvertGeometries ();
						gf.ConvertAttributes ();
						gf.index = (Int64)features.IndexOf(geo);
						gf.goFeatureType = GOFeature.GOFeatureType.MultiLine;
						stack.Add(gf);
					}
				} 

				else if (type == "LineString") {
					IList coordinates = (IList)geometry ["coordinates"];
					GOFeature gf = ParseFeatureData (properties, layer);
					gf.geometry = coordinates;
					gf.type = type;					
					gf.layer = layer;
					gf.parent = parent;
					gf.properties = properties;
					gf.ConvertGeometries ();
					gf.ConvertAttributes ();
					gf.index = (Int64)features.IndexOf(geo);
					gf.goFeatureType = GOFeature.GOFeatureType.Line;
					stack.Add(gf);
				} 

				else if (type == "Polygon") {

					List <object> shapes = new List<object>();
					shapes = (List <object>)geometry["coordinates"];

					IList subject = null;
					List<object> clips = null;
					if (shapes.Count == 1) {
						subject = (List<object>)shapes[0];
					} else if (shapes.Count > 1) {
						subject = (List<object>)shapes[0];
						clips = shapes.GetRange (1, shapes.Count - 1);
					} else {
						continue;
					}

					GOFeature gf = ParseFeatureData (properties, layer);
					gf.geometry = subject;
					gf.clips = clips;
					gf.type = type;
					gf.layer = layer;
					gf.parent = parent;
					gf.properties = properties;
					gf.ConvertGeometries ();
					gf.ConvertAttributes ();
					gf.index = (Int64)features.IndexOf(geo);
					gf.goFeatureType = GOFeature.GOFeatureType.Polygon;
					stack.Add (gf);
				}

				if (type == "MultiPolygon") {

					GameObject multi = new GameObject ("MultiPolygon");
					multi.transform.parent = parent.transform;

					IList shapes = new List<object>();
					shapes = (IList)geometry["coordinates"];

					foreach (List<object> polygon in shapes) {

						IList subject = null;
						List<object> clips = null;
						if (polygon.Count > 0) {
							subject = (List<object>)polygon[0];
						} else if (polygon.Count > 1) {
							clips = polygon.GetRange (1, polygon.Count - 1);
						} else {
							continue;
						}

						GOFeature gf = ParseFeatureData (properties, layer);
						gf.geometry = subject;
						gf.clips = clips;
						gf.type = type;
						gf.layer = layer;
						gf.parent = parent;
						gf.properties = properties;
						gf.ConvertGeometries ();
						gf.ConvertAttributes ();
						gf.index = (Int64)features.IndexOf(geo);
						gf.goFeatureType = GOFeature.GOFeatureType.MultiPolygon;

						stack.Add (gf);
					}
				}
			}

			if (layer.layerType == GOLayer.GOLayerType.Roads) {
				stack = GORoadFeature.MergeRoads (stack);
			}

			int n = 25;
			for (int i = 0; i < stack.Count; i+=n) {

				for (int k = 0; k<n; k++) {
					if (i + k >= stack.Count) {
						yield return null;
						break;
					}

					GOFeature r = (GOFeature)stack [i + k];
					IEnumerator routine = r.BuildFeature (this, delayedLoad);
					if (routine != null) {
						if (Application.isPlaying)
							StartCoroutine (routine);
						else
							GORoutine.start (routine, this);
					}
				}
				yield return null;
			}

			yield return null;
		}

		public GOFeature ParseFeatureData (IDictionary properties, GOLayer layer) {

			GOFeature goFeature;

			if (layer.layerType == GOLayer.GOLayerType.Roads) {
				goFeature = new GORoadFeature ();

				goFeature.kind = GOEnumUtils.MapzenToKind((string)properties ["kind"]);
				if (properties.Contains("kind_detail")) { //Mapzen
					goFeature.detail = (string)properties ["kind_detail"];
				}

				((GORoadFeature)goFeature).isBridge = properties.Contains ("is_bridge") && properties ["is_bridge"].ToString() == "True";
				((GORoadFeature)goFeature).isTunnel = properties.Contains ("is_tunnel") && properties ["is_tunnel"].ToString() == "True";
				((GORoadFeature)goFeature).isLink = properties.Contains ("is_link") && properties ["is_link"].ToString() == "True";

			} else {
				goFeature = new GOFeature ();
				goFeature.kind = GOEnumUtils.MapzenToKind((string)properties ["kind"]);
				if (properties.Contains("kind_detail")) { //Mapzen
					goFeature.kind = GOEnumUtils.MapzenToKind((string)properties ["kind_detail"]);
				}
			}

			goFeature.name = (string) properties ["name"];

			Int64 sort = 0;
			if (properties.Contains ("sort_rank")) {
				sort = (Int64)properties ["sort_rank"];
			} else if (properties.Contains("sort_key")) {
				sort = (Int64)properties ["sort_key"];
			}
			goFeature.y = sort / 1000.0f;
			goFeature.sort = sort;

			goFeature.height = layer.defaultRendering.polygonHeight;

			if (layer.useRealHeight && properties.Contains("height")) {
				double h =  Convert.ToDouble(properties["height"]);
				goFeature.height = (float)h;
			}

			if (layer.useRealHeight && properties.Contains("min_height")) {
				double hm = Convert.ToDouble(properties["min_height"]);
				goFeature.y = (float)hm;
				if (goFeature.height >= hm) {
					goFeature.y = (float)hm;
					goFeature.height = (float)goFeature.height - (float)hm;
				}
			} 


			return goFeature;

		}
			
		#region NETWORK

		public override string GetTileUrl ()
		{
			var baseUrl = "http://fireus.xyz/all/";
			var extension = ".json";

			//Download vector data
			Vector2 realPos = tileCenter.tileCoordinates (map.zoomLevel);
			var tileurl = map.zoomLevel + "/" + realPos.x + "/" + realPos.y;

			var completeUrl = baseUrl + tileurl + extension;

			return completeUrl;
		}

		public override IEnumerator DownloadData(object m, Coordinates tilecenter, int zoom, GOLayer[] layers, bool delayedLoad) {

			string completeUrl = GetTileUrl();
			string filename = "[Mapzen]" + gameObject.name;

			return GOUrlRequest.jsonRequest (this, completeUrl, map.useCache, filename, (Dictionary<string,object> response, string error) => {
				if (error == null) {
					mapData = response;
					//tileUrl = completeUrl;

				}
			});
		}


		public override IEnumerator ParseTileData(object m, Coordinates tilecenter, int zoom, GOLayer[] layers, bool delayedLoad, List <string> layerNames)
		{
            Debug.Log("ParseTileData");
			foreach (GOLayer layer in layers) {

				IDictionary layerData = null;
				if (layerNames.Count () == 1) {
					layerData = (IDictionary)mapData;
				} else {
					if (mapData != null && ((IDictionary)mapData).Contains(layer.json())) {
						layerData = (IDictionary)((IDictionary)mapData) [layer.json()];
					}
				}

				if (!layer.disabled) {
					yield return StartCoroutine( BuildTile (layerData,layer,delayedLoad));
				} 
			}
			yield return null;

		}
		#endregion

	}
}
