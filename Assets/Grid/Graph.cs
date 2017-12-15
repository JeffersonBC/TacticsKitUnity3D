using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class SquareGridGraph {
	public Vertex [] Vertices;

	public int Width;	//X coordinate
	public int Height;  //Z coordinate

	public List<Vector3> Path;

	public List<int> walkableBorder 	= new List<int>();
	public HashSet<int> searchedTiles 	= new HashSet<int>();

	#region Initializer
	public SquareGridGraph(int x, int y){
		Vertices = new Vertex[x*y];

		Width = x;
		Height = y;

		walkableBorder 	= new List<int>();
		searchedTiles	= new HashSet<int>();

		//Initializing
		for (int i = 0; i < x; i++){
			for (int j = 0; j < y; j++){
				Vertices[j*x + i] = new Vertex();

				Vertices[j*x + i].up 	= ((j < y-1) ? true : false);
				Vertices[j*x + i].right = ((i < x-1) ? true : false);
				Vertices[j*x + i].down 	= ((j > 0) ? true : false);
				Vertices[j*x + i].left 	= ((i > 0) ? true : false);
			}
		}
	}
	#endregion

	#region Dijkstra Search

	//Performs Dijkstra's Algorithm in the whole grid
	#region SearchFrom(int p0)
	public void SearchFrom(int p0){	//Dijkstra Algorithm
		List<Pair> Q = new List<Pair>() ;

		//Set the start point's distance to 0 and previous vertex to -1
		//Set every other vertex's distance to 0 as infinity
		for (int i = 0; i < Width; i++){
			for(int j = 0; j < Height; j++){
				if (j*Width + i == p0){ 
					Vertices[j*Width + i].DistanceFrom0 = 0;
					Vertices[j*Width + i].PreviousVertex = -1;
				}

				else Vertices[j*Width + i].DistanceFrom0 = Mathf.Infinity;

				Q.Add(new Pair(Vertices[j*Width + i].DistanceFrom0, j*Width + i));
			}
		}

		//While there are vertices in Q, take the one in there that is the closest to the starting point
		//and recalculate the distance to its adjacent vertices
		while(Q.Count > 0){
			Q.Sort(CompareByValue);

			int u = Q[0].Value; //Vertices[u] = Current Node
			Q.RemoveAt(0);

			int z = -1;			//Vertices[z] = Opposite Node

			for (int i = 0; i < 4; i++){
				if((i == 0 && Vertices[u].up) ||
				   (i == 1 && Vertices[u].down) ||
				   (i == 2 && Vertices[u].left) ||
				   (i == 3 && Vertices[u].right) ){

					switch (i){
					case 0: z = u + Width; break;
					case 1: z = u - Width; break;
					case 2: z = u - 1; break;
					case 3: z = u + 1; break;
					}

					if (Vertices[z].DistanceFrom0 > Vertices[u].DistanceFrom0 + Vertices[z].TerrainCost){
						Q.Remove(new Pair(Vertices[z].DistanceFrom0,z) );

						Vertices[z].DistanceFrom0 = Vertices[u].DistanceFrom0 + Vertices[z].TerrainCost;
						Vertices[z].PreviousVertex = u;

						Q.Add(new Pair(Vertices[z].DistanceFrom0,z) );
					}
				}
			}
			
		}

	}
	#endregion

	//Performs Dijkstra's Algorithm within a certain distance from p0
	#region SearchFrom(int p0, int MaxDistance)
	public void SearchFrom(int p0, int MaxDistance){
		List<Pair> Q = new List<Pair>() ;

		int p0X = p0 % Width;
		int p0Z = p0 / Width;

		//Set the start point's distance to 0 and previous vertex to -1
		//Set every other vertex's distance to 0 to infinity
		for (int i = 0; i < Width*Height; i++){
			if (i != p0){
				Vertices[i].DistanceFrom0 = Mathf.Infinity;	}
			
			else {
				Vertices[i].DistanceFrom0 = 0;
				Vertices[i].PreviousVertex = -1;	}

			//Only add to Q tiles that are possible to be within MaxDistance
			if( (int)Mathf.Abs(i%Width - p0X) + (int)Mathf.Abs(i/Width - p0Z) <= MaxDistance ){
				Q.Add(new Pair(Vertices[i].DistanceFrom0, i));

				//Debug.Log("Vertice " + i.ToString() + " distance to " + p0.ToString() 
				//          + " is: " + ((int)Mathf.Abs(i%Width - p0X) + (int)Mathf.Abs(i/Width - p0Z)).ToString() );
			}
		}
		
		//While there are vertices in Q, take the one in there that is the closest to the starting point
		//and recalculate the distance to its adjacent vertices
		while(Q.Count > 0){
			Q.Sort(CompareByValue);
			
			int u = Q[0].Value; //Vertices[u] = Current Node
			Q.RemoveAt(0);

			int z = -1;			//Vertices[z] = Opposite Node

			if(Vertices[u].DistanceFrom0 < MaxDistance){
				for (int i = 0; i < 4; i++){
					if((i == 0 && Vertices[u].up) ||
					   (i == 1 && Vertices[u].down) ||
					   (i == 2 && Vertices[u].left) ||
					   (i == 3 && Vertices[u].right) ){
						
						switch (i){
						case 0: z = u + Width; break;
						case 1: z = u - Width; break;
						case 2: z = u - 1; break;
						case 3: z = u + 1; break;
						}
						
						if (Vertices[z].DistanceFrom0 > Vertices[u].DistanceFrom0 + Vertices[z].TerrainCost){
							Q.Remove(new Pair(Vertices[z].DistanceFrom0,z) );

							Vertices[z].DistanceFrom0 = Vertices[u].DistanceFrom0 + Vertices[z].TerrainCost;
							Vertices[z].PreviousVertex = u;
							
							Q.Add(new Pair(Vertices[z].DistanceFrom0,z) );
						}
					}
				}
			}
		}

	}
	#endregion

	//Performs Dijkstra's Algorithm within a certain distance from mutilple p0s
	#region SearchFromMultiple
	public void SearchFromMultiple(List<int> p0s, int MaxDistance){
		searchedTiles = new HashSet<int>();

		foreach (int p0 in p0s){
			List<Pair> Q = new List<Pair>();
			
			int p0X = p0 % Width;
			int p0Z = p0 / Width;
			
			//Set the start point's distance to 0 and previous vertex to -1
			//Set every other vertex's distance to 0 to infinity
			for (int i = 0; i < Width*Height; i++){
				if (i != p0){
					Vertices[i].DistanceFrom0 = Mathf.Infinity;	}
				
				else {
					Vertices[i].DistanceFrom0 = 0;
					Vertices[i].PreviousVertex = -1;	}
				
				//Only add to Q tiles that are possible to be within MaxDistance
				if( (int)Mathf.Abs(i%Width - p0X) + (int)Mathf.Abs(i/Width - p0Z) <= MaxDistance ){
					Q.Add(new Pair(Vertices[i].DistanceFrom0, i));
				}
			}
			
			//While there are vertices in Q, take the one in there that is the closest to the starting point
			//and recalculate the distance to its adjacent vertices
			while(Q.Count > 0){
				Q.Sort(CompareByValue);
				
				int u = Q[0].Value; //Vertices[u] = Current Node
				Q.RemoveAt(0);
				
				int z = -1;			//Vertices[z] = Opposite Node

				if(Vertices[u].DistanceFrom0 <= MaxDistance){
					searchedTiles.Add(u);

					for (int i = 0; i < 4; i++){
						if((i == 0 && Vertices[u].up) ||
						   (i == 1 && Vertices[u].down) ||
						   (i == 2 && Vertices[u].left) ||
						   (i == 3 && Vertices[u].right) ){
							
							switch (i){
							case 0: z = u + Width; break;
							case 1: z = u - Width; break;
							case 2: z = u - 1; break;
							case 3: z = u + 1; break;
							}
							
							if (Vertices[z].DistanceFrom0 > Vertices[u].DistanceFrom0 + Vertices[z].TerrainCost){
								Q.Remove(new Pair(Vertices[z].DistanceFrom0,z) );
								
								Vertices[z].DistanceFrom0 = Vertices[u].DistanceFrom0 + Vertices[z].TerrainCost;
								Vertices[z].PreviousVertex = u;
								
								Q.Add(new Pair(Vertices[z].DistanceFrom0,z) );
							}
						}
					}
				}				
			}

		}
	}
	#endregion

	public void CalculatePathTo (int endPoint){
		int n = endPoint;
		Path = new List<Vector3>();
		Grid grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();

		while (n != -1) {
			Path.Insert(0, grid.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[n*5 + 4]);
			n = Vertices[n].PreviousVertex; }

		Path.RemoveAt(0);
	}

	public int CompareByValue (Pair x, Pair y){
		return x.Value.CompareTo(y.Value);
	}

	#endregion

	#region Editing node
	public void MakeUnwalkable (int VertexNumber){
		Vertices[VertexNumber].walkable = false;

		Vertices[VertexNumber].up 		= false;
		Vertices[VertexNumber].right 	= false;
		Vertices[VertexNumber].down 	= false;
		Vertices[VertexNumber].left 	= false;

        if (VertexNumber + Width < Height * Width) 	Vertices[VertexNumber + Width].down = false;
		if (VertexNumber - Width > 0) 				Vertices[VertexNumber - Width].up   = false;

		if (VertexNumber % Width != 0 && VertexNumber > 0)							Vertices[VertexNumber - 1].right = false;
		if ((VertexNumber+1) % Width != 0 && VertexNumber < (Height * Width) - 1)	Vertices[VertexNumber + 1].left  = false;
	}

	public void MakeWalkable (int VertexNumber){
		Vertices[VertexNumber].walkable = true;

		//To Up Tile
		if (VertexNumber + Width < Height * Width && Vertices[VertexNumber + Width].walkable) {
			Vertices[VertexNumber + Width].down = true;
			Vertices[VertexNumber].up 			= true;	}

		//To Down Tile
		if (VertexNumber - Width > 0 && Vertices[VertexNumber - Width].walkable) {
			Vertices[VertexNumber - Width].up = true;
			Vertices[VertexNumber].down 	  = true;	}

		//To Left Tile
		if (VertexNumber % Width != 0 && VertexNumber > 0 && Vertices[VertexNumber - 1].walkable) {
			Vertices[VertexNumber - 1].right = true;
			Vertices[VertexNumber].left 	 = true;	}

		//To Right Tile
		if ((VertexNumber+1) % Width != 0 && VertexNumber < (Height * Width) - 1 && Vertices[VertexNumber + 1].walkable) {
			Vertices[VertexNumber + 1].left  = true;
			Vertices[VertexNumber].right 	 = true; 	}
	}
	#endregion

	#region PathFromStartPoint
	public Stack<int> PathFromStartPoint (int endPoint){
		int n = endPoint;
		Stack<int> Path = new Stack<int>();

		while (n != -1) {
			Path.Push(n);
			n = Vertices[n].PreviousVertex;
		}

		return Path;
	}
	#endregion
}

#region Supporting Data Structures
[Serializable]
public class Vertex {
	public int TerrainCost;
	public float DistanceFrom0;
	public int PreviousVertex;

	public bool up, down, left, right;

	public bool walkable;

	public bool occupied;
	public bool traversable;
	public VertexState state;

	public Vertex(){
		TerrainCost 	= 1;
		DistanceFrom0  	= Mathf.Infinity;
		PreviousVertex 	= -1;

		walkable 			= true;

		occupied			= false;
		traversable			= true;
		state				= VertexState.unpainted;
	}
}

[Serializable]
public enum VertexState{
	unpainted,
	paintedAsWalkable,
	paintedAsAttackable
}

[Serializable]
public class Pair {
	public float Key;
	public int Value;
	
	public Pair(float key, int value){
		Key = key;
		Value = value;
	}
}
#endregion