using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoShared {
	
	public enum GOFeatureKind {

		//Unknown
		baseKind,

		//Buildings
		abandoned,
		administrative,
		agricultural,
		airport,
		allotment_house,
		apartments,
		arbour,
		bank,
		barn,
		basilica,
		beach_hut,
		bell_tower,
		boathouse,
		brewery,
		bridge,
		bungalow,
		bunker,
		cabin,
		carport,
		castle,
		cathedral,
		chapel,
		chimney,
		church,
		civic,
		clinic,
		closed,
		clubhouse,
		collapsed,
		college,
		commercial,
		construction,
		container,
		convent,
		cowshed,
		dam,
		damaged,
		depot,
		destroyed,
		detached,
		disused,
		dormitory,
		duplex,
		factory,
		farm,
		farm_auxiliary,
		fire_station,
		garage,
		garages,
		gazebo,
		ger,
		glasshouse,
		government,
		grandstand,
		greenhouse,
		hangar,
		healthcare,
		hermitage,
		historical,
		hospital,
		hotel,
		house,
		houseboat,
		hut,
		industrial,
		kindergarten,
		kiosk,
		library,
		mall,
		manor,
		manufacture,
		mixed_use,
		mobile_home,
		monastery,
		mortuary,
		mosque,
		museum,
		office,
		outbuilding,
		parking,
		pavilion,
		power,
		prison,
		proposed,
		pub,
		residential,
		restaurant,
		retail,
		roof,
		ruin,
		ruins,
		school,
		semidetached_house,
		service,
		shed,
		shelter,
		shop,
		shrine,
		silo,
		slurry_tank,
		stable,
		stadium,
		static_caravan,
		storage,
		storage_tank,
		store,
		substation,
		summer_cottage,
		summer_house,
		supermarket,
		synagogue,
		tank,
		temple,
		terrace,
		tower,
		train_station,
		transformer_tower,
		transportation,
		university,
		utility,
		veranda,
		warehouse,
		wayside_shrine,
		works,


		//Landuse

		aerodrome,
		allotments,
		amusement_ride,
		animal,
		apron,
		aquarium,
		artwork,
		attraction,
		aviary,
		battlefield,
		beach,
		breakwater,
		camp_site,
		caravan_site,
		carousel,
		cemetery,
		cinema,
		city_wall,
		common,
		cutline,
		dike,
		dog_park,
		enclosure,
		farmland,
		farmyard,
		fence,
		footway,
		forest,
		fort,
		fuel,
		garden,
		gate,
		generator,
		glacier,
		golf_course,
		grass,
		grave_yard,
		groyne,
		hanami,
		land,
		maze,
		meadow,
		military,
		national_park,
		nature_reserve,
		natural_forest,// - See planned bug fix in #1096.
		natural_park,// - See planned bug fix in #1096.
		natural_wood,// - See planned bug fix in #1096.
		park,
		pedestrian,
		petting_zoo,
		picnic_site,
		pier,
		pitch,
		place_of_worship,
		plant,
		playground,
		protected_area,
		quarry,
		railway,
		recreation_ground,
		recreation_track,
		resort,
		rest_area,
		retaining_wall,
		rock,
		roller_coaster,
		runway,
		rural,
		scree,
		scrub,
		service_area,
		snow_fence,
		sports_centre,
		stone,
		summer_toboggan,
		taxiway,
		theatre,
		theme_park,
		trail_riding_station,
		tree_row,
		urban_area,
		urban,
		village_green,
		wastewater_plant,
		water_park,
		water_slide,
		water_works,
		wetland,
		wilderness_hut,
		wildlife_park,
		winery,
		winter_sports,
		wood,
		zoo,

		//Water

		basin, //polygon
		bay, //point, intended for label placement only
		canal, //line
		ditch, //line
		dock,  //polygon
		drain, //line
		fjord, //point, intended for label placement only
		lake, //polygon
		ocean, //polygon + point, intended for label placement only
		playa, //polygon
		river, //line
		riverbank, //polygon
		sea, //point, intended for label placement only
		stream, //line
		strait, //point, intended for label placement only
		swimming_pool, //polygon
		water, //polygon

		//Roads

		highway,
		major_road,
		minor_road,
		path,
		rail,
		ferry
	};

	public class GOEnumUtils {

		#region LANDUSE

		public static GOFeatureKind MapzenToKind(string kind) {

			try {
				GOFeatureKind parsed_enum = (GOFeatureKind)System.Enum.Parse( typeof( GOFeatureKind ), kind );
				return parsed_enum;
			} catch {
				return GOFeatureKind.baseKind;
			}

		}

		public static GOFeatureKind MapboxToKind(string kind) {

//			Debug.Log (kind);

			if (kind == null)
				return GOFeatureKind.baseKind;

			//This is very empiric. looking forward to a more complete system

			if (kind.Contains ("motorway")) {
				return GOFeatureKind.highway;
			} else if (kind == "service" || kind == "secondary" || kind == "street" || kind == "tertiary" || kind == "transit" || kind == "minor") {
				return GOFeatureKind.minor_road;
			} else if (kind == "rail" || kind.Contains ("rail")) {
				return GOFeatureKind.rail;
			} else if (kind == "primary") {
				return GOFeatureKind.major_road;
			} else if (kind == "track") {
				return GOFeatureKind.path;
			} 


			try {
				GOFeatureKind parsed_enum = (GOFeatureKind)System.Enum.Parse( typeof( GOFeatureKind ), kind );
				return parsed_enum;
			} catch {
				return GOFeatureKind.baseKind;
			}

		}



		#endregion
	}
}