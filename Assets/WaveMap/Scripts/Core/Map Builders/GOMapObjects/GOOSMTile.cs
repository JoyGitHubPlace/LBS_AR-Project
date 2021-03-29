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
	public class GOOSMTile : GOPBFTile
	{

		public override string GetLayersStrings (GOLayer layer)
		{
			return layer.lyr_osm ();
		}

		public override GOFeature ParseFeatureData (VectorTileFeature feature, GOLayer layer) {
			IDictionary properties = feature.GetProperties ();
			GOFeature goFeature;

			if (layer.layerType == GOLayer.GOLayerType.Roads) {
				goFeature = new GORoadFeature ();
				((GORoadFeature)goFeature).isBridge = properties.Contains ("brunnel") && (string)properties ["brunnel"] == "bridge";
				((GORoadFeature)goFeature).isTunnel = properties.Contains ("brunnel") && (string)properties ["brunnel"] == "tunnel";
				((GORoadFeature)goFeature).isLink = properties.Contains ("brunnel") && (string)properties ["brunnel"] == "link";
			} else {
				goFeature = new GOFeature ();
			}

			goFeature.kind = GOEnumUtils.MapboxToKind((string)properties["class"]);

			goFeature.y = goFeature.index/1000 + layer.defaultLayerY();

			if (goFeature.kind == GOFeatureKind.lake) //terrible fix for vector maps without a sort value.
				goFeature.y = layer.defaultLayerY (GOLayer.GOLayerType.Landuse);

			goFeature.height = layer.defaultRendering.polygonHeight;

			if (layer.useRealHeight && properties.Contains("render_height")) {
				double h =  Convert.ToDouble(properties["render_height"]);
				goFeature.height = (float)h;
			}

			if (layer.useRealHeight && properties.Contains("render_min_height")) {
				double hm = Convert.ToDouble(properties["render_min_height"]);
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
			var baseUrl = "https://free-0.tilehosting.com/data/v3/";
			var extension = ".pbf.pict";

			Vector2 realPos = tileCenter.tileCoordinates (map.zoomLevel);
			var tileurl = map.zoomLevel + "/" + realPos.x + "/" + realPos.y;
			var completeUrl = baseUrl + tileurl + extension; 
			return completeUrl;
		}

		#endregion

	}
}
