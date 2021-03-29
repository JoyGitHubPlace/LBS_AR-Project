using UnityEngine;
using System.IO;

#if !UNITY_WEBPLAYER

namespace GoShared {

	public class FileHandler : MonoBehaviour {

		public static bool Exist(string filename) {

			string path = System.IO.Path.Combine (Application.persistentDataPath,filename);
	//		Debug.Log ("Exist at path: "+ path);
			return File.Exists(path);
		}

		public static void Save(string filename, byte[] bytes) {

			string path = System.IO.Path.Combine (Application.persistentDataPath,filename);
	//		Debug.Log ("Save path: "+ path);
			File.WriteAllBytes(path, bytes);
		}

		public static byte[] Load(string filename) {
	       
			string path = System.IO.Path.Combine (Application.persistentDataPath,filename);
			return File.ReadAllBytes (path);
		}

		public static void Remove(string filename) {
			string path = System.IO.Path.Combine (Application.persistentDataPath,filename);
			if (File.Exists (path)) {
				File.Delete (path);
			} 
		}

		public static void SaveText(string filename, string stringToWrite) {

			string path = System.IO.Path.Combine (Application.persistentDataPath,filename);
	//		Debug.Log ("Save path: "+ path);
			File.WriteAllText(path,stringToWrite);
		}

		public static string LoadText(string filename) {
			string path = System.IO.Path.Combine (Application.persistentDataPath,filename);
	//		Debug.Log ("Load path: "+ path);
			return File.ReadAllText (path);
		}

		public static void ClearEverythingInFolder(string path) {

			var info = new DirectoryInfo(path);
			var fileInfo = info.GetFiles();
			Debug.Log (info);

			foreach (FileInfo file in fileInfo) {
				Debug.Log (file.Name);
				file.Delete ();
			}

		}
	}
}
#endif 