using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Events;
using System.Linq;
using GoShared;

public static class GM
{
    private const int TileSize = 256;
    private const int EarthRadius = 6378137;
    private const double InitialResolution = 2 * Math.PI * EarthRadius / TileSize;
    private const double OriginShift = 2 * Math.PI * EarthRadius / 2;

    public static Vector2 LatLonToMeters(Vector2 v)
    {
        return LatLonToMeters(v.x, v.y);
    }

    //Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913
    public static Vector2 LatLonToMeters(double lat, double lon)
    {
        var p = new Vector2();
        p.x = (float)(lon * OriginShift / 180);
        p.y = (float)(Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180));
        p.y = (float)(p.y * OriginShift / 180);
        return new Vector2(p.x, p.y);
    }

    //Converts pixel coordinates in given zoom level of pyramid to EPSG:900913
    public static Vector2 PixelsToMeters(Vector2 p, int zoom)
    {
        var res = Resolution(zoom);
        var met = new Vector2();
        met.x = (float)(p.x * res - OriginShift);
        met.y = -(float)(p.y * res - OriginShift);
        return met;
    }

    //Converts EPSG:900913 to pyramid pixel coordinates in given zoom level
    public static Vector2 MetersToPixels(Vector2 m, int zoom)
    {
        var res = Resolution(zoom);
        var pix = new Vector2();
        pix.x = (float)((m.x + OriginShift) / res);
        pix.y = (float)((-m.y + OriginShift) / res);
        return pix;
    }

    //Returns a TMS (NOT Google!) tile covering region in given pixel coordinates
    public static Vector2 PixelsToTile(Vector2 p)
    {
        var t = new Vector2();
        t.x = (int)Math.Ceiling(p.x / (double)TileSize) - 1;
        t.y = (int)Math.Ceiling(p.y / (double)TileSize) - 1;
        return t;
    }

    public static Vector2 PixelsToRaster(Vector2 p, int zoom)
    {
        var mapSize = TileSize << zoom;
        return new Vector2(p.x, mapSize - p.y);
    }

    //Returns tile for given mercator coordinates
    public static Vector2 MetersToTile(Vector2 m, int zoom)
    {
        var p = MetersToPixels(m, zoom);
        return PixelsToTile(p);
    }

    //Returns bounds of the given tile in EPSG:900913 coordinates
    public static Rect TileBounds(Vector2 t, int zoom)
    {
        var min = PixelsToMeters(new Vector2(t.x * TileSize, t.y * TileSize), zoom);
        var max = PixelsToMeters(new Vector2((t.x + 1) * TileSize, (t.y + 1) * TileSize), zoom);
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    //Returns bounds of the given tile in latutude/longitude using WGS84 datum
    //public static Rect TileLatLonBounds(Vector2 t, int zoom)
    //{
    //    var bound = TileBounds(t, zoom);
    //    var min = MetersToLatLon(new Vector2(bound.xMin, bound.yMin));
    //    var max = MetersToLatLon(new Vector2(bound.xMax, bound.yMax));
    //    return new Rect(min.x, min.y, Math.Abs(max.x - min.x), Math.Abs(max.y - min.y));
    //}

    //Resolution (meters/pixel) for given zoom level (measured at Equator)
    public static double Resolution(int zoom)
    {
        return InitialResolution / (Math.Pow(2, zoom));
    }

    public static double ZoomForPixelSize(double pixelSize)
    {
        for (var i = 0; i < 30; i++)
            if (pixelSize > Resolution(i))
                return i != 0 ? i - 1 : 0;
        throw new InvalidOperationException();
    }

    // Switch to Google Tile representation from TMS
    public static Vector2 ToGoogleTile(Vector2 t, int zoom)
    {
        return new Vector2(t.x, ((int)Math.Pow(2, zoom) - 1) - t.y);
    }

    // Switch to TMS Tile representation from Google
    public static Vector2 ToTmsTile(Vector2 t, int zoom)
    {
        return new Vector2(t.x, ((int)Math.Pow(2, zoom) - 1) - t.y);
    }
}
namespace WaveMap
{
	//[ExecuteInEditMode]
	[System.Serializable]
	public class GOMap : MonoBehaviour 
	{


