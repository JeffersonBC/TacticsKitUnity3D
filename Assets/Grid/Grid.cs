using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider) ) ]
[System.Serializable]
public class Grid : MonoBehaviour {

	public int X, Z;
	public Vector2 Grid0	 = Vector2.zero;
		
	public float snap 	 	 = 0.25f;
	public int   Edition 	 = 0;
	public int 	 EditionType = 1;

	public float tileWidthX = 1f; //Not yet implemented
	public float tileWidthZ = 1f; //Not yet implemented

	public Color32	[] colors;

	public SquareGridGraph 	pathGraph;
	public Vector3[] 		PathLines; //REMOVE AS SOON AS POSSIBLE!!!

	void Awake() {
		if (pathGraph == null){
			Debug.Log("Null graph");
			pathGraph = new SquareGridGraph(X,Z);
		}

		UnpaintAllFaces();
	}

	public Vector3[] CalculateNormals (int size){ // (X*Y*5)
		Vector3 [] normals = new Vector3[size];

		for (int i = 0; i < size; i++) {
			normals[i] = Vector3.up;	}
		
		return normals;
	}

	public Vector2[] CalculateUV(int size){		// (X*Y)
		Vector2 [] uv = new Vector2[size*5];
		
		for (int i = 0; i < size; i++) {
			uv[i*5]   = new Vector2 (0f ,0f);	
			uv[i*5+1] = new Vector2 (1f ,0f);
			uv[i*5+2] = new Vector2 (0f ,1f);
			uv[i*5+3] = new Vector2 (1f ,1f);
			uv[i*5+4] = new Vector2 (.5f,.5f);
		}
		
		return uv;
	}

	public Vector2[] CalculateUV2(int size){	// (X*Y*5)
		Vector2 [] uv = new Vector2[size];

		for (int j = 0; j < Z; j++) {
			for (int i = 0; i < X; i++) {
				uv[(j*X + i)*5] 	= new Vector2 ( (float)i 		/ X	, (float)j 		/ Z);
				uv[(j*X + i)*5 + 1] = new Vector2 ( (float)(i+1) 	/ X	, (float)j 		/ Z);
				uv[(j*X + i)*5 + 2] = new Vector2 ( (float)i 		/ X	, (float)(j+1) 	/ Z);
				uv[(j*X + i)*5 + 3] = new Vector2 ( (float)(i+1) 	/ X	, (float)(j+1)	/ Z);
				uv[(j*X + i)*5 + 4] = new Vector2 ( (float)(i+.5f) 	/ X	, (float)(j+.5f)/ Z);

			}
		}

		return uv;
	}

	public int[] CalculateTriangles(int X, int Z){	// (X,Y)
		int[] triangles = new int[X*Z*4*3];
		
		for (int j = 0; j < Z; j++) {
			for (int i = 0; i < X; i++) {
				triangles[0 + (j*X + i)*12] = (j*X + i)*5; 	 //0		2 -----	3
				triangles[1 + (j*X + i)*12] = (j*X + i)*5 + 4; //4		|	4	|
				triangles[2 + (j*X + i)*12] = (j*X + i)*5 + 1; //1		0 ----- 1
				
				triangles[3 + (j*X + i)*12] = (j*X + i)*5 + 1; //1
				triangles[4 + (j*X + i)*12] = (j*X + i)*5 + 4; //4
				triangles[5 + (j*X + i)*12] = (j*X + i)*5 + 3; //3
				
				triangles[6 + (j*X + i)*12] = (j*X + i)*5 + 3; //3
				triangles[7 + (j*X + i)*12] = (j*X + i)*5 + 4; //4
				triangles[8 + (j*X + i)*12] = (j*X + i)*5 + 2; //2
				
				triangles[9 + (j*X + i)*12] = (j*X + i)*5 + 2; //2
				triangles[10+ (j*X + i)*12] = (j*X + i)*5 + 4; //4
				triangles[11+ (j*X + i)*12] = (j*X + i)*5;	 //0
			}			
		}
		
		return triangles;
	}

