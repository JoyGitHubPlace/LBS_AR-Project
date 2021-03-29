using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveMap
{

	public class GOStreetName : MonoBehaviour {


		public float roadLenght;
		public float textLenght;
		public Vector3 rot;

		public IEnumerator Build (string name) {
			
			GOFeatureBehaviour road = transform.parent.GetComponent<GOFeatureBehaviour> ();

			GORoadSegment segment = GORoadSegment.FindTheLongestStreightSegment(road.goFeature.convertedGeometry,0);
//			segment.DebugSegment ();

			transform.localPosition = segment.findMiddlePoint(0.01f); //LineCenter (road._verts); 
			transform.localScale = Vector3.one * 3;

			TextMesh textMesh = gameObject.AddComponent<TextMesh> ();
			textMesh.text = name;
			textMesh.color = new Color (61/255.0f, 61/255.0f, 83/255f);
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.alignment = TextAlignment.Center;
			textMesh.fontStyle = FontStyle.Bold;
			textMesh.fontSize = 15;

			float minimumFontSize = 12;

			textLenght = textMesh.GetComponent<Renderer>().bounds.size.x;

			//Find correct size
			for (int i = textMesh.fontSize; i >= minimumFontSize-1 ; i--) {
				textMesh.fontSize = i;
				float tl = textMesh.GetComponent<Renderer>().bounds.size.x;
				if (segment.distance >= tl) {
					break;
				}
				if (i==minimumFontSize-1) {
					GameObject.Destroy (this.gameObject);
					yield break;
				}
			}
						
			var rotation = transform.eulerAngles;
			rotation.x = 90;

			Vector3 targetDir = segment.direction ();
			if (targetDir.Equals (Vector3.zero)) {
				rotation.y = 90;
			} 
			else {
				Quaternion finalRotation = Quaternion.LookRotation (targetDir);
				rotation.y = finalRotation.eulerAngles.y + 90;

				rotation.y = (rotation.y % 360 + 360) % 360;

				if (rotation.y > 90 && rotation.y < 180) {
					rotation.y -= 180;
				} 
				else if (rotation.y >0 && rotation.y < 90) {
					rotation.y += 180;
				} 
			}

			rot = rotation;
			transform.eulerAngles = rotation;


			yield return null;

		}

	}


	public class GORoadSegment {

		public Vector3 pointA;
		public Vector3 pointB;
		public float distance;
		public float angle;

		public static GORoadSegment FindTheLongestStreightSegment(List<Vector3> line, float maxAngle) {

			Vector3 pointA = Vector3.zero;
			Vector3 pointB = Vector3.zero;
			float d = 0;
			float angle = 0;

			GORoadSegment spare = null;

			for (int i = 1; i < line.Count; i++) {

				if (i == 1) {
					pointA = line [0];
					pointB = line [1];
					d = Vector3.Distance (pointA, pointB);
					angle = AngleBetweenVector2XZ (pointA, pointB);
					continue;
				}

				Vector3 stepA = line [i - 1];
				Vector3 stepB = line [i];
				float stepD = Vector3.Distance (stepA, stepB);
				float stepAngle = AngleBetweenVector2XZ (stepA, stepB);
				float angleDiff = Mathf.Abs (stepAngle - angle);

				if (spare != null && Mathf.Abs (stepAngle - spare.angle) <= maxAngle) {
					stepD += spare.distance;
					stepA = spare.pointA;
					spare = null;
				}


				if (angleDiff > maxAngle) { //angle is too wide
				
					if (stepD > d) { //Reset segment
						pointA = stepA;
						pointB = stepB;
						d = stepD;
						angle = stepAngle;
//						Debug.Log ("Reset segment");
					} 
					else { //Save this segment for next step, just in case
//						Debug.Log ("Save segment");
						GORoadSegment s = new GORoadSegment ();
						s.pointA = stepA;
						s.pointB = stepB;
						s.angle = stepAngle;
						s.distance = stepD;
						spare = s;
					}
	
				} else { //angle is ok, add the current segment

					pointB = stepB;
					d += stepD;
//					Debug.Log ("Add segment "+angle+ " " + stepAngle);
				}
			}



			GORoadSegment segment = new GORoadSegment ();
			segment.pointA = pointA;
			segment.pointB = pointB;
			segment.angle = angle;
			segment.distance = d;

			return segment;

		}
	


		public static float AngleBetweenVector2XZ(Vector3 vec1, Vector3 vec2)
		{
			Vector3 diference = vec2 - vec1;
			float sign = (vec2.z < vec1.z)? -1.0f : 1.0f;
			return Vector3.Angle(Vector3.right, diference) * sign;
		}

		public static float AngleBetweenVector3(Vector3 vec1, Vector3 vec2)
		{
			Vector3 diference = vec2 - vec1;
			float sign = (vec2.y < vec1.y)? -1.0f : 1.0f;
			return Vector3.Angle(Vector3.right, diference) * sign;
		}


		public Vector3 findMiddlePoint (float y) {

			Vector3 v = Vector3.Lerp(pointA, pointB,0.5f);
			v.y += y;
			return v;
		}

		public Vector3 direction () {

			return  (pointB - pointA);
		}

		public void DebugSegment() {

			Debug.DrawLine(pointA, pointB, Color.red, 5000000000, false);
		}
	}
}