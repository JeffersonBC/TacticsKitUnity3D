using UnityEngine;
using UnityEditor;
using System.Collections;

public class PathUtility {

	public static Material lineMaterial;


	public static void DrawGraph(Object target){
		//Grid grid = (Grid)target;

		DrawWalkablePath(target);
		DrawPathToP0(target);
		GraphDebug(target);
	}


	public static void UpdatePathLines(Object target){
		Grid grid = (Grid)target;

		grid.PathLines = new Vector3[grid.X * grid.Z * 5];

		Vector3 up = Vector3.up * 0.05f;

		for (int i = 0; i < grid.X; i++){
			for (int j = 0; j < grid.Z; j++){
				/*Middle*/ 	grid.PathLines[(j*grid.X + i)*5 + 4] =  GridEditorUtility.vertices[(j*grid.X + i)*5 + 4] + up;

				/*Up*/ 		grid.PathLines[(j*grid.X + i)*5 + 3] = 
					(GridEditorUtility.vertices[(j*grid.X + i)*5 + 2] + GridEditorUtility.vertices[(j*grid.X + i)*5 + 3])/2f + up; //   3
				/*Right*/ 	grid.PathLines[(j*grid.X + i)*5 + 1] = 
					(GridEditorUtility.vertices[(j*grid.X + i)*5 + 1] + GridEditorUtility.vertices[(j*grid.X + i)*5 + 3])/2f + up; //2  4  1
				/*Down*/ 	grid.PathLines[(j*grid.X + i)*5 + 0] = 
					(GridEditorUtility.vertices[(j*grid.X + i)*5 + 0] + GridEditorUtility.vertices[(j*grid.X + i)*5 + 1])/2f + up; //   0
				/*Left*/ 	grid.PathLines[(j*grid.X + i)*5 + 2] = 
					(GridEditorUtility.vertices[(j*grid.X + i)*5 + 0] + GridEditorUtility.vertices[(j*grid.X + i)*5 + 2])/2f + up; //
			}
		}
	}

	public static void EditingGraph(Object target){
		Grid grid = (Grid)target;

		if (grid.pathGraph == null){
			Debug.Log("Null graph");
			grid.pathGraph = new SquareGridGraph(grid.X, grid.Z);
			UpdatePathLines(target);
		}

		#region Nodes
		if (grid.Edition == 4){


			if(lineMaterial == null){
				if (grid.GetComponent<Renderer>().sharedMaterial != null)
					lineMaterial = grid.GetComponent<Renderer>().sharedMaterial;
				else
					return;
			}

			Texture tex = grid.GetComponent<Renderer>().sharedMaterial.mainTexture;
			grid.GetComponent<Renderer>().sharedMaterial.mainTexture = null;

			lineMaterial.SetPass(0);		
			GL.Begin(GL.LINES);
			
			GL.Color(Color.green);
			
			
			for (int i = 0; i < grid.X*grid.Z; i++){
				//Up
				if (grid.pathGraph.Vertices[i].up){
					GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
					GL.Vertex3(grid.PathLines[i*5 +3].x, grid.PathLines[i*5 +3].y, grid.PathLines[i*5 +3].z);	}
				//Right
				if (grid.pathGraph.Vertices[i].right){
					GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
					GL.Vertex3(grid.PathLines[i*5 +1].x, grid.PathLines[i*5 +1].y, grid.PathLines[i*5 +1].z);	}
				//Down
				if (grid.pathGraph.Vertices[i].down){
					GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
					GL.Vertex3(grid.PathLines[i*5 +0].x, grid.PathLines[i*5 +0].y, grid.PathLines[i*5 +0].z);	}
				//Left
				if (grid.pathGraph.Vertices[i].left){
					GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
					GL.Vertex3(grid.PathLines[i*5 +2].x, grid.PathLines[i*5 +2].y, grid.PathLines[i*5 +2].z);	}
				
			}
			
			GL.End();

			grid.GetComponent<Renderer>().sharedMaterial.mainTexture = tex;
			
			for (int i = 0; i < grid.X*grid.Z; i++){
				if (grid.pathGraph.Vertices[i].walkable){
					Handles.color = Color.green;
					if(Handles.Button(grid.PathLines[i*5 +4], Quaternion.identity, 0.15f, 0.15f, Handles.SphereCap) ){
						grid.pathGraph.MakeUnwalkable(i);	
						grid.PaintFace(i, new Color (0,0,0,0));	}	}
				
				else {
					Handles.color = Color.gray;
					if(Handles.Button(grid.PathLines[i*5 +4], Quaternion.identity, 0.15f, 0.15f, Handles.SphereCap) ) {
						grid.pathGraph.MakeWalkable(i);
						grid.PaintFace(i, new Color32 (76,76,76,76) ); }	}
			}
			
		}
		#endregion
		
		#region Edges
		if (grid.Edition == 5){
			
			for (int i = 0; i < grid.X*grid.Z; i++){
				//Up
				if (grid.pathGraph.Vertices[i].up){
					Handles.color = Color.green;
					if(Handles.Button(grid.PathLines[i*5 +4] + Vector3.forward*0.2f, Quaternion.identity, 0.25f, 0.10f, Handles.ArrowCap) ){
						grid.pathGraph.Vertices[i].up = false; }	}
				else{
					Handles.color = Color.gray;
					if(Handles.Button(grid.PathLines[i*5 +4] + Vector3.forward*0.2f, Quaternion.identity, 0.25f, 0.10f, Handles.ArrowCap) ){
						grid.pathGraph.Vertices[i].up = true; }		}
				
				//Right
				if (grid.pathGraph.Vertices[i].right){
					Handles.color = Color.green;
					if(Handles.Button(grid.PathLines[i*5 +4] + Vector3.right*0.2f, Quaternion.Euler(0,90,0), 0.25f, 0.10f, Handles.ArrowCap) ){
						grid.pathGraph.Vertices[i].right = false; }	}
				else{
					Handles.color = Color.gray;
					if(Handles.Button(grid.PathLines[i*5 +4] + Vector3.right*0.2f, Quaternion.Euler(0,90,0), 0.25f, 0.10f, Handles.ArrowCap) ){
						grid.pathGraph.Vertices[i].right = true; }		}
				
				//Down
				if (grid.pathGraph.Vertices[i].down){
					Handles.color = Color.green;
					if(Handles.Button(grid.PathLines[i*5 +4] + Vector3.back*0.2f, Quaternion.Euler(0,180,0), 0.25f, 0.10f, Handles.ArrowCap) ){
						grid.pathGraph.Vertices[i].down = false; }	}
				else{
					Handles.color = Color.gray;
					if(Handles.Button(grid.PathLines[i*5 +4] + Vector3.back*0.2f, Quaternion.Euler(0,180,0), 0.25f, 0.10f, Handles.ArrowCap) ){
						grid.pathGraph.Vertices[i].down = true; }		}
				
				//Left
				if (grid.pathGraph.Vertices[i].left){
					Handles.color = Color.green;
					if(Handles.Button(grid.PathLines[i*5 +4] + Vector3.left*0.2f, Quaternion.Euler(0,270,0), 0.25f, 0.15f, Handles.ArrowCap) ){
						grid.pathGraph.Vertices[i].left = false; }	}
				else{
					Handles.color = Color.gray;
					if(Handles.Button(grid.PathLines[i*5 +4] + Vector3.left*0.2f, Quaternion.Euler(0,270,0), 0.25f, 0.15f, Handles.ArrowCap) ){
						grid.pathGraph.Vertices[i].left = true; }		}
				
			}				
		}	
		#endregion
		
		#region Weight - to be implemented
		
		#endregion
	}


	public static void DrawWalkablePath(Object target){
		Grid grid = (Grid)target;

		if(lineMaterial == null){
			if (grid.GetComponent<Renderer>().sharedMaterial != null)
				lineMaterial = grid.GetComponent<Renderer>().sharedMaterial;
			else
				return;
		}

		Texture tex = grid.GetComponent<Renderer>().sharedMaterial.mainTexture;
		grid.GetComponent<Renderer>().sharedMaterial.mainTexture = null;

		lineMaterial.SetPass(0);		
		GL.Begin(GL.LINES);

		GL.Color(Color.green);


		for (int i = 0; i < grid.X*grid.Z; i++){

			//Up
			if (grid.pathGraph.Vertices[i].up){
				GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
				GL.Vertex3(grid.PathLines[i*5 +3].x, grid.PathLines[i*5 +3].y, grid.PathLines[i*5 +3].z);	}
			//Right
			if (grid.pathGraph.Vertices[i].right){
				GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
				GL.Vertex3(grid.PathLines[i*5 +1].x, grid.PathLines[i*5 +1].y, grid.PathLines[i*5 +1].z);	}
			//Down
			if (grid.pathGraph.Vertices[i].down){
				GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
				GL.Vertex3(grid.PathLines[i*5 +0].x, grid.PathLines[i*5 +0].y, grid.PathLines[i*5 +0].z);	}
			//Left
			if (grid.pathGraph.Vertices[i].left){
				GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
				GL.Vertex3(grid.PathLines[i*5 +2].x, grid.PathLines[i*5 +2].y, grid.PathLines[i*5 +2].z);	}
			
		}

		GL.End();

		grid.GetComponent<Renderer>().sharedMaterial.mainTexture = tex;
	}

	public static void DrawPathToP0(Object target){
		Grid grid = (Grid)target;
		
		if(lineMaterial == null) {
			if (grid.GetComponent<Renderer>().sharedMaterial != null)
				lineMaterial = grid.GetComponent<Renderer>().sharedMaterial;
			else
				return;
		}

		Texture tex = grid.GetComponent<Renderer>().sharedMaterial.mainTexture;
		grid.GetComponent<Renderer>().sharedMaterial.mainTexture = null;

		lineMaterial.SetPass(0);		
		GL.Begin(GL.LINES);
		
		GL.Color(Color.green);
		
		
		for (int i = 0; i < grid.X*grid.Z; i++){
			//Up
			if (i < grid.X * (grid.Z-1))
				if (grid.pathGraph.Vertices[i].PreviousVertex == i + grid.X	||
				    grid.pathGraph.Vertices[i + grid.X].PreviousVertex == i ){
					GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
					GL.Vertex3(grid.PathLines[i*5 +3].x, grid.PathLines[i*5 +3].y, grid.PathLines[i*5 +3].z);	}

			//Right
			if (i % grid.X != grid.X - 1)
				if (grid.pathGraph.Vertices[i].PreviousVertex == i + 1 ||
					grid.pathGraph.Vertices[i + 1].PreviousVertex == i ){
					GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
					GL.Vertex3(grid.PathLines[i*5 +1].x, grid.PathLines[i*5 +1].y, grid.PathLines[i*5 +1].z);	}

			//Down
			if (i >= grid.X)
				if (grid.pathGraph.Vertices[i].PreviousVertex == i - grid.X	||
				    grid.pathGraph.Vertices[i - grid.X].PreviousVertex == i ){	
					GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
					GL.Vertex3(grid.PathLines[i*5 +0].x, grid.PathLines[i*5 +0].y, grid.PathLines[i*5 +0].z);	}

			//Left
			if (i % grid.X != 0)
				if (grid.pathGraph.Vertices[i].PreviousVertex == i - 1 ||
				    grid.pathGraph.Vertices[i - 1].PreviousVertex == i ){
					GL.Vertex3(grid.PathLines[i*5 +4].x, grid.PathLines[i*5 +4].y, grid.PathLines[i*5 +4].z);
					GL.Vertex3(grid.PathLines[i*5 +2].x, grid.PathLines[i*5 +2].y, grid.PathLines[i*5 +2].z);	}
			
			
		}
		
		GL.End();

		grid.GetComponent<Renderer>().sharedMaterial.mainTexture = tex;
	}

	public static void GraphDebug(Object target){
		Grid grid = (Grid)target;

		if (!EditorApplication.isPlaying){
			for (int i = 0; i < grid.X; i++){
				for (int j = 0; j < grid.Z; j++){
					Handles.Label(GridEditorUtility.vertices[(j*grid.X + i)*5 + 4], 

					              j*grid.X + i + ") \n"
					              //+ grid.pathGraph.Vertices[j*grid.X + i].DistanceFrom0.ToString()
					              + (grid.pathGraph.Vertices[j*grid.X + i].state == VertexState.paintedAsWalkable 	? "Walkable \n" 	: "")
					              + (grid.pathGraph.Vertices[j*grid.X + i].state == VertexState.paintedAsAttackable ? "Attackable \n" 	: "")
					              + (grid.pathGraph.Vertices[j*grid.X + i].traversable ? "" : "Not Traversable\n")
					              + (grid.pathGraph.Vertices[j*grid.X + i].occupied ? "Occupied\n" : "")
					              );

					//if (grid.pathGraph.walkableBorder != null){
					//	Handles.Label(grid.vertices[(j*grid.X + i)*5 + 4], 
					//	              (grid.pathGraph.walkableBorder.Contains(j*grid.X + i)) ? "Border" : " ");
					//}

					//if (grid.pathGraph.searchedTiles != null){
					//	Handles.Label(grid.vertices[(j*grid.X + i)*5 + 4], 
					//	              (grid.pathGraph.searchedTiles.Contains(j*grid.X + i)) ? "Searched" : " ");
					//}



				}
			}
		}
	}
}
