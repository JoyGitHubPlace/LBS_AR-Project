using UnityEngine;
using System.Collections;
using WaveMap;
using GoShared;
public class GOObject : MonoBehaviour {
	
	public GOMap map;
	public Coordinates coordinatesGPS;

	// Use this for initialization
	void Awake () {

		if (map == null) {
			Debug.LogWarning ("GOObject - Map property not set");
			return;
		}
	}

	void LoadData (Coordinates currentLocation) {//This is called when the origin is set

		map.dropPin (coordinatesGPS.latitude, coordinatesGPS.longitude, gameObject);

	}

}
