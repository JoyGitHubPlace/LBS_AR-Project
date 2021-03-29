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
	public class GOMapzenProtoTile : GOPBFTile
	{

		public override string GetLayersStrings (GOLayer layer)
		{
			return layer.json();
		}

		public override GOFeature ParseFeatureData (VectorTileFeature feature, GOLayer layer) {

			IDictionary properties = feature.GetProperties ();
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
				sort = Convert.ToInt64(properties ["sort_rank"]);
			} else if (properties.Contains("sort_key")) {
				sort = Convert.ToInt64(properties ["sort_key"]);
			}
			goFeature.y = sort / 1000.0f;
			goFeature.sort = sort;

			goFeature.height = layer.defaultRendering.polygonHeight;

			if (layer.useRealHeight && properties.Contains("height")) {
				double h =  Convert.ToDouble(properties["height"]);
				goFeature.height = (float)h*Global.tilesizeRank;
			}

			if (layer.useRealHeight && properties.Contains("min_height")) {
				double hm = Convert.ToDouble(properties["min_height"]) * Global.tilesizeRank;
				goFeature.y = (float)hm;
				if (goFeature.height >= hm) {
					goFeature.y = (float)hm;
					goFeature.height = (float)goFeature.height - (float)hm;
				}
			} 


			return goFeature;

		}

        #region NETWORK

        public override string GetTileUrl()
        {
            var baseUrl = "http://smap.vlab.club/mvt/v1/all/";// "https://tile.mapzen.com/mapzen/vector/v1/all/"; http://smap.vlab.club/mvt/v1/all/
            var extension = ".mvt";

            //Download vector data
            Vector2 realPos = tileCenter.tileCoordinates(map.zoomLevel);
            var tileurl = map.zoomLevel + "/" + realPos.x + "/" + realPos.y;

            var completeUrl = baseUrl + tileurl + extension;

            return completeUrl;

        }
        #endregion

    }
}
