using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCollider : MonoBehaviour
{
    int m_TextureType = 1;
    MeshCollider m_Collider;
    private Material CurMaterial;
    Material m;
    Material rootm;
    private GameObject roof;
    void Start()
    {
        rootm = Resources.Load<Material>("Material/RoofMaterial");
    }
    // Use this for initialization
    public void init(int textureType)
    {
        m_TextureType = textureType;
        //		gameObject.AddComponent<MeshCollider>();
        m_Collider = gameObject.GetComponent<MeshCollider>();
        m_Collider.convex = true;
        m_Collider.isTrigger = true;
    }
    public void InitRoot(GameObject r)
    {
        roof = r;
        CurMaterial = this.gameObject.GetComponent<Renderer>().material;
        m = Resources.Load<Material>("Material/BuildingAlpha" + m_TextureType);
    }
        void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Self")
        {
            return;
        }
        
        GetComponent<MeshRenderer>().material = m;
        if (roof!= null)
        {
            roof.GetComponent<MeshRenderer>().material = m;
        }
        

    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag != "Self")
        {
            return;
        }

        GetComponent<MeshRenderer>().material = m;
        if (roof != null)
        {
            roof.GetComponent<MeshRenderer>().material = m;
        }


    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Self")
        {
            return;
        }
        //Material m;
        //Material m = Resources.Load<Material>("Material/" + LanduseKind);
        //if (m == null)
        //{
        //	m = Resources.Load<Material>("Material/Building"+ m_TextureType);
        //}
        GetComponent<MeshRenderer>().material = CurMaterial;
        if (roof != null)
        {
            roof.GetComponent<MeshRenderer>().material = rootm;
        }
    }
}