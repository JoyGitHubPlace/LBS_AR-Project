using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

namespace WaveMap
{

    public class GOFeatureMeshBuilder
    {
        GOFeature feature;
        public Mesh mesh;
        public Mesh mesh2D;
        public Vector3 center;

        public GOFeatureMeshBuilder(GOFeature f) {

            feature = f;
            if (feature.goFeatureType == WaveMap.GOFeature.GOFeatureType.Polygon || feature.goFeatureType == WaveMap.GOFeature.GOFeatureType.MultiPolygon)
                center = feature.convertedGeometry.Aggregate((acc, cur) => acc + cur) / feature.convertedGeometry.Count;

        }

        #region Builders

        public void BuildLine(GameObject line, GOLayer layer, GORenderingOptions renderingOptions, GOMap map)
        {
            if (feature.convertedGeometry.Count == 2 && feature.convertedGeometry[0].Equals(feature.convertedGeometry[1]))
            {
                return;
            }

            if (renderingOptions.tag.Length > 0)
            {
                line.tag = renderingOptions.tag;
            }

            if (renderingOptions.material)
                renderingOptions.material.renderQueue = -(int)feature.sort;
            if (renderingOptions.outlineMaterial)
                renderingOptions.outlineMaterial.renderQueue = -(int)feature.sort;

            GOLineMesh lineMesh = new GOLineMesh(feature.convertedGeometry);
            lineMesh.width = renderingOptions.lineWidth * Global.tilesizeRank;
            lineMesh.load(line);
            mesh = lineMesh.mesh;
            mesh = SimpleExtruder.Extrude(mesh, line, 0.05f);
            line.GetComponent<MeshFilter>().sharedMesh = mesh;
            line.GetComponent<Renderer>().material = renderingOptions.material;
            Vector3 position = line.transform.position;
            position.y = feature.y * Global.tilesizeRank;


            line.transform.position = position;

            if (renderingOptions.outlineMaterial != null)
            {
                GameObject outline = CreateRoadOutline(line, renderingOptions.outlineMaterial, Global.tilesizeRank * (renderingOptions.lineWidth + layer.defaultRendering.outlineWidth));
                if (layer.useColliders)
                    outline.AddComponent<MeshCollider>().sharedMesh = outline.GetComponent<MeshFilter>().sharedMesh;

                outline.layer = line.layer;
                outline.tag = line.tag;

            }
            else if (layer.useColliders)
            {
                //				Mesh m = gameObject.GetComponent<MeshFilter> ().sharedMesh;
                line.AddComponent<MeshCollider>();
            }
        }