		public LocationManager locationManager;
		[Range (0,8)]  public int tileBuffer = 2;
		[ShowOnly] public int zoomLevel = 0;
		public bool useCache = true;

		[InspectorButton("ClearCache")] public bool clearCache;

        public GameObject _player;

		public Material tileBackground;
		[HideInInspector]
		public GameObject tempTileBackgorund;
		public GOLayer[] layers;

		public GOTileEvent OnTileLoad;


		Vector2 Center_tileCoords;
        [HideInInspector]
        public Dictionary<string, GOTile> tiles;

		//features ID
		[HideInInspector]
		public IList buildingsIds = new List<object>();
        private Vector2 CueVector;
        private Vector2 ChVector = new Vector2(0.5f,0.5f);
        void Awake () 
	    {
            tiles = new Dictionary<string, GOTile>();
            OnOriginSet();
            zoomLevel = locationManager.zoomLevel;

			if (zoomLevel == 0) {
				zoomLevel = locationManager.zoomLevel;	
			}	
			if (tileBackground != null && Application.isPlaying) {
				CreateTemporaryMapBackground ();
			}
	    }
        int i = 0;
        void Update()
        {
            i = i + 1;
            if (i>100)
            {
                i = 0;
                OnOriginSet();
            }
        }
        private Vector2 GetCurVector()
        {
            if (_player == null)
                return ChVector;
            var dif = _player.transform.position;
            var tileDif = Vector2.zero;
            float size = 1041.5f * Global.tilesizeRank;
            if (dif.x < 0)
                tileDif.x = (int)((dif.x - size / 2) / size);
            else
                tileDif.x = (int)((dif.x + size / 2) / size);
            if (-dif.z < 0)
                tileDif.y = (int)((-dif.z - size / 2) / size);
            else
                tileDif.y = (int)((-dif.z + size / 2) / size);
            return tileDif;
        }
        void OnOriginSet () {

            var GCV = GetCurVector();
            if (GCV != ChVector)
            {
                var gps = locationManager.demo_CenterWorldCoordinates;
                var v2 = GM.LatLonToMeters(gps.latitude, gps.longitude);
                var tile = GM.MetersToTile(v2, zoomLevel);
                ChVector = GCV;
                CueVector = tile + GCV;
                StartCoroutine(ReloadMap(CueVector, false));
            }
            
		}

        #region GoMap Load
        private List<Vector2> veclist(Vector2 tms,int tBuffer)
        {
            
            List<Vector2> vlist = new List<Vector2>();
            for (int i = -tBuffer; i <= tBuffer; i++)
            {
                for (int j = -tBuffer; j <= tBuffer; j++)
                {
                    var v = new Vector2(tms.x + i, tms.y + j);
                    vlist.Add(v);
                }
            }
            return vlist;
        }
		public IEnumerator ReloadMap (Vector2 location, bool delayed) {
           
            //Get SmartTiles
            List<Vector2> tileList = veclist(location,tileBuffer);
            List <GOTile> newTiles = new List<GOTile> ();
			// Create new tiles
			foreach (Vector2 tileCoords in tileList) {
                string name = tileCoords.x + "-" + tileCoords.y + "-" + zoomLevel;
                if (!tiles.ContainsKey (name)) {
					GOTile adiacentSmartTile = createSmartTileObject (tileCoords, zoomLevel);
					adiacentSmartTile.tileCenter = new Coordinates (tileCoords, zoomLevel);
					adiacentSmartTile.gameObject.transform.position = adiacentSmartTile.tileCenter.convertCoordinateToVector();
					newTiles.Add (adiacentSmartTile);

					if (tileBackground != null) {
						CreateTileBackground (adiacentSmartTile);
					}
				}
			}
			foreach (GOTile tile in newTiles) {
				#if !UNITY_WEBPLAYER
				if (tile != null && FileHandler.Exist (tile.gameObject.name) && useCache) {
					yield return tile.StartCoroutine(tile.LoadTileData(this, tile.tileCenter, zoomLevel,layers,delayed));
				} else {
					tile.StartCoroutine(tile.LoadTileData(this, tile.tileCenter, zoomLevel,layers,delayed));
				}
				#endif
			}
            yield return null;
        }

