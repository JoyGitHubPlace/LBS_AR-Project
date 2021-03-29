using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using GoShared;
using Mapbox.VectorTile;


namespace WaveMap
{
	[ExecuteInEditMode]
	public class GOMapboxTile : GOPBFTile
	{
		public override string GetLayersStrings (GOLayer layer)
		{
			return layer.lyr();
		}

		public override GOFeature ParseFeatureData (VectorTileFeature feature, GOLayer layer) {

			IDictionary properties = feature.GetProperties ();
			GOFeature goFeature;

			if (layer.layerType == GOLayer.GOLayerType.Roads) {
				goFeature = new GORoadFeature ();

				((GORoadFeature)goFeature).isBridge = properties.Contains ("structure") && (string)properties ["structure"] == "bridge";
				((GORoadFeature)goFeature).isTunnel = properties.Contains ("structure") && (string)properties ["structure"] == "tunnel";
				((GORoadFeature)goFeature).isLink = properties.Contains ("structure") && (string)properties ["structure"] == "link";
			} else {
				goFeature = new GOFeature ();
			}

			goFeature.kind = GOEnumUtils.MapboxToKind((string)properties["class"]);

			goFeature.y = goFeature.index/100 + layer.defaultLayerY();

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
			var baseUrl = "https://api.mapbox.com:443/v4/mapbox.mapbox-streets-v7/";
			var extension = ".vector.pbf";

			//Download vector data
			Vector2 realPos = tileCenter.tileCoordinates (map.zoomLevel);
			var tileurl = map.zoomLevel + "/" + realPos.x + "/" + realPos.y;

			var completeUrl = baseUrl + tileurl + extension; 
//			var filename = "[MapboxVector]" + gameObject.name;


			return completeUrl;
		}
			

		#endregion

	}
}