        public GameObject BuildPolygon(string name, GOLayer layer, float height)
        {
            //GameObject polygon = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/CityBuildings/" + "Building_CornerHouse_A"));
            GameObject polygon = new GameObject();
            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
            poly.outside = feature.convertedGeometry;
            if (feature.clips != null) {
                foreach (IList clipVerts in feature.clips) {
                    poly.holes.Add(GOFeature.CoordsToVerts(clipVerts));
                }
            }

            MeshFilter filter = polygon.AddComponent<MeshFilter>();
            polygon.AddComponent(typeof(MeshRenderer));

            try {
                mesh = Poly2Mesh.CreateMesh(poly);
            } catch {
            }
            if (height > 1)
            {
                if (mesh)
                {
                    Vector2[] uv2d = new Vector2[mesh.vertices.Length];
                    for (int i = 0; i < uv2d.Length; i++)
                    {
                        uv2d[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
                    }
                    var meshlemp = Mesh.Instantiate(mesh);
                    meshlemp.uv = uv2d;
                    mesh2D = Mesh.Instantiate(meshlemp);

                    mesh.uv = uv2d;
                    if (height > 0)
                    {
                        mesh = SimpleExtruder.Extrude(mesh, polygon, height);
                        Vector2[] uvs3D = new Vector2[mesh.vertices.Length];
                        for (int i = 0; i < uvs3D.Length - 1; i++)
                        {
                            uvs3D[i] = new Vector2(Vector2.Distance(new Vector2(mesh.vertices[i + 1].x, mesh.vertices[i + 1].z), new Vector2(mesh.vertices[i].x, mesh.vertices[i].z)), mesh.vertices[i].y);
                        }
                        // uvs2[uvs2.Length - 1] = new Vector2(Mathf.Sqrt((float)(Math.Pow(mesh.vertices[0].x - mesh.vertices[uvs2.Length - 1].x, 2) + Math.Pow(mesh.vertices[0].z - mesh.vertices[uvs2.Length - 1].z, 2))), mesh.vertices[uvs2.Length - 1].y);
                        uvs3D[uvs3D.Length - 1] = new Vector2(10, mesh.vertices[uvs3D.Length - 1].y);
                        mesh.uv = uvs3D;
                    }
                }
            }
            else
            {
                if (mesh)
                {
                    Vector2[] uvs = new Vector2[mesh.vertices.Length];
                    for (int i = 0; i < uvs.Length; i++)
                    {
                        uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
                    }
                    mesh.uv = uvs;
                    if (height > 0)
                    {
                        mesh = SimpleExtruder.Extrude(mesh, polygon, height);
                    }
                }
            }
            filter.sharedMesh = mesh;

            if (layer.useColliders)
                polygon.AddComponent<MeshCollider>().sharedMesh = mesh;
            return polygon;

        }
        public GameObject BuildPolygon(string name, Vector3 pos)
        {
            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
            poly.outside = feature.convertedGeometry;
            if (feature.clips != null)
            {
                foreach (IList clipVerts in feature.clips)
                {
                    poly.holes.Add(GOFeature.CoordsToVerts(clipVerts));
                }
            }
            try
            {
                mesh = Poly2Mesh.CreateMesh(poly);
            }
            catch
            {
            }
            float d = 0f;
            Vector3 f_Pos = pos;
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                var l = Vector2.Distance(pos, mesh.vertices[i]);
                if (d < l)
                {
                    d = l;
                    f_Pos = mesh.vertices[i];
                }
            }
            float ea_y = GetSC(f_Pos, pos);
            var sc = d / 1.414f ;
            if (sc > 2f)
            {
                sc = sc / 4;
                GameObject polygon = new GameObject("polygon");
                polygon.transform.localPosition = pos;
                //for (int i = 0;i<2;i++)
               // {
                //    for (int j = 0;j<2;j++)
                 //   {
                        GameObject item = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/CityBuildings/" + "B_10"));
                        item.transform.localPosition = pos;//new Vector3((pos.x-sc/2)+sc*i,0, (pos.z - sc / 2) + sc * j) ;
                        item.transform.localEulerAngles = new Vector3(0, ea_y, 0);
                        item.transform.localScale = Vector3.one * (sc * Global.tilesizeRank);
                        item.transform.SetParent(polygon.transform);
                  //  }
             //   }
                polygon.transform.Rotate(0, ea_y,0);
                return polygon;
            }
            else
            {
                if (sc<0.8f)
                {
                    sc =1f;
                }
                GameObject polygon = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/CityBuildings/" + "B_" + UnityEngine.Random.Range(3, 10)));
                polygon.transform.localPosition = pos;
                polygon.transform.localEulerAngles = new Vector3(0, ea_y, 0);
                polygon.transform.localScale = Vector3.one * (sc * Global.tilesizeRank);
                return polygon;
            }
        }

        private float GetSC(Vector3 f_Pos,Vector3 pos)
        {
            var ea_y = 0f;
            if (f_Pos.x == pos.x)
            {
                ea_y = 0f;
            }
            else
            {
                ea_y = (float)(Math.Atan((f_Pos.z - pos.z) / (f_Pos.x - pos.x)) * (180 / Math.PI)) ;
            }
            return ea_y;
        }
        #endregion

        #region LINE UTILS
        GameObject CreateRoadOutline (GameObject line, Material material, float width) {

			GameObject outline = new GameObject (line.name+"outline");
			outline.transform.parent = line.transform;

			material.renderQueue = -((int)feature.sort-1);
            GOLineMesh lineMesh = new GOLineMesh(feature.convertedGeometry);
            lineMesh.width = width;
            lineMesh.load(outline);
            mesh = lineMesh.mesh;

            mesh = SimpleExtruder.Extrude(mesh, outline, 0.05f);
            Vector2[] uvs2 = new Vector2[mesh.vertices.Length];
            for (int i = 0; i < uvs2.Length; i++)
            {
                uvs2[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
            }
            mesh.uv = uvs2;
            mesh.RecalculateBounds();
            outline.GetComponent<MeshFilter>().sharedMesh = mesh;
            Vector3 position = outline.transform.position;
			position.y = -0.01f;
			outline.transform.localPosition = position;

			outline.GetComponent<Renderer>().material = material;

			return outline;
		}

		#endregion

		#region POLYGON UTILS

		public GameObject CreateRoof (){

			GameObject roof = new GameObject();
			MeshFilter filter = roof.AddComponent<MeshFilter>();
			roof.AddComponent(typeof(MeshRenderer));
			filter.mesh = mesh2D;
			return roof;
		}

		public static string VectorListToString (List<Vector3> list) {

			list =  new HashSet<Vector3>(list).ToList();
			string s = "";
			foreach (Vector3 v in list) {
				s += v.ToString() + " ";
			}
			return s;

		}

		#endregion

    }




}
