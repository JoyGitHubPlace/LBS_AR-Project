using GoShared;
using UnityEngine.Serialization;


namespace WaveMap
{

	[System.Serializable]
	public class GOLayer
	{

		public string name {
			get {
				return layerType.ToString ();
			}
			set {
				this.name = value;
			}
		}

		public GOLayerType layerType;
		public enum GOLayerType  {
			Buildings,
			Landuse,
			Water,
			Earth,
			Roads
		}

		public bool isPolygon;
		public bool useRealHeight = false;
		public GORenderingOptions defaultRendering;
		public GORenderingOptions [] renderingOptions;

		public GOFeatureKind[] useOnly;
		public GOFeatureKind[] avoid;
		public bool useTunnels = true;
		public bool useBridges = true;
		public bool useColliders = false;
		public bool useLayerMask = false;

		public bool startInactive;
		public bool disabled = false;

		public GOEvent OnFeatureLoad; 


		public string json () {  //Mapzen

			return layerType.ToString ().ToLower ();
		}

		public string lyr () { //Mapbox
			switch (layerType) {
			case GOLayerType.Buildings:
				return "building";
			case GOLayerType.Landuse:
				return "landuse";
			case GOLayerType.Water:
				return "water";
			case GOLayerType.Earth:
				return "earth";
			case GOLayerType.Roads:
				return "road";
			default:
				return "";
			}		
		}

		public string lyr_osm () { //OSM
			switch (layerType) {
			case GOLayerType.Buildings:
				return "building";
			case GOLayerType.Landuse:
				return "landcover";
			case GOLayerType.Water:
				return "water";
			case GOLayerType.Earth:
				return "landcover";
			case GOLayerType.Roads:
				return "transportation";
			default:
				return "";
			}		
		}

		public string lyr_esri () { //Esri
			switch (layerType) {
			case GOLayerType.Buildings:
				return "Building";
			case GOLayerType.Landuse:
				return "Park or farming,Education,Cemetery,Medical,Landmark";
			case GOLayerType.Water:
				return "Water area,Marine area";
			case GOLayerType.Earth:
				return "Land";
			case GOLayerType.Roads:
				return "Road,Road tunnel,Railroad,Transportation";
			default:
				return "";
			}		
		}

		public float defaultLayerY() {
			return defaultLayerY (layerType);
		}

		public float defaultLayerY(GOLayerType t) {
            float h = 0;
			switch (t) {
			case GOLayerType.Buildings:
                    h = 0;
                    break;
			case GOLayerType.Landuse:
                    h = 0.3f;
                    break;
                case GOLayerType.Water:
                    h = 0.2f;
                    break;
                case GOLayerType.Earth:
                    h = 0.1f;
                    break;
                case GOLayerType.Roads:
                    h = 0.4f;
                    break;
                default:
                    h = 0;
                    break;
            }
            return h;

        }
	}
}