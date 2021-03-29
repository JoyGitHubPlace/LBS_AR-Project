using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GoShared;


namespace WaveMap
{
	
	public class GOEnvironment : MonoBehaviour {

		public GameObject [] treePrefab;
		public GameObject boatPrefab;
		public GameObject [] baloonPrefab;
        public int treeDistance = 30;

        public void SpawnBallons (GOTile tile) {
		
			//int spawn = Random.Range (0, 5);
			//if (spawn == 0) {
				//float y = Random.Range (90, 250);
				Vector3 pos = tile.tileCenter.convertCoordinateToVector ();
				//pos.y = y;
				int n = Random.Range (0, baloonPrefab.Length);
				GameObject obj = (GameObject)Instantiate (baloonPrefab[n]);
				obj.transform.position = pos;
				obj.transform.parent = transform;
			//}

		}
		public void GrowTrees (Mesh mesh, GOLayer layer, GOFeatureKind kind,Vector3 center) {
            //return ;
            // || kind == GOFeatureKind.recreation_ground | kind == GOFeatureKind.attraction || kind == GOFeatureKind.grass
            if (kind == GOFeatureKind.parking || kind == GOFeatureKind.park || kind == GOFeatureKind.garden || kind == GOFeatureKind.recreation_ground | kind == GOFeatureKind.attraction)
            {
                List<Vector3> treePoslist = new List<Vector3>();
                Vector3[] mv = mesh.vertices;
                if (mv.Length > 0)
                {
                    for (int i = 0;i<mv.Length-1;i++)
                    {
                        float d = Vector3.Distance(mv[i],mv[i+1]);
                        
                        if (d>treeDistance)
                        {
                            for (int j = 0; j < (int)(d / treeDistance); j++)
                            {
                                //Debug.Log(j/(d / treeDistance));
                                treePoslist.Add(Vector3.Lerp(mv[i], mv[i + 1],j /(d / treeDistance)));
                            }
                        }  
                    }
                    for (int i = 0; i < treePoslist.Count; i++)
                    {
                        var item = treePoslist[i];
                        var randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        int n = Random.Range(0, treePrefab.Length);
                        var p = new Vector3(item.x, treePrefab[n].transform.position.y* Global.tilesizeRank, item.z);
                        center.y = treePrefab[n].transform.position.y*Global.tilesizeRank;
                        p = Vector3.Lerp(p, center, Random.Range(0.2f,0.8f));
                        GameObject obj = (GameObject)Instantiate(treePrefab[n], p, randomRotation);
                        obj.transform.localScale = new Vector3(Global.tilesizeRank, Global.tilesizeRank, Global.tilesizeRank);
                        obj.transform.parent = transform;
                        obj.name = "tree"+i;
                    }
                }
            }
        }
			
		public void AddBoats (Mesh mesh, GOLayer layer, GOFeatureKind kind,Vector3 center) {

			bool spawn = Random.value > 0.5f;
			if (kind != GOFeatureKind.riverbank && kind != GOFeatureKind.water && spawn) {
				var randomRotation = Quaternion.Euler (0, Random.Range (0, 360), 0);
				center.y = 2;
				GameObject obj = (GameObject)Instantiate (boatPrefab, center, randomRotation);
				obj.transform.parent = transform;	
			}
		}


		public Vector3 RandomPositionInMesh(Mesh mesh){


			Bounds bounds = mesh.bounds;

			float minX = bounds.size.x * 0.5f;
			float minZ = bounds.size.z * 0.5f;

			Vector3 newVec = new Vector3(Random.Range (minX, -minX),
				gameObject.transform.position.y,
				Random.Range (minZ, -minZ));
			return newVec;
		}

	}
}


