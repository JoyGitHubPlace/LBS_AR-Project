using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GoShared {

	public class LocationManager : MonoBehaviour {

		public enum DemoLocation{
			NewYork, 
			Rome,
			NewYork2,
			Venice,
			SanFrancisco,
			Berlin,
			RioDeJaneiro,
			GrandCanyon,
			Matterhorn,
			NoGPSTest,
			Custom
		};

		public enum MotionPreset{
			Run, 
			Bike,
			Car
		};

		public enum MotionMode{
			Avatar, 
			GPS
		};

		public enum MotionState{
			Idle, 
			Walk,
			Run
		};
		public bool useLocationServices;
		public int zoomLevel = 16;

		public DemoLocation demoLocation;
		public Coordinates demo_CenterWorldCoordinates;
		[HideInInspector]
		public Vector2 demo_CenterWorldTile;

		[HideInInspector]
		public static Coordinates CenterWorldCoordinates;

		public float desiredAccuracy = 50;
		public float updateDistance = 0.1f;

		[HideInInspector]
		public float updateEvery = 1 / 1000f;

		public MotionPreset simulateMotion = MotionPreset.Run;

		public GameObject avatar;

		public bool useBannerInsideEditor;
		//public GameObject banner;
		//public Text bannerText;

		public static bool IsOriginSet;
		public static bool UseLocationServices;
		public static LocationServiceStatus status;

		public GOLocationEvent onOriginSet;
//		public delegate void OnOriginSet(Coordinates origin);

		public GOLocationEvent onLocationChanged;
//		public delegate void OnLocationChanged(Coordinates current);

		public GOMotionStateEvent OnMotionStateChanged;

		// Use this for initialization
		void Start () {
			LoadDemoLocation ();
            //Debug.Log(Vector2.Distance(new Vector2(40.55135f,-36.11618f),new Vector2(58.69318f,-21.77685f)));
		}
			

		void SetOrigin(Coordinates coords) {
			IsOriginSet = true;
			CenterWorldCoordinates = coords.tileCenter(zoomLevel);
			demo_CenterWorldTile = coords.tileCoordinates(zoomLevel);
			Coordinates.setWorldOrigin (CenterWorldCoordinates);
			if (onOriginSet != null) {
				onOriginSet.Invoke (CenterWorldCoordinates);
			}
		}



		#region DEMO LOCATIONS

		public void LoadDemoLocation () {

			SetOrigin(demo_CenterWorldCoordinates);

		}

		#endregion

	}

	[Serializable]
	public class GOMotionStateEvent : UnityEvent <GoShared.LocationManager.MotionState> {

       
	}

	[Serializable]
	public class GOLocationEvent : UnityEvent <GoShared.Coordinates> {


	}
}