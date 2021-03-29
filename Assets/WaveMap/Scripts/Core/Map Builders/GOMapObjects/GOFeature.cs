using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;
using System;
using Mapbox.VectorTile.Geometry;


namespace WaveMap
{
    [System.Serializable]
    public class GOFeature
    {

        public string name;
        //		public string kind;
        public GOFeatureKind kind = GOFeatureKind.baseKind;
        public string detail;
        public string type;
        public float sort;
        public float y;
        public float height;
        public Int64 index;
        public GameObject roof;
        public GOFeatureType goFeatureType;
        public enum GOFeatureType
        {
            Line,
            MultiLine,
            Polygon,
            MultiPolygon
        }

        [HideInInspector]
        public IList geometry;
        [HideInInspector]
        public IList clips;

        public List<Vector3> convertedGeometry;
        public IDictionary properties;
        public List<KeyValue> attributes;

        [HideInInspector]
        public GameObject parent;
        [HideInInspector]
        public GOLayer layer;

        bool defaultRendering = true;

        #region BUILDERS

        public void ConvertGeometries()
        {

            convertedGeometry = CoordsToVerts(geometry);

        }

        public void ConvertAttributes()
        {

            List<KeyValue> list = new List<KeyValue>();
            foreach (string key in properties.Keys)
            {
                KeyValue keyValue = new KeyValue();
                keyValue.key = key;
                if (properties[key] != null)
                {
                    keyValue.value = properties[key].ToString();
                }
                list.Add(keyValue);
            }
            attributes = list;
        }

        public virtual IEnumerator BuildFeature(GOTile tile, bool delayedLoad)
        {
            if (type == null)
            {
                Debug.Log("type is null");
                return null;
            }
            try
            {
                if (goFeatureType == GOFeatureType.Line || goFeatureType == GOFeatureType.MultiLine)
                {
                    if (properties.Contains("min_zoom"))
                    {
                        if (float.Parse(properties["min_zoom"].ToString()) < 14f)
                        {
                            return CreateLine(tile, delayedLoad);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    // return null;
                    return CreatePolygon(tile, delayedLoad);
                    /* if (properties.Contains("min_zoom"))
                     {
                         if (float.Parse(properties["min_zoom"].ToString()) < 15f)
                         {
                             return CreatePolygon(tile, delayedLoad);
                         }
                         else
                         {
                             return null;
                         }
                     }
                     else
                     {
                         return null;
                     }*/

                }
            }
            catch (Exception ex)
            {
                Debug.Log("[GOFeature] Catched exception: " + ex);
                return null;
            }
        }

        public virtual IEnumerator CreateLine(GOTile tile, bool delayedLoad)
        {
            GORenderingOptions renderingOptions = GetRenderingOptions();

            if (renderingOptions.lineWidth == 0)
            {
                yield break;
            }

            GameObject line = new GameObject(name != null ? name : kind.ToString());
            line.transform.parent = parent.transform;

            //Layer mask
            if (layer.useLayerMask == true)
            {
                tile.AddObjectToLayerMask(layer, line);
            }

            GOFeatureMeshBuilder builder = new GOFeatureMeshBuilder(this);
            builder.BuildLine(line, layer, renderingOptions, tile.map);

            GOFeatureBehaviour fb = line.AddComponent<GOFeatureBehaviour>();
            fb.goFeature = this;

            if (layer.layerType == GOLayer.GOLayerType.Roads && name != null && name.Length > 0 && renderingOptions.useStreetNames)
            {
                GOStreetName streetName = new GameObject().AddComponent<GOStreetName>();
                streetName.gameObject.name = name + "_streetname";
                streetName.transform.SetParent(line.transform);
                yield return tile.StartCoroutine(streetName.Build(name));
            }

            if (layer.OnFeatureLoad != null)
            {
                layer.OnFeatureLoad.Invoke(builder.mesh, layer, kind, builder.center);
            }

            if (delayedLoad)
                yield return null;
        }

        public virtual IEnumerator CreatePolygon(GOTile tile, bool delayedLoad)
        {

            GameObject polygon = null;
            GORenderingOptions renderingOptions = GetRenderingOptions();
            GOFeatureMeshBuilder builder = new GOFeatureMeshBuilder(this);

            //Materials
            Material material = tile.GetMaterial(renderingOptions, builder.center);
            Material roofMat = renderingOptions.roofMaterial == null ? renderingOptions.material : renderingOptions.roofMaterial;

            if (sort != 0)
            {
                if (material)
                    material.renderQueue = -(int)sort;
                if (roofMat)
                    roofMat.renderQueue = -(int)sort;
            }

            //Group buildings by center coordinates
            if (layer.layerType == GOLayer.GOLayerType.Buildings && defaultRendering)
            {
                GameObject centerContainer = tile.findNearestCenter(builder.center, parent, material);
                parent = centerContainer;
                material = centerContainer.GetComponent<GOMatHolder>().material;
            }

            int offset = 0;
            if (!layer.useRealHeight)
            {
                height = renderingOptions.polygonHeight;
            }


            if (height == 0f)
            {
                height = 0.05f;
            }
            polygon = builder.BuildPolygon(name, layer, height + offset);
            polygon.GetComponent<Renderer>().material = material;

            //}
            if (polygon == null)
                yield break;
            if (name == "")
            {
                name = "某大楼";
            }
            polygon.name = name;

            polygon.transform.parent = parent.transform;

            //Layer mask
            if (layer.useLayerMask == true)
            {
                tile.AddObjectToLayerMask(layer, polygon);
            }

            if (renderingOptions.tag.Length > 0)
            {
                polygon.tag = renderingOptions.tag;
            }
            Vector3 pos = polygon.transform.localPosition;
            if (layer.useRealHeight && roofMat != null)
            {

                roof = builder.CreateRoof();
                roof.name = "roof";
                roof.transform.parent = polygon.transform;
                roof.GetComponent<MeshRenderer>().material = roofMat;
                roof.transform.position = new Vector3(roof.transform.position.x, height + 0.01f, roof.transform.position.z);
                roof.tag = polygon.tag;
                roof.layer = polygon.layer;
                if (y < 1f)
                {
                    y = 0.02f;
                }
            }

            if (height > 1f)
            {
                polygon.AddComponent<BuildingCollider>().InitRoot(roof);
            }
            /* if (y > 0.45f)
             {
                 y = 0.01f;
             }
             else*/
            if (y == 0.2f || y == 0.096f || this.kind == GOFeatureKind.commercial || this.kind == GOFeatureKind.recreation_ground || this.kind == GOFeatureKind.attraction)
            {
                y = 0.02f;
            }
            else if (y == 0.098f)
            {
                y = 0.03f;
            }
            // y = y * Global.tilesizeRank;
            pos.y = y;
            //polygon.transform.position = pos;
            polygon.transform.localPosition = pos;

            GOFeatureBehaviour fb = polygon.AddComponent<GOFeatureBehaviour>();
            fb.goFeature = this;
            if (layer.OnFeatureLoad != null)
            {
                layer.OnFeatureLoad.Invoke(builder.mesh, layer, kind, builder.center);
            }

            if (delayedLoad)
                yield return null;

        }

        #endregion

        #region UTILS

        public GORenderingOptions GetRenderingOptions()
        {
            GORenderingOptions renderingOptions = layer.defaultRendering;
            foreach (GORenderingOptions r in layer.renderingOptions)
            {
                if (r.kind == kind)
                {
                    defaultRendering = false;
                    renderingOptions = r;
                    break;
                }
            }
            return renderingOptions;
        }

        public static List<Vector3> CoordsToVerts(IList geometry)
        {

            var convertedGeometry = new List<Vector3>();
            //			for (int i = 0; i < geometry.Count - 1; i++) Just why?
            for (int i = 0; i < geometry.Count; i++)
            {
                if (geometry.GetType() == typeof(List<LatLng>))
                { //Mapbox 
                    LatLng c = (LatLng)geometry[i];
                    Coordinates coords = new Coordinates(c.Lat, c.Lng, 0);
                    Vector3 p = coords.convertCoordinateToVector();
                    convertedGeometry.Add(p);
                }
                else
                { //Mapzen
                    IList c = (IList)geometry[i];
                    Coordinates coords = new Coordinates((double)c[1], (double)c[0], 0);
                    convertedGeometry.Add(coords.convertCoordinateToVector());
                }
            }
            return convertedGeometry;
        }

        public static bool IsGeoPolygonClockwise(IList coords)
        {
            return (IsClockwise(GOFeature.CoordsToVerts(coords)));

        }

        public static bool IsClockwise(IList<Vector3> vertices)
        {
            double sum = 0.0;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v1 = vertices[i];
                Vector3 v2 = vertices[(i + 1) % vertices.Count];
                sum += (v2.x - v1.x) * (v2.z + v1.z);
            }
            return sum > 0.0;
        }


        #endregion
    }

    [System.Serializable]
    public class KeyValue
    {
        public string key;
        public string value;
    }
}