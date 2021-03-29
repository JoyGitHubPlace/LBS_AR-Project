using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GOLineMesh {
	

	private bool isLoop = false;
	public bool smoothEdges = false;

	public float width;
	public Mesh mesh;

	private List<Vector3> geometry;
	private Vector3[] vertices = new Vector3[0];
	private int[] triangles = new int[0];
	private Vector2[] uvs = new Vector2[0];

	private MeshFilter filter;

	#region CONSTRUCTOR

	public GOLineMesh (List<Vector3> geometry_) {
		geometry = geometry_;
	}

	#endregion

	#region LOADERS

	public void load (GameObject go) {

		filter = go.GetComponent<MeshFilter> ();
		if (filter == null) {
			filter = (MeshFilter)go.AddComponent(typeof(MeshFilter));
		}

		MeshRenderer renderer = go.GetComponent<MeshRenderer> ();
		if (renderer == null) {
			renderer = (MeshRenderer)go.AddComponent(typeof(MeshRenderer));
		}

		UpdateVertices();

		filter.sharedMesh = CreateMesh();
	}

	public Mesh CreateMesh()
	{

		mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;

		Vector2[] uvs = new Vector2[vertices.ToArray().Length];

		for (int i=0; i < uvs.Length; i++) {
			uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
		}
		mesh.uv = uvs;



		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		return mesh;
	}

	#endregion

	#region BUILDERS

	private void UpdateVertices() {
		
		if (geometry.Count < 2) return; // minimum to make a line

		int count = geometry.Count - 1;

		isLoop = geometry [0].Equals (geometry [count]);

		vertices = new Vector3[count*4];
		triangles = new int[(geometry.Count-1)*6];
		uvs = new Vector2[count*4];

		List <Vector3> dirs = new List<Vector3> ();
		List <Vector3> tans = new List<Vector3> ();
 
		Vector3 tanVect = Vector3.down;


		for (int p = 0; p<geometry.Count; p++)
		{
			Vector3 dir;
			Vector3 tangent;

			if (p == 0) // First 
			{
				if (isLoop) {
					dir = (geometry[p+1] - geometry[p]).normalized; 
					Vector3 dirBefore = (geometry [p] - geometry [geometry.Count-2]).normalized;
					tangent =  Vector3.Cross( tanVect,(dirBefore + dir) * 0.5f ).normalized;
				} 
				else {
					dir = (geometry[p+1] - geometry[p]).normalized; 
					tangent = Vector3.Cross( tanVect, dir).normalized;
				}
			}

			else if (p != geometry.Count-1) // Middles
			{
				dir = (geometry[p+1] - geometry[p]).normalized; 
				Vector3 dirBefore = (geometry [p] - geometry [p-1]).normalized;
				tangent =  Vector3.Cross( tanVect,(dirBefore + dir) * 0.5f ).normalized;
			}

			else // Last
			{
				if (isLoop) {
					dir = (geometry[1] - geometry[p]).normalized; 
					Vector3 dirBefore = (geometry [p] - geometry [p-1]).normalized;
					tangent =  Vector3.Cross( tanVect,(dirBefore + dir) * 0.5f ).normalized;

				} else {
					dir = (geometry [p] - geometry [p-1]).normalized;
					tangent = Vector3.Cross( tanVect, dir).normalized;
				}

			}

			dirs.Add (dir);
			tans.Add (tangent);

		}
			

		for (int i = 0; i<count; i++)
		{
			vertices[(i*4)+0] = geometry[i] + (tans[i] * (width));
			vertices[(i*4)+1] = geometry[i] - (tans[i] * (width));
			vertices[(i*4)+2] = geometry[i+1] + (tans[i+1] * (width));
			vertices[(i*4)+3] = geometry[i+1] - (tans[i+1] * (width));

			Vector2 offsetRight = new Vector2 (1,Vector3.Distance(vertices[(i*4)+1],vertices[(i*4)+3] )); // Green - Blue
			Vector2 offsetLeft = new Vector2 (1,Vector3.Distance(vertices[(i*4)+0],vertices[(i*4)+2] )); 	// Red _ Yellow

			uvs [(i * 4) + 0] = Vector2.zero;
			uvs [(i * 4) + 1] = Vector2.right;
			uvs [(i * 4) + 2] = Vector2.Scale(Vector2.up , offsetLeft);
			uvs [(i * 4) + 3] = Vector2.Scale(Vector2.one, offsetRight);

			triangles[(i*6)+0] = (i*4)+0;
			triangles[(i*6)+1] = (i*4)+2;
			triangles[(i*6)+2] = (i*4)+1;
			triangles[(i*6)+3] = (i*4)+2;
			triangles[(i*6)+4] = (i*4)+3;
			triangles[(i*6)+5] = (i*4)+1;

		}

		if (filter) {
			for (int i = 0; i < vertices.Length; i++) {
				vertices [i] = filter.transform.InverseTransformPoint (vertices [i]);
			}
		}

	}
		
	#endregion

}

