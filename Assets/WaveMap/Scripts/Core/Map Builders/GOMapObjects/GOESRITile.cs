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
	public class GOEsriTIle : GOPBFTile
	{

		public override string GetLayersStrings (GOLayer layer)
		{
			return layer.lyr_esri();
		}

		public override GOFeature ParseFeatureData (VectorTileFeature feature, GOLayer layer) {

			IDictionary properties = feature.GetProperties ();
			GOFeature goFeature;

			if (layer.layerType == GOLayer.GOLayerType.Roads) {
				goFeature = new GORoadFeature ();
			} else {
				goFeature = new GOFeature ();
			}

			goFeature.kind = GOEnumUtils.MapboxToKind(layer.name);

			goFeature.y = layer.defaultLayerY();
			if (properties.Contains ("_symbol"))
				goFeature.y = Convert.ToInt64 (properties ["_symbol"]) / 10.0f*Global.tilesizeRank;

			goFeature.height = layer.defaultRendering.polygonHeight;

			return goFeature;

		}

		#region NETWORK

		public override string GetTileUrl ()
		{
            Debug.Log("aaaaaaaaaaaaaaaa");
			var baseUrl = "https://basemaps.arcgis.com/v1/arcgis/rest/services/World_Basemap/VectorTileServer/tile/";
			var extension = ".pbf";

			//Download vector data
			Vector2 realPos = tileCenter.tileCoordinates (map.zoomLevel);
			var tileurl = map.zoomLevel + "/" + realPos.y + "/" + realPos.x; //of course Esri uses inverted tile x,y. =/
			var completeUrl = baseUrl + tileurl + extension; 
//			var filename = "[ESRIVector]" + gameObject.name;

			return completeUrl;
		}
			

		#endregion

	}
}