	#region Paint functions
	//Paints a single tile with the given color
	public void PaintFace(int faceNumber, Color32 color){
		for (int n = 0; n < 5; n++)
			colors[faceNumber * 5 + n] = color;			
	}

	//Paints all tiles within a distance from the calculated start point
	public void PaintFaces(int DistanceFromP0, Color32 color){
		for (int i = 0; i < X*Z; i++){
			if (pathGraph.Vertices[i].DistanceFrom0 <= DistanceFromP0)
				PaintFace(i, color);
		}
	}

	//Paints all tiles within a maximum anda minumum distance from the calculated start point
	public void PaintFaces(int MaxDistanceFromP0, int MinDistanceFromP0, Color32 color){
		for (int i = 0; i < X*Z; i++){
			if (pathGraph.Vertices[i].DistanceFrom0 <= MaxDistanceFromP0 &&
			    pathGraph.Vertices[i].DistanceFrom0 >= MinDistanceFromP0)
				PaintFace(i, color);
		}
	}

	//Paints walkable tiles and sets them as 'paintedAsWalkable'
	public void PaintWalkableFaces(int P0, int MaxDistanceFromP0, Color32 color){
		for (int i = 0; i < X*Z; i++){
			if (pathGraph.Vertices[i].DistanceFrom0 <= MaxDistanceFromP0 &&
			    pathGraph.Vertices[i].DistanceFrom0 >= 1 &&
			    !pathGraph.Vertices[i].occupied){

				for (int n = 0; n < 5; n++)
					colors[i * 5 + n] = color;
				
				pathGraph.Vertices[i].state = VertexState.paintedAsWalkable;

				//Calculating walkable border
				if (pathGraph.Vertices[i].DistanceFrom0 == MaxDistanceFromP0) pathGraph.walkableBorder.Add(i);

				if (!pathGraph.Vertices[i].up) 			pathGraph.walkableBorder.Add(i);
				else if (!pathGraph.Vertices[i].down) 	pathGraph.walkableBorder.Add(i);
				else if (!pathGraph.Vertices[i].left) 	pathGraph.walkableBorder.Add(i);
				else if (!pathGraph.Vertices[i].right)	pathGraph.walkableBorder.Add(i);

			}
		}

		pathGraph.walkableBorder.Add(P0);
	}

	//Paints attackable tiles
	public void PaintAttackableFaces(int MaxDistanceFromP0, Color32 color){
		//Search which tiles are withing the units attack range from the border of
		//the walkable tiles
		pathGraph.SearchFromMultiple(pathGraph.walkableBorder, MaxDistanceFromP0);

		foreach (int i in pathGraph.searchedTiles){
			//traversable = there's no enemy
			//occupied    = in this case, there's an ally
			if (pathGraph.Vertices[i].state != VertexState.paintedAsWalkable && (
			    (!pathGraph.Vertices[i].traversable	|| !pathGraph.Vertices[i].occupied) 	||
				(pathGraph.Vertices[i].traversable	|| pathGraph.Vertices[i].occupied)	)	)	{
			
				for (int n = 0; n < 5; n++)
					colors[i * 5 + n] = color;

				pathGraph.Vertices[i].state = VertexState.paintedAsAttackable;
			}
		}

	}

	//Paints all tiles in the graph with the color (0.3f,0.3f,0.3f,0.3f)
	public void UnpaintAllFaces(){
		for (int i = 0; i < X*Z; i++){
			if (pathGraph.Vertices[i].walkable || !pathGraph.Vertices[i].traversable){
				PaintFace(i, new Color32 (76,76,76,76) );

				pathGraph.Vertices[i].state = VertexState.unpainted;
			}
		}

		GetComponent<MeshFilter>().sharedMesh.colors32 = colors;
		pathGraph.walkableBorder.Clear();
	}

	//Need to be called after a Paint function
	public void AssignColorToMesh(){
		if (GetComponent<MeshFilter>().sharedMesh != null)
			GetComponent<MeshFilter>().sharedMesh.colors32 = colors;

		else
			Debug.LogError ("Grid Mesh is null");
	}
	#endregion
}