		public List <string> layerNames () {
		
			List <string> layerNames = new List<string>();
			for (int i = 0; i < layers.ToList().Count; i++) {
				if (layers [i].disabled == false) {
					layerNames.Add(layers [i].json());
				}
			}
			return layerNames;
		}

		public void DestroyTiles (List <Vector2> list) {

			try {
				List <string> tileListNames = new List <string> ();
                Debug.Log(list.Count);
				foreach (Vector2 v in list) {
					tileListNames.Add ((int)v.x + "-" + (int)v.y + "-" + zoomLevel);
				}
                Debug.Log(tileListNames.Count);
				foreach (var kv in tiles) {
					if (!tileListNames.Contains (kv.Key)) {
                        tiles[kv.Key] = null;
                        GameObject.Destroy(kv.Value.gameObject);
                    }
				}
			} catch  {
				
			}
		}


		GOTile createSmartTileObject (Vector2 tileCoords, int Zoom) {
			GameObject tileObj = new GameObject((int)tileCoords.x+ "-" + (int)tileCoords.y+ "-" + zoomLevel);
			tileObj.transform.parent = gameObject.transform;
			GOTile tile = tileObj.AddComponent<GOMapzenProtoTile> ();
			tiles.Add(tileObj.name,tile);
			return tile;
		}

		#endregion

		#region Utils

		public void dropPin(double lat, double lng, GameObject go) {

			Transform pins = transform.FindChild ("Pins");
			if (pins == null) {
				pins = new GameObject ("Pins").transform;
				pins.parent = transform;
			}

			Coordinates coordinates = new Coordinates (lat, lng,0);
			go.transform.localPosition = coordinates.convertCoordinateToVector(0);
			go.transform.parent = pins;	
		}

		#endregion

		#region Tile Background

		private void CreateTileBackground(GOTile tile) {

			MeshFilter filter = tile.gameObject.AddComponent<MeshFilter>();
			MeshRenderer renderer = tile.gameObject.AddComponent<MeshRenderer>();

			tile.vertices = new List<Vector3> ();
			IList verts = tile.tileCenter.tileVertices (zoomLevel);
			foreach (Vector3 v in verts) {
				tile.vertices.Add(tile.transform.InverseTransformPoint(v));
			}

			Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
			poly.outside = tile.vertices;
			Mesh mesh = Poly2Mesh.CreateMesh (poly);

			Vector2[] uvs = new Vector2[mesh.vertices.Length];
			for (int i=0; i < uvs.Length; i++) {
				uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
			}
			mesh.uv = uvs;

			filter.sharedMesh = mesh;
			tile.gameObject.AddComponent<MeshCollider> ();

			renderer.material = tileBackground;

		}

		private void CreateTemporaryMapBackground () {

			tempTileBackgorund = new GameObject ("Ground");

			MeshFilter filter = tempTileBackgorund.AddComponent<MeshFilter>();
			MeshRenderer renderer = tempTileBackgorund.AddComponent<MeshRenderer>();

			float size = 1000;

			Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
			poly.outside = new List<Vector3> {
				new Vector3(size, -0.1f, size),
				new Vector3(size, -0.1f, -size),
				new Vector3(-size, -0.1f, -size),
				new Vector3(-size,-0.1f ,size)
			};
			Mesh mesh = Poly2Mesh.CreateMesh (poly);

			Vector2[] uvs = new Vector2[mesh.vertices.Length];
			for (int i=0; i < uvs.Length; i++) {
				uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
			}
			mesh.uv = uvs;

			filter.mesh = mesh;
			renderer.material = tileBackground;
		
		} 

		private void DestroyTemporaryMapBackground () {
			GameObject.DestroyImmediate (tempTileBackgorund);
			tempTileBackgorund = null;
		}

		#endregion

		#region CacheControl

		public void ClearCache() {
			FileHandler.ClearEverythingInFolder (Application.persistentDataPath);
		}

		#endregion

		#region Editor Map Builder

		public void BuildInsideEditor () {


		}
		#endregion
	}




	#region Events
	[Serializable]
	public class GOEvent : UnityEvent <Mesh,GOLayer,GOFeatureKind,Vector3> {


	}

	[Serializable]
	public class GOTileEvent : UnityEvent <GOTile> {


	}
	#endregion
}