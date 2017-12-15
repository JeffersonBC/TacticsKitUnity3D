using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
	
public class GridEditorUtility {

	public static HashSet<int> 	SelectedVertices 	= new HashSet<int>();
	public static HashSet<int> 	SelectedHandles		= new HashSet<int>();
	public static HashSet<int> 	SelectedFaces		= new HashSet<int>();

	public static Vector3[] 	vertices;

	public static Vector2 MousePos0 	= Vector2.zero;
	public static Vector2 MousePosPrev 	= Vector2.zero;
	public static Vector2 MousePosF 	= Vector2.zero;

	public static bool IsMouseDown		= false;

	public static Shader[] TempShader;

	public static Rect 		SelectionBox = new Rect(0,0,0,0);

	public static int  		handleI, handleJ, handleK;
	public static bool		isH;

	public static Vector3 	HandlePos0, HandlePosF;
	public static float		HandleDelta = 0;
	static int 				HandleHash 	= "HandleHash".GetHashCode();

	public static Stack<VertexUndo> MeshUndo = new Stack<VertexUndo>();
	public static Stack<VertexUndo> MeshRedo = new Stack<VertexUndo>();

	#region New/ Increase/ Decreas Grid
	public static void NewGrid(Object target){
		Grid grid = (Grid)target;
				
		ResizeArrays(grid, grid.X,grid.Z);

		NewVertices(grid);
		AssignColors(grid);

		grid.pathGraph = new SquareGridGraph(grid.X,grid.Z);
		PathUtility.UpdatePathLines(target);

		MeshUndo.Clear();
		MeshRedo.Clear();

		SelectedVertices.Clear();
		SelectedHandles.Clear();
		SelectedFaces.Clear();

	}

	public static void IncreaseGrid(Object target, int Direction){
		Grid grid = (Grid)target;

		/*switch(Direction){
		case 1: // =>
			Undo.RecordObject(target, "Increase Grid Size > ");
			grid.X++;
			ResizeArrays(grid, grid.X,grid.Z);
			IncreaseRight(grid);
			break;

		case 2: // <=
			Undo.RecordObject(target, "Increase Grid Size < ");
			grid.X++;
			ResizeArrays(grid, grid.X,grid.Z);
			IncreaseLeft(grid);
			break;

		case 3: // /\
			Undo.RecordObject(target, "Increase Grid Size Up ");
			grid.Z++;
			ResizeArrays(grid, grid.X,grid.Z);
			IncreaseUp(grid);
			break;

		case 4: // V
			Undo.RecordObject(target, "Increase Grid Size V ");
			grid.Z++;
			ResizeArrays(grid, grid.X,grid.Z);
			IncreaseDown(grid);
			break;

		}*/

		AssignColors(grid);
		
		grid.pathGraph = new SquareGridGraph(grid.X,grid.Z);
		PathUtility.UpdatePathLines(target);
		
		EditorUtility.SetDirty (target);

	}

	public static void DecreaseGrid(Object target, int Direction){
		Grid grid = (Grid)target;
		
		/*switch(Direction){
		case 1: // =>
			Undo.RecordObject(target, "Decrease Grid Size > ");
			grid.X--;
			ResizeArrays(grid, grid.X,grid.Z);
			DecreaseRight(grid);
			break;
			
		case 2: // <=
			Undo.RecordObject(target, "Decrease Grid Size < ");
			grid.X--;
			ResizeArrays(grid, grid.X,grid.Z);
			DecreaseLeft(grid);
			break;

		case 3: // /\
			Undo.RecordObject(target, "Decrease Grid Size Up ");
			grid.Z--;
			ResizeArrays(grid, grid.X,grid.Z);
			DecreaseUp(grid);
			break;
				
		case 4: // V
			Undo.RecordObject(target, "Decrease Grid Size V ");
			grid.Z--;
			ResizeArrays(grid, grid.X,grid.Z);
			DecreaseDown(grid);
			break;
		}*/
		
		AssignColors(grid);

		grid.pathGraph = new SquareGridGraph(grid.X,grid.Z);
		PathUtility.UpdatePathLines(target);

		EditorUtility.SetDirty (target);
		
	}
	#endregion

	#region EditingGrid
	public static void EditingGrid(Object target){

		Grid grid = (Grid)target;

		#region Not editing
		if (grid.Edition == 0) return;
		#endregion

		Vector3 oldPos;
				
		int id = GUIUtility.GetControlID(HandleHash, FocusType.Passive);
		Vector3 screenPosition;

		switch (Event.current.GetTypeForControl(id)){

		case EventType.MouseDown:
			if (Event.current.button == 0){
				IsMouseDown = true;

				MousePos0 	= Event.current.mousePosition;
				MousePos0.y = Camera.current.pixelHeight - MousePos0.y;

				MousePosF = MousePos0;

				#region Vertices
				if (grid.Edition == 1 && grid.EditionType == 1){
					for (int j = 0; j <= grid.Z && grid.Z > 0; j++) {
						for (int i = 0; i <= grid.X && grid.X > 0; i++) {
							
							oldPos = OldPosVertice(target,i,j);
							screenPosition = Handles.matrix.MultiplyPoint(oldPos);

							if (HandleUtility.DistanceToCircle(screenPosition, .05f) < 5){
								
								if (!SelectedHandles.Contains(j*(grid.X+1)+i)) AddVertice(target,i,j);

								handleI = i;
								handleJ = j;
								
								HandlePos0 		= oldPos;
								HandlePosF		= oldPos;
								HandleDelta		= 0;
																
								GUIUtility.hotControl = id;

								Event.current.Use();
								EditorGUIUtility.SetWantsMouseJumping(1);

								return;
							}
						}			
					}
				}
				#endregion

				#region Edges
				else if (grid.Edition == 2 && grid.EditionType == 1){
					//Vertical edges	(i,j) -> (i,j+1)
					for (int j = 0; j < grid.Z; j++) {
						for (int i = 0; i < grid.X+1; i++) {

							oldPos = OldPosEdgeV(target,i,j);
							screenPosition = Handles.matrix.MultiplyPoint(oldPos);

							if (HandleUtility.DistanceToCircle(screenPosition, .05f) < 5){
								
								if (!SelectedHandles.Contains(j*(grid.X+1)+i)) AddEdgeV(target,i,j);
								
								handleI = i;
								handleJ = j;
								
								HandlePos0 		= oldPos;
								HandlePosF		= oldPos;
								HandleDelta		= 0;
								isH				= false;
								
								GUIUtility.hotControl = id;
								
								Event.current.Use();
								EditorGUIUtility.SetWantsMouseJumping(1);
								
								return;
							}
						}
					}
					
					//Horizontal edges	(i,j) -> (i+1,j)
					for (int j = 0; j < grid.Z+1; j++) {
						for (int i = 0; i < grid.X; i++) {

							oldPos = OldPosEdgeH(target,i,j);
							screenPosition = Handles.matrix.MultiplyPoint(oldPos);

							if (HandleUtility.DistanceToCircle(screenPosition, .05f) < 5){
								
								if (!SelectedHandles.Contains(j*(grid.X+1)+i)) AddEdgeH(target,i,j);
								
								handleI = i;
								handleJ = j;
								
								HandlePos0 		= oldPos;
								HandlePosF		= oldPos;
								HandleDelta		= 0;
								isH				= true;
								
								GUIUtility.hotControl = id;
								
								Event.current.Use();
								EditorGUIUtility.SetWantsMouseJumping(1);
								
								return;
							}

						}
					}
				}
				#endregion

				#region Faces
				else if (grid.Edition == 3 && grid.EditionType == 1){					
					for (int j = 0; j < grid.Z; j++) {
						for (int i = 0; i < grid.X; i++) {

							oldPos = OldPosFace(target,i,j);
							screenPosition = Handles.matrix.MultiplyPoint(oldPos);
							
							if (HandleUtility.DistanceToCircle(screenPosition, .05f) < 5){
								
								if (!SelectedHandles.Contains(j*(grid.X+1)+i)) AddFace(target,i,j);
								
								handleI = i;
								handleJ = j;
								
								HandlePos0 		= oldPos;
								HandlePosF		= oldPos;
								HandleDelta		= 0;
								
								GUIUtility.hotControl = id;
								
								Event.current.Use();
								EditorGUIUtility.SetWantsMouseJumping(1);
								
								return;
							}

						}
					}
				}
				#endregion

				#region Separate Vertices
				else if (grid.Edition == 1 && grid.EditionType == 2){
					
					for (int j = 0; j < grid.Z; j++) {
						for (int i = 0; i < grid.X; i++) {
							for(int k = 0; k < 4; k++) {

								oldPos = OldPosSepVertice(target,i,j,k);
								screenPosition = Handles.matrix.MultiplyPoint(oldPos);
								
								if (HandleUtility.DistanceToCircle(screenPosition, .05f) < 5){
									
									if (!SelectedHandles.Contains(j*(grid.X+1)+i)) AddSeparateVertice(target,i,j,k);
									
									handleI = i; 
									handleJ = j;
									handleK = k;

									HandlePos0 		= oldPos;
									HandlePosF		= oldPos;
									HandleDelta		= 0;
									
									GUIUtility.hotControl = id;
									
									Event.current.Use();
									EditorGUIUtility.SetWantsMouseJumping(1);
									
									return;
								}
							}
						}
					}
				}
				#endregion

				#region Separate Edges
				else if (grid.Edition == 2 && grid.EditionType == 2){
					
					//Vertical edges	(i,j) -> (i,j+1)
					for (int j = 0; j < grid.Z ; j++) {
						for (int i = 0; i < grid.X; i++) {
							for (int k = 0; k < 2; k++){
								
								oldPos = OldPosSepEdgeV(target,i,j,k);						
								screenPosition = Handles.matrix.MultiplyPoint(oldPos);

								if (HandleUtility.DistanceToCircle(screenPosition, .05f) < 5){
									
									if (!SelectedHandles.Contains(j*(grid.X+1)+i)) AddSeparateEdgeV(target,i,j,k);
									
									handleI = i;
									handleJ = j;
									handleK = k;
									
									HandlePos0 		= oldPos;
									HandlePosF		= oldPos;
									HandleDelta		= 0;
									
									GUIUtility.hotControl = id;
									
									Event.current.Use();
									EditorGUIUtility.SetWantsMouseJumping(1);
									
									return;
								}
							}
						}
					}
					
					//Horizontal edges	(i,j) -> (i+1,j)
					for (int j = 0; j < grid.Z ; j++) {
						for (int i = 0; i < grid.X; i++) {
							for (int k = 0; k < 2; k++){

								oldPos = OldPosSepEdgeH(target,i,j,k);					
								screenPosition = Handles.matrix.MultiplyPoint(oldPos);

								if (HandleUtility.DistanceToCircle(screenPosition, .05f) < 5){
									
									if (!SelectedHandles.Contains(j*(grid.X+1)+i)) AddSeparateEdgeH(target,i,j,k);
									
									handleI = i;
									handleJ = j;
									handleK = k;
									
									HandlePos0 		= oldPos;
									HandlePosF		= oldPos;
									HandleDelta		= 0;
									
									GUIUtility.hotControl = id;
									
									Event.current.Use();
									EditorGUIUtility.SetWantsMouseJumping(1);
									
									return;
								}
							}
						}
					}
				
					
				}
				#endregion

				#region Separete Faces
				else if (grid.Edition == 3 && grid.EditionType == 2){
					for (int j = 0; j < grid.Z; j++) {
						for (int i = 0; i < grid.X; i++) {

							oldPos = OldPosSepFace(target,i,j);
							screenPosition = Handles.matrix.MultiplyPoint(oldPos);
							
							if (HandleUtility.DistanceToCircle(screenPosition, .05f) < 5){
								
								if (!SelectedHandles.Contains(j*(grid.X+1)+i)) AddSeparateFace(target,i,j);
								
								handleI = i;
								handleJ = j;
								
								HandlePos0 		= oldPos;
								HandlePosF		= oldPos;
								HandleDelta		= 0;
								
								GUIUtility.hotControl = id;
								
								Event.current.Use();
								EditorGUIUtility.SetWantsMouseJumping(1);
								
								return;
							}

						}
					}
				}
				#endregion

				IsMouseDown = false;
			}
			break;

		case EventType.MouseUp:
			if (GUIUtility.hotControl == id && Event.current.button == 0) {

				GUIUtility.hotControl = 0;
				Event.current.Use();
				EditorGUIUtility.SetWantsMouseJumping(0);

				MeshUndo.Push(new VertexUndo( SelectedVertices, SelectedFaces, HandleDelta) );
				MeshRedo.Clear();

				if (!SelectedHandles.Contains(handleJ*(grid.X+1)+handleI)){
					if (grid.Edition == 1 && grid.EditionType == 1)	RemoveVertice(target,handleI,handleJ);
					if (grid.Edition == 2 && grid.EditionType == 1){
						if (isH) 	RemoveEdgeH(target,handleI,handleJ);
						else 		RemoveEdgeV(target,handleI,handleJ);
					}
					if (grid.Edition == 3 && grid.EditionType == 1)	RemoveFace(target,handleI,handleJ);

					if (grid.Edition == 1 && grid.EditionType == 2)	RemoveSeparateVertice(target, handleI, handleJ, handleK);
					if (grid.Edition == 2 && grid.EditionType == 2) {
						if (!isH) 	RemoveSeparateEdgeV(target, handleI, handleJ, handleK);
					}
					if (grid.Edition == 3 && grid.EditionType == 2)	RemoveSeparateFace(target,handleI,handleJ);

				}

				if (!SelectedHandles.Contains((grid.X*grid.Z*2) + handleJ*(grid.X+1)+handleI))
					RemoveSeparateEdgeH(target, handleI, handleJ, handleK);


				HandleDelta		= 0;
				IsMouseDown 	= false;
			}
			break;			

		case EventType.MouseDrag:
			if (GUIUtility.hotControl == id && Event.current.button == 0){
				MousePosF += new Vector2(Event.current.delta.x, -Event.current.delta.y);
				
				Vector3 position2 = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(HandlePos0))
					+ (Vector3)(MousePosF - MousePos0);
				
				HandlePosF.y = Handles.matrix.inverse.MultiplyPoint(Camera.current.ScreenToWorldPoint(position2)).y;

				foreach (int n in SelectedVertices)	vertices[n].y += round((HandlePosF.y - HandlePos0.y) - HandleDelta, grid.snap);
				foreach (int n in SelectedFaces)	SmoothFace(n);

				HandleDelta = round(HandlePosF.y - HandlePos0.y, grid.snap);

				grid.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices 	= vertices;
				grid.gameObject.GetComponent<MeshCollider>().sharedMesh = 
					grid.gameObject.GetComponent<MeshFilter>().sharedMesh;


				GUI.changed = true;
				Event.current.Use();
			}
			break;

			#region Repaint
		case EventType.Repaint:
			if (grid.Edition == 1 && grid.EditionType == 1){
				for (int j = 0; j <= grid.Z && grid.Z > 0; j++) {
					for (int i = 0; i <= grid.X && grid.X > 0; i++) {
						if(!SelectedHandles.Contains(j*(grid.X+1)+i) ) 	Handles.color = new Color (0f, 1f, 0f, .5f);
						else 											Handles.color = new Color (1f, 0.92f, 0.016f, 1f);
						
						oldPos = OldPosVertice(target,i,j);
						Handles.DotCap(0,oldPos, Quaternion.identity, .05f); }}}

			else if (grid.Edition == 2 && grid.EditionType == 1){
				//Vertical edges	(i,j) -> (i,j+1)
				for (int j = 0; j < grid.Z; j++) {
					for (int i = 0; i < grid.X+1; i++) {
						if(!SelectedHandles.Contains(j*(grid.X+1)+i)) 	Handles.color = new Color (1f, 0f, 0f, .5f);
						else 											Handles.color = new Color (1f, 0.92f, 0.016f, .5f);

						oldPos = OldPosEdgeV(target,i,j);
						Handles.DotCap(0,oldPos, Quaternion.identity, .05f);}}

				//Horizontal edges	(i,j) -> (i+1,j)
				for (int j = 0; j < grid.Z+1; j++) {
					for (int i = 0; i < grid.X; i++) {
						if(!SelectedHandles.Contains( ((grid.X+1)*grid.Z) + j*(grid.X+1)+i )) 	Handles.color = new Color (1f, 0f, 0f, .5f);
						else 																	Handles.color = new Color (1f, 0.92f, 0.016f, .5f);
						
						oldPos = OldPosEdgeH(target,i,j);
						Handles.DotCap(0,oldPos, Quaternion.identity, .05f);	}}}

			else if (grid.Edition == 3 && grid.EditionType == 1){
				for (int j = 0; j < grid.Z; j++) {
					for (int i = 0; i < grid.X; i++) {
						
						if(!SelectedHandles.Contains(j*grid.X+i)) 	Handles.color = new Color (0,0,1,0.5f);
						else 										Handles.color = new Color (1f, 0.92f, 0.016f, .5f);
						
						oldPos = OldPosFace(target,i,j);
						Handles.DotCap(0,oldPos, Quaternion.identity, .05f);	}}}

			else if (grid.Edition == 1 && grid.EditionType == 2){
				for (int j = 0; j < grid.Z; j++) {
					for (int i = 0; i < grid.X; i++) {
						for(int k = 0; k < 4; k++) {

							if(!SelectedHandles.Contains((j*grid.X +i)*5 + k)) Handles.color = new Color (0f, .5f, 0f, .5f);
							else Handles.color = new Color (1f, 0.92f, 0.016f, .5f);
							
							oldPos = OldPosSepVertice(target,i,j,k);
							Handles.DotCap(0,oldPos, Quaternion.identity, .05f);	}}}}

			else if (grid.Edition == 2 && grid.EditionType == 2){
				//
				for (int j = 0; j < grid.Z ; j++) {
					for (int i = 0; i < grid.X; i++) {
						for (int k = 0; k < 2; k++){

							if(!SelectedHandles.Contains((j*grid.X +i)*5 + k)) 	Handles.color = Color.red*.5f;
							else 												Handles.color = new Color (1f, 0.92f, 0.016f, .5f);

							oldPos = OldPosSepEdgeV(target,i,j,k);
							Handles.DotCap(0,oldPos, Quaternion.identity, .05f);	}}}

				//
				for (int j = 0; j < grid.Z ; j++) {
					for (int i = 0; i < grid.X; i++) {
						for (int k = 0; k < 2; k++){
							if(!SelectedHandles.Contains((grid.X*grid.Z*2)*5 +(j*grid.X +i)*5 + k)) Handles.color = Color.red*.5f;
							else 																	Handles.color = new Color (1f, 0.92f, 0.016f, .5f);
							
							oldPos = OldPosSepEdgeH(target,i,j,k);
							Handles.DotCap(0,oldPos, Quaternion.identity, .05f);	}}}				
			}					

			else if (grid.Edition == 3 && grid.EditionType == 2){
				for (int j = 0; j < grid.Z; j++) {
					for (int i = 0; i < grid.X; i++) {
						if(!SelectedHandles.Contains(j*grid.X+i)) Handles.color = new Color (0,0,.5f,0.4f);
						else Handles.color = new Color (1f, 0.92f, 0.016f, .4f);
						
						oldPos = OldPosSepFace(target,i,j);
						Handles.DotCap(0,oldPos, Quaternion.identity, .05f);}}}




			break;

		}
			#endregion			
				
	}
	#endregion

	public static void Selecting (Object target){
		Grid grid = (Grid)target;
		
		#region Not selecting
		if (grid.Edition == 0) return;
		#endregion

		#region Selecting vertices
		if (grid.Edition == 1 && grid.EditionType == 1){ 
			Vector3 oldPos;

			for (int j = 0; j <= grid.Z && grid.Z > 0; j++) {
				for (int i = 0; i <= grid.X && grid.X > 0; i++) {
					if(!SelectedHandles.Contains(j*(grid.X+1)+i))
						 Handles.color = new Color (0f, 1f, 0f, .4f);
					else Handles.color = new Color (1f, 0.92f, 0.016f, .4f);

					oldPos = OldPosVertice(target,i,j);

					if (Handles.Button(oldPos, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap)){
						if ( !SelectedHandles.Contains(j*(grid.X+1)+i) ){
							SelectedHandles.Add(j*(grid.X+1)+i);
							AddVertice(target,i,j);		}

						else{
							SelectedHandles.Remove(j*(grid.X+1)+i);
							RemoveVertice(target,i,j);	}
					}									
				}
			}
		}
		#endregion

		#region Selecting egdes
		else if (grid.Edition == 2 && grid.EditionType == 1){
			Vector3 oldPos;

			//Vertical edges	(i,j) -> (i,j+1)
			for (int j = 0; j < grid.Z; j++) {
				for (int i = 0; i < grid.X+1; i++) {
					if(!SelectedHandles.Contains(j*(grid.X+1)+i)) Handles.color = new Color (1f, 0f, 0f, .4f);
					else Handles.color = new Color (1f, 0.92f, 0.016f, .4f);

					oldPos = OldPosEdgeV(target,i,j);

					if (Handles.Button(oldPos, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap)){
						if ( !SelectedHandles.Contains(j*(grid.X+1)+i) ){
							SelectedHandles.Add(j*(grid.X+1)+i);
							AddEdgeV(target,i,j);	}
						else{
							SelectedHandles.Remove(j*(grid.X+1)+i);
							RemoveEdgeV(target,i,j);	} } } }

			//Horizontal edges	(i,j) -> (i+1,j)
			for (int j = 0; j < grid.Z+1; j++) {
				for (int i = 0; i < grid.X; i++) {
					if(!SelectedHandles.Contains( ((grid.X+1)*grid.Z) + j*(grid.X+1)+i ))
						Handles.color = new Color (1f, .1f, 0f, .4f);
					else Handles.color = new Color (1f, 0.92f, 0.016f, .4f);
					
					oldPos = OldPosEdgeH(target,i,j);

					if (Handles.Button(oldPos, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap)){
						if ( !SelectedHandles.Contains( ((grid.X+1)*grid.Z) + j*(grid.X+1)+i) ){
							SelectedHandles.Add( ((grid.X+1)*grid.Z) + j*(grid.X+1)+i );
							AddEdgeH(target,i,j);	}
						else{
							SelectedHandles.Remove( ((grid.X+1)*grid.Z) + j*(grid.X+1)+i );
							RemoveEdgeH(target,i,j);	} 
					} } }
		}
		#endregion

		#region Selecting faces
		else if (grid.Edition == 3 && grid.EditionType == 1){
			Vector3 oldPos;

			for (int j = 0; j < grid.Z; j++) {
				for (int i = 0; i < grid.X; i++) {
					if(!SelectedHandles.Contains(j*grid.X+i)) Handles.color = new Color (0,0,1,0.4f);
					else Handles.color = new Color (1f, 0.92f, 0.016f, .4f);
					
					oldPos = vertices[(j*(grid.X) + i)*5 + 4];

					if (Handles.Button(oldPos, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap)){
						if ( !SelectedHandles.Contains(j*(grid.X) + i) ){
							SelectedHandles.Add(j*(grid.X) + i);
							AddFace(target,i,j);		}
						else{
							SelectedHandles.Remove(j*(grid.X) + i);
							RemoveFace(target,i,j);	}
					} } }
		}
		#endregion
	
		#region Selecting separate vertices
		else if (grid.Edition == 1 && grid.EditionType == 2){
			Vector3 oldPos;

			for (int j = 0; j < grid.Z; j++) {
				for (int i = 0; i < grid.X; i++) {
					for(int k = 0; k < 4; k++) {
						if(!SelectedHandles.Contains((j*grid.X +i)*5 + k)) Handles.color = new Color (0f, .5f, 0f, .5f);
						else Handles.color = new Color (1f, 0.92f, 0.016f, .5f);
						
						oldPos = OldPosSepVertice(target,i,j,k);

						if (Handles.Button(oldPos, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap)){
							if ( !SelectedHandles.Contains((j*grid.X +i)*5 + k) ){
								SelectedHandles.Add((j*grid.X +i)*5 + k);
								AddSeparateVertice(target,i,j,k);		}
							
							else{
								SelectedHandles.Remove((j*grid.X +i)*5 + k);
								RemoveSeparateVertice(target,i,j,k);	}
						}		
					}
				}
			}
		}
		#endregion

		#region Selecting separate egdes
		else if (grid.Edition == 2  && grid.EditionType == 2){
			Vector3 oldPos;

			//Vertical edges	(i,j) -> (i,j+1)
			for (int j = 0; j < grid.Z ; j++) {
				for (int i = 0; i < grid.X; i++) {
					for (int k = 0; k < 2; k++){						
						if(!SelectedHandles.Contains((j*grid.X +i)*5 + k)) Handles.color = Color.red*.5f;
						else Handles.color = new Color (1f, 0.92f, 0.016f, .5f);
						
						oldPos = OldPosSepEdgeV(target,i,j,k);	

						if (Handles.Button(oldPos, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap)){
							if ( !SelectedHandles.Contains((j*grid.X +i)*5 + k) ){
								SelectedHandles.Add((j*grid.X +i)*5 + k);
								AddSeparateEdgeV(target,i,j,k);	}
							else{
								SelectedHandles.Remove((j*grid.X +i)*5 + k);
								RemoveSeparateEdgeV(target,i,j,k);	}	}
					} } }


			for (int j = 0; j < grid.Z ; j++) {
				for (int i = 0; i < grid.X; i++) {
					for (int k = 0; k < 2; k++){
						if(!SelectedHandles.Contains((grid.X*grid.Z*2)*5 +(j*grid.X +i)*5 + k)) Handles.color = Color.red*.5f;
						else Handles.color = new Color (1f, 0.92f, 0.016f, .5f);
						
						oldPos = OldPosSepEdgeH(target,i,j,k);

						if (Handles.Button(oldPos, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap)){
							if ( !SelectedHandles.Contains((grid.X*grid.Z*2)*5 +(j*grid.X +i)*5 + k )){
								SelectedHandles.Add		  ((grid.X*grid.Z*2)*5 +(j*grid.X +i)*5 + k);
								AddSeparateEdgeH(target,i,j,k);	}
							else{
								SelectedHandles.Remove((grid.X*grid.Z*2)*5 +(j*grid.X +i)*5 + k);
								RemoveSeparateEdgeH(target,i,j,k);	}	}
					} } }


		}
		#endregion

		#region Selecting separate faces
		else if (grid.Edition == 3 && grid.EditionType == 2){
			Vector3 oldPos;
			
			for (int j = 0; j < grid.Z; j++) {
				for (int i = 0; i < grid.X; i++) {
					if(!SelectedHandles.Contains(j*grid.X+i)) Handles.color = new Color (0,0,.5f,0.4f);
					else Handles.color = new Color (1f, 0.92f, 0.016f, .4f);
					
					oldPos = vertices[(j*(grid.X) + i)*5 + 4];
					
					if (Handles.Button(oldPos, Quaternion.identity, 0.05f, 0.05f, Handles.DotCap)){
						if ( !SelectedHandles.Contains(j*(grid.X) + i) ){
							SelectedHandles.Add(j*(grid.X) + i);
							AddSeparateFace(target,i,j);		}
						else{
							SelectedHandles.Remove(j*(grid.X) + i);
							RemoveSeparateFace(target,i,j);	}
					} } }
		}

		#endregion

	}

	public static void DragSelecting(Object target){
		Grid grid = (Grid)target;

		SelectionBox = new Rect();
		HandleDelta  = 0;

		SelectionBox.xMin = Mathf.Min(MousePos0.x, MousePosF.x);
		SelectionBox.yMin = Mathf.Min(MousePos0.y, MousePosF.y);

		SelectionBox.xMax = Mathf.Max(MousePos0.x, MousePosF.x);
		SelectionBox.yMax = Mathf.Max(MousePos0.y, MousePosF.y);

		#region Selecting vertices
		if (grid.Edition == 1 && grid.EditionType == 1){ 
			Vector3 oldPos;
			
			for (int j = 0; j <= grid.Z && grid.Z > 0; j++) {
				for (int i = 0; i <= grid.X && grid.X > 0; i++) {
					oldPos = Camera.current.WorldToViewportPoint(OldPosVertice(target,i,j) );

					if (SelectionBox.Contains((Vector2)(oldPos)) ){
						if ( !SelectedHandles.Contains(j*(grid.X+1)+i) ){
							SelectedHandles.Add(j*(grid.X+1)+i);
							AddVertice(target,i,j);		}
						
						else{
							SelectedHandles.Remove(j*(grid.X+1)+i);
							RemoveVertice(target,i,j);	}
					}

				}
			}
		}
		#endregion

		#region Selecting edges
		else if (grid.Edition == 2 && grid.EditionType == 1){
			Vector3 oldPos;
			
			//Vertical edges	(i,j) -> (i,j+1)
			for (int j = 0; j < grid.Z; j++) {
				for (int i = 0; i < grid.X+1; i++) {

					oldPos = Camera.current.WorldToViewportPoint(OldPosEdgeV(target,i,j));
					
					if (SelectionBox.Contains((Vector2)(oldPos)) ){
						if ( !SelectedHandles.Contains(j*(grid.X+1)+i) ){
							SelectedHandles.Add(j*(grid.X+1)+i);
							AddEdgeV(target,i,j);	}
						else{
							SelectedHandles.Remove(j*(grid.X+1)+i);
							RemoveEdgeV(target,i,j);	} } } }
			
			//Horizontal edges	(i,j) -> (i+1,j)
			for (int j = 0; j < grid.Z+1; j++) {
				for (int i = 0; i < grid.X; i++) {

					oldPos = Camera.current.WorldToViewportPoint(OldPosEdgeH(target,i,j) );
					
					if (SelectionBox.Contains((Vector2)(oldPos)) ){
						if ( !SelectedHandles.Contains( ((grid.X+1)*grid.Z) + j*(grid.X+1)+i) ){
							SelectedHandles.Add( ((grid.X+1)*grid.Z) + j*(grid.X+1)+i );
							AddEdgeH(target,i,j);	}
						else{
							SelectedHandles.Remove( ((grid.X+1)*grid.Z) + j*(grid.X+1)+i );
							RemoveEdgeH(target,i,j);	} 
					} 
				} 
			}

		}	
		#endregion

		#region Selecting faces
		else if (grid.Edition == 3 && grid.EditionType == 1){
			Vector3 oldPos;
			
			for (int j = 0; j < grid.Z; j++) {
				for (int i = 0; i < grid.X; i++) {

					oldPos = Camera.current.WorldToViewportPoint(vertices[(j*(grid.X) + i)*5 + 4] );
					
					if (SelectionBox.Contains((Vector2)(oldPos)) ){
						if ( !SelectedHandles.Contains(j*(grid.X) + i) ){
							SelectedHandles.Add(j*(grid.X) + i);
							AddFace(target,i,j);		}
						else{
							SelectedHandles.Remove(j*(grid.X) + i);
							RemoveFace(target,i,j);	}
					} } }
		}
		#endregion

		#region Selecting separate vertices
		else if (grid.Edition == 1 && grid.EditionType == 2){
			Vector3 oldPos;
			
			for (int j = 0; j < grid.Z; j++) {
				for (int i = 0; i < grid.X; i++) {
					for(int k = 0; k < 4; k++) {
												
						oldPos = Camera.current.WorldToViewportPoint(OldPosSepVertice(target,i,j,k) );
						
						if (SelectionBox.Contains((Vector2)(oldPos)) ){
							if ( !SelectedHandles.Contains((j*grid.X +i)*5 + k) ){
								SelectedHandles.Add((j*grid.X +i)*5 + k);
								AddSeparateVertice(target,i,j,k);		}
							
							else{
								SelectedHandles.Remove((j*grid.X +i)*5 + k);
								RemoveSeparateVertice(target,i,j,k);	}
						}		
					}
				}
			}
		}
		#endregion
		
		#region Selecting separate egdes
		else if (grid.Edition == 2  && grid.EditionType == 2){
			Vector3 oldPos;
			
			//Vertical edges	(i,j) -> (i,j+1)
			for (int j = 0; j < grid.Z ; j++) {
				for (int i = 0; i < grid.X; i++) {
					for (int k = 0; k < 2; k++){						

						oldPos = Camera.current.WorldToViewportPoint(OldPosSepEdgeV(target,i,j,k) );	
						
						if (SelectionBox.Contains((Vector2)(oldPos)) ){
							if ( !SelectedHandles.Contains((j*grid.X +i)*5 + k) ){
								SelectedHandles.Add((j*grid.X +i)*5 + k);
								AddSeparateEdgeV(target,i,j,k);	}
							else{
								SelectedHandles.Remove((j*grid.X +i)*5 + k);
								RemoveSeparateEdgeV(target,i,j,k);	}	}
					} } }
			
			
			for (int j = 0; j < grid.Z ; j++) {
				for (int i = 0; i < grid.X; i++) {
					for (int k = 0; k < 2; k++){

						oldPos = Camera.current.WorldToViewportPoint(OldPosSepEdgeH(target,i,j,k) );
						
						if (SelectionBox.Contains((Vector2)(oldPos)) ){
							if ( !SelectedHandles.Contains((grid.X*grid.Z*2)*5 +(j*grid.X +i)*5 + k )){
								SelectedHandles.Add		  ((grid.X*grid.Z*2)*5 +(j*grid.X +i)*5 + k);
								AddSeparateEdgeH(target,i,j,k);	}
							else{
								SelectedHandles.Remove((grid.X*grid.Z*2)*5 +(j*grid.X +i)*5 + k);
								RemoveSeparateEdgeH(target,i,j,k);	}	}
					} } }
			
			
		}
		#endregion
		
		#region Selecting separate faces
		else if (grid.Edition == 3 && grid.EditionType == 2){
			Vector3 oldPos;
			
			for (int j = 0; j < grid.Z; j++) {
				for (int i = 0; i < grid.X; i++) {

					oldPos = Camera.current.WorldToViewportPoint(vertices[(j*(grid.X) + i)*5 + 4] );
					
					if (SelectionBox.Contains((Vector2)(oldPos)) ){
						if ( !SelectedHandles.Contains(j*(grid.X) + i) ){
							SelectedHandles.Add(j*(grid.X) + i);
							AddSeparateFace(target,i,j);		}
						else{
							SelectedHandles.Remove(j*(grid.X) + i);
							RemoveSeparateFace(target,i,j);	}
					} } }
		}
		
		#endregion

	}
	
	public static void DeselectAll(){
		SelectedVertices.Clear();
		SelectedHandles.Clear();
		SelectedFaces.Clear();
	}
	
	#region New Grid functions
	public static void ResizeArrays (Grid grid, int X, int Z){
		vertices  	= new Vector3 [X * Z * 5];
		grid.colors	= new Color32 [X * Z * 5];
	}

	public static void NewVertices(Grid grid){
		for (int j = 0; j < grid.Z; j++) {			//							
			for (int i = 0; i < grid.X; i++) {		// (j*(grid.X) + i) = face number							
				vertices [(j*(grid.X) + i)*5 	 ] = new Vector3 (i    *grid.tileWidthX	 , 0, j		 *grid.tileWidthZ);
				vertices [(j*(grid.X) + i)*5 + 1] = new Vector3 ((i+1)*grid.tileWidthX	 , 0, j		 *grid.tileWidthZ);
				vertices [(j*(grid.X) + i)*5 + 2] = new Vector3 (i	  *grid.tileWidthX	 , 0, (j+1)	 *grid.tileWidthZ);
				vertices [(j*(grid.X) + i)*5 + 3] = new Vector3 ((i+1)*grid.tileWidthX	 , 0, (j+1)  *grid.tileWidthZ);
				vertices [(j*(grid.X) + i)*5 + 4] = new Vector3 ((i+.5f)*grid.tileWidthX , 0, (j+.5f)*grid.tileWidthZ);
				
			}																	
		}	
	}

	public static void AssignColors(Grid grid){
		for (int j = 0; j < grid.Z; j++) {
			for (int i = 0; i < grid.X; i++) {
				for (int n = 0; n < 5; n++){
					grid.colors [(j*(grid.X) + i)*5 + n] = new Color32 (76,76,76,76);
				}
			}																	
		}
	}

	#endregion

	#region Selection functions

	#region Vertice
	public static Vector3 OldPosVertice(Object target, int i, int j){
		Grid grid = (Grid)target;
		Vector3 oldPos = new Vector3 ((i * grid.tileWidthX) + vertices[0].x, 
		                              0, 
		                              (j * grid.tileWidthZ) + vertices[0].z);

		if(j>0 && i>0)				oldPos.y += vertices[((j-1)*(grid.X) + (i-1))*5 + 3].y;
		if(j>0 && i<grid.X) 		oldPos.y += vertices[((j-1)*(grid.X) + i)*5 + 2].y;
		if(i>0 && j<grid.Z) 		oldPos.y += vertices[(j*(grid.X) + (i-1))*5 + 1].y ;	
		if(i<grid.X && j<grid.Z)	oldPos.y += vertices[(j*(grid.X) + i)*5].y;
		
		if( ((j==0 || j == grid.Z) && (i>0 && i<grid.X)) || 
		    ((i==0 || i == grid.X) && (j>0 && j<grid.Z)) )
			oldPos.y /= 2f;
		
		if ( (i>0 && i<grid.X) && (j>0 && j<grid.Z) ) 	oldPos.y /= 4f;

		return oldPos;
	}

	public static void AddVertice(Object target, int i, int j){
		Grid grid = (Grid)target;

		if(j>0 && i>0) 			{SelectedVertices.Add(((j-1)*(grid.X) + (i-1))*5 + 3); 	SelectedFaces.Add((j-1)*(grid.X) + (i-1));}
		if(j>0 && i<grid.X) 	{SelectedVertices.Add(((j-1)*(grid.X) + i)*5 + 2);	   	SelectedFaces.Add((j-1)*(grid.X) + i);}
		if(i>0 && j<grid.Z)		{SelectedVertices.Add((j*(grid.X) + (i-1))*5 + 1);	   	SelectedFaces.Add(j*(grid.X) + (i-1));}
		if(i<grid.X && j<grid.Z){SelectedVertices.Add((j*(grid.X) + i)*5);			   	SelectedFaces.Add(j*(grid.X) + i);}
	}

	public static void RemoveVertice(Object target, int i, int j){
		Grid grid = (Grid)target;

		if(j>0 && i>0) 			{SelectedVertices.Remove(((j-1)*(grid.X) + (i-1))*5 + 3);	SelectedFaces.Remove((j-1)*(grid.X) + (i-1));}
		if(j>0 && i<grid.X) 	{SelectedVertices.Remove(((j-1)*(grid.X) + i)*5 + 2); 		SelectedFaces.Remove((j-1)*(grid.X) + i);}
		if(i>0 && j<grid.Z)		{SelectedVertices.Remove((j*(grid.X) + (i-1))*5 + 1); 		SelectedFaces.Remove(j*(grid.X) + (i-1));}
		if(i<grid.X && j<grid.Z){SelectedVertices.Remove((j*(grid.X) + i)*5); 				SelectedFaces.Remove(j*(grid.X) + i);}		
	}
	#endregion

	#region Edge
	public static Vector3 OldPosEdgeV (Object target, int i, int j){
		Grid grid = (Grid)target;
		Vector3 oldPos = Vector3.zero;
		
		if (i < grid.X) {
			oldPos += vertices[(j*(grid.X) + i)*5 + 0];
			oldPos += vertices[(j*(grid.X) + i)*5 + 2]; }
		
		if (i > 0){
			oldPos += vertices[(j*(grid.X) + (i-1))*5 + 1]; 
			oldPos += vertices[(j*(grid.X) + (i-1))*5 + 3]; }
		
		if (i<grid.X && i>0) 	oldPos /= 4f;
		else 					oldPos /= 2f;

		return oldPos;
	}
	
	public static void AddEdgeV(Object target, int i, int j){
		Grid grid = (Grid)target;

		if(j>0 && i>0)				SelectedVertices.Add( ((j-1)*grid.X + (i-1))*5 + 3);
		if(j>0 && i<grid.X) 		SelectedVertices.Add( ((j-1)*grid.X + i)*5 	   + 2);
		if(i>0 )					SelectedVertices.Add( (j*grid.X 	+ (i-1))*5 + 1);
		if(i<grid.X)				SelectedVertices.Add( (j*grid.X 	+ i)*5 	   + 0);
		
		if(i>0)						SelectedVertices.Add( (j*grid.X 	+ (i-1))*5 + 3);
		if(i<grid.X+1 && i<grid.X)	SelectedVertices.Add( (j*grid.X 	+ i)*5     + 2);
		if(i>0 && j<grid.Z-1)		SelectedVertices.Add( ((j+1)*grid.X + (i-1))*5 + 1);
		if(j< grid.Z-1 && i<grid.X)	SelectedVertices.Add( ((j+1)*grid.X + i)*5 	   + 0);
		
		
		if (j>0 && i>0) 			SelectedFaces.Add((j-1)*grid.X + (i-1));
		if (j>0 && i<grid.X) 		SelectedFaces.Add((j-1)*grid.X + i);
		
		if (i>0) 					SelectedFaces.Add(j*grid.X + (i-1));
		if (i<grid.X) 				SelectedFaces.Add(j*grid.X + i);
		
		if (j<grid.Z-1 && i>0) 		SelectedFaces.Add((j+1)*grid.X + (i-1));
		if (j<grid.Z-1 && i<grid.X) SelectedFaces.Add((j+1)*grid.X + i);
	}
	
	public static void RemoveEdgeV(Object target, int i, int j){
		Grid grid = (Grid)target;

		if(j>0 && i>0)				SelectedVertices.Remove( ((j-1)*grid.X + (i-1))*5 + 3);
		if(j>0 && i<grid.X) 		SelectedVertices.Remove( ((j-1)*grid.X + i)*5 	   + 2);
		if(i>0 )					SelectedVertices.Remove( (j*grid.X 	+ (i-1))*5 + 1);
		if(i<grid.X)				SelectedVertices.Remove( (j*grid.X 	+ i)*5 	   + 0);
		
		if(i>0)						SelectedVertices.Remove( (j*grid.X 	+ (i-1))*5 + 3);
		if(i<grid.X+1 && i<grid.X)	SelectedVertices.Remove( (j*grid.X 	+ i)*5     + 2);
		if(i>0 && j<grid.Z-1)		SelectedVertices.Remove( ((j+1)*grid.X + (i-1))*5 + 1);
		if(j< grid.Z-1 && i<grid.X)	SelectedVertices.Remove( ((j+1)*grid.X + i)*5 	   + 0);
		
		
		if (j>0 && i>0) 			SelectedFaces.Remove((j-1)*grid.X + (i-1));
		if (j>0 && i<grid.X) 		SelectedFaces.Remove((j-1)*grid.X + i);
		
		if (i>0) 					SelectedFaces.Remove(j*grid.X + (i-1));
		if (i<grid.X) 				SelectedFaces.Remove(j*grid.X + i);
		
		if (j<grid.Z-1 && i>0) 		SelectedFaces.Remove((j+1)*grid.X + (i-1));
		if (j<grid.Z-1 && i<grid.X) SelectedFaces.Remove((j+1)*grid.X + i);
	}


	public static Vector3 OldPosEdgeH (Object target, int i, int j){
		Grid grid = (Grid)target;
		Vector3 oldPos = Vector3.zero;
		
		if(j > 0){
			oldPos += vertices[((j-1)*(grid.X) + i)*5 + 2];
			oldPos += vertices[((j-1)*(grid.X) + i)*5 + 3]; }
		
		if(j < grid.Z){
			oldPos += vertices[(j*(grid.X) + i)*5 + 0];
			oldPos += vertices[(j*(grid.X) + i)*5 + 1]; }
		
		if(j>0 && j<grid.Z) oldPos /= 4f;
		else oldPos /= 2f;

		return oldPos;
	}

	public static void AddEdgeH(Object target, int i, int j){
		Grid grid = (Grid)target;

		if(i > 0 && j > 0) 			SelectedVertices.Add(((j-1)*grid.X + (i-1))*5 + 3);
		if(j > 0) 					SelectedVertices.Add(((j-1)*grid.X + i    )*5 + 2);
		if(i>0 && j<grid.Z) 		SelectedVertices.Add((j   *grid.X 	+ (i-1))*5 + 1);
		if(j < grid.Z)				SelectedVertices.Add((j   *grid.X 	+ i    )*5 + 0);
		
		if(j > 0) 					SelectedVertices.Add(((j-1)*grid.X + i    )*5 + 3);
		if(j>0 && i<grid.X-1) 		SelectedVertices.Add(((j-1)*grid.X + (i+1))*5 + 2);
		if(j < grid.Z) 				SelectedVertices.Add((j*grid.X + i    )*5 + 1);
		if(j<grid.Z && i<grid.X-1)	SelectedVertices.Add((j*grid.X + (i+1))*5 + 0);
		
		
		if(i > 0 && j > 0)	SelectedFaces.Add((j-1)*grid.X + (i-1));
		if(i>0 && j<grid.Z)	SelectedFaces.Add(j	 *grid.X + (i-1));
		
		if(j > 0)			SelectedFaces.Add((j-1)*grid.X + i);
		if(j<grid.Z)		SelectedFaces.Add(j	 *grid.X + i);
		
		if(j>0 && i<grid.X-1)		SelectedFaces.Add((j-1)*grid.X + (i+1));
		if(i<grid.X-1 && j<grid.Z)	SelectedFaces.Add(j	 *grid.X + (i+1));
	}

	public static void RemoveEdgeH(Object target, int i, int j){
		Grid grid = (Grid)target;
		
		if(i > 0 && j > 0) 			SelectedVertices.Remove(((j-1)*grid.X + (i-1))*5 + 3);
		if(j > 0) 					SelectedVertices.Remove(((j-1)*grid.X + i    )*5 + 2);
		if(i>0 && j<grid.Z) 		SelectedVertices.Remove((j   *grid.X 	+ (i-1))*5 + 1);
		if(j < grid.Z)				SelectedVertices.Remove((j   *grid.X 	+ i    )*5 + 0);
		
		if(j > 0) 					SelectedVertices.Remove(((j-1)*grid.X + i    )*5 + 3);
		if(j>0 && i<grid.X-1) 		SelectedVertices.Remove(((j-1)*grid.X + (i+1))*5 + 2);
		if(j < grid.Z) 				SelectedVertices.Remove((j*grid.X + i    )*5 + 1);
		if(j<grid.Z && i<grid.X-1)	SelectedVertices.Remove((j*grid.X + (i+1))*5 + 0);
		
		
		if(i > 0 && j > 0)	SelectedFaces.Remove((j-1)*grid.X + (i-1));
		if(i>0 && j<grid.Z)	SelectedFaces.Remove(j	 *grid.X + (i-1));
		
		if(j > 0)			SelectedFaces.Remove((j-1)*grid.X + i);
		if(j<grid.Z)		SelectedFaces.Remove(j	 *grid.X + i);
		
		if(j>0 && i<grid.X-1)		SelectedFaces.Remove((j-1)*grid.X + (i+1));
		if(i<grid.X-1 && j<grid.Z)	SelectedFaces.Remove(j	 *grid.X + (i+1));
	}
	#endregion

	#region Face
	public static Vector3 OldPosFace(Object target, int i, int j){
		Grid grid = (Grid)target;

		return vertices[(j*(grid.X) + i)*5 + 4];
	}

	public static void AddFace(Object target, int i, int j){
		Grid grid = (Grid)target;

		for (int n = 0; n < 5; n ++)
			SelectedVertices.Add( (j*(grid.X) + i)*5 + n);
		
		if (i > 0){
			SelectedVertices.Add( (j*(grid.X) + (i-1))*5 + 1);
			SelectedVertices.Add( (j*(grid.X) + (i-1))*5 + 3);
			SelectedFaces.Add( j*(grid.X) + (i-1) );
			
			if (j > 0){
				SelectedVertices.Add( ((j-1)*(grid.X) + (i-1))*5 + 3);
				SelectedFaces.Add( (j-1)*(grid.X) + (i-1) );}
			if (j < (grid.Z-1)){
				SelectedVertices.Add( ((j+1)*(grid.X) + (i-1))*5 + 1);
				SelectedFaces.Add( (j+1)*(grid.X) + (i-1) );
			}
		}
		
		if (i < (grid.X-1)){
			SelectedVertices.Add( (j*(grid.X) + (i+1))*5 + 0);
			SelectedVertices.Add( (j*(grid.X) + (i+1))*5 + 2);
			SelectedFaces.Add( j*(grid.X) + (i+1) );
			
			if (j > 0){
				SelectedVertices.Add( ((j-1)*(grid.X) + (i+1))*5 + 2);
				SelectedFaces.Add( (j-1)*(grid.X) + (i+1) );}
			if (j < (grid.Z-1)){
				SelectedVertices.Add( ((j+1)*(grid.X) + (i+1))*5 + 0);
				SelectedFaces.Add( (j+1)*(grid.X) + (i+1) );
			}
		}
		
		if (j > 0){
			SelectedVertices.Add(((j-1)*(grid.X) + i)*5 + 2);
			SelectedVertices.Add(((j-1)*(grid.X) + i)*5 + 3);
			SelectedFaces.Add( (j-1)*(grid.X) + i );
		}
		if (j < (grid.Z-1)) {
			SelectedVertices.Add( ((j+1)*(grid.X) + i)*5 + 0);
			SelectedVertices.Add( ((j+1)*(grid.X) + i)*5 + 1);
			SelectedFaces.Add( (j+1)*(grid.X) + i );
		}
	}

	public static void RemoveFace(Object target, int i, int j){
		Grid grid = (Grid)target;
		
		for (int n = 0; n < 5; n ++)
			SelectedVertices.Remove( (j*(grid.X) + i)*5 + n);
		
		if (i > 0){
			SelectedVertices.Remove( (j*(grid.X) + (i-1))*5 + 1);
			SelectedVertices.Remove( (j*(grid.X) + (i-1))*5 + 3);
			SelectedFaces.Remove( j*(grid.X) + (i-1) );
			
			if (j > 0){
				SelectedVertices.Remove( ((j-1)*(grid.X) + (i-1))*5 + 3);
				SelectedFaces.Remove( (j-1)*(grid.X) + (i-1) );}
			if (j < (grid.Z-1)){
				SelectedVertices.Remove( ((j+1)*(grid.X) + (i-1))*5 + 1);
				SelectedFaces.Remove( (j+1)*(grid.X) + (i-1) );
			}
		}
		
		if (i < (grid.X-1)){
			SelectedVertices.Remove( (j*(grid.X) + (i+1))*5 + 0);
			SelectedVertices.Remove( (j*(grid.X) + (i+1))*5 + 2);
			SelectedFaces.Remove( j*(grid.X) + (i+1) );
			
			if (j > 0){
				SelectedVertices.Remove( ((j-1)*(grid.X) + (i+1))*5 + 2);
				SelectedFaces.Remove( (j-1)*(grid.X) + (i+1) );}
			if (j < (grid.Z-1)){
				SelectedVertices.Remove( ((j+1)*(grid.X) + (i+1))*5 + 0);
				SelectedFaces.Remove( (j+1)*(grid.X) + (i+1) );
			}
		}
		
		if (j > 0){
			SelectedVertices.Remove(((j-1)*(grid.X) + i)*5 + 2);
			SelectedVertices.Remove(((j-1)*(grid.X) + i)*5 + 3);
			SelectedFaces.Remove( (j-1)*(grid.X) + i );
		}
		if (j < (grid.Z-1)) {
			SelectedVertices.Remove( ((j+1)*(grid.X) + i)*5 + 0);
			SelectedVertices.Remove( ((j+1)*(grid.X) + i)*5 + 1);
			SelectedFaces.Remove( (j+1)*(grid.X) + i );
		}
	}
	#endregion

	#region Separate vertice
	public static Vector3 OldPosSepVertice(Object target, int i, int j, int k){
		Grid grid = (Grid)target;

		Vector3 oldPos = vertices[(j*(grid.X) + i)*5 + k];
		oldPos += 0.2f*(vertices[(j*(grid.X) + i)*5 + 4] - vertices[(j*(grid.X) + i)*5 + k]);

		return oldPos;
	}

	public static void AddSeparateVertice(Object target, int i, int j, int k){
		Grid grid = (Grid)target;

		SelectedVertices.Add((j*(grid.X) + i)*5 + k); 
		SelectedFaces.Add(j*(grid.X) + i);
	}

	public static void RemoveSeparateVertice(Object target, int i, int j, int k){
		Grid grid = (Grid)target;
		
		SelectedVertices.Remove((j*(grid.X) + i)*5 + k); 
		SelectedFaces.Remove(j*(grid.X) + i);
	}
	#endregion

	#region Separate edges
	public static Vector3 OldPosSepEdgeV(Object target, int i, int j, int k){
		Grid grid = (Grid)target;

		Vector3 oldPos = (vertices[(j*grid.X + i)*5 + k] + vertices[(j*grid.X + i)*5 + (2+k)] )/2f;
		oldPos += 0.15f*(vertices[(j*(grid.X) + i)*5 + 4] - oldPos);
		return oldPos;
	}

	public static void AddSeparateEdgeV(Object target, int i, int j, int k){
		Grid grid = (Grid)target;

		SelectedVertices.Add((j*grid.X + i)*5 + k);
		SelectedVertices.Add((j*grid.X + i)*5 + (2+k));
		SelectedFaces.Add(j*grid.X + i);
	}

	public static void RemoveSeparateEdgeV(Object target, int i, int j, int k){
		Grid grid = (Grid)target;
		
		SelectedVertices.Remove((j*grid.X + i)*5 + k);
		SelectedVertices.Remove((j*grid.X + i)*5 + (2+k));
		SelectedFaces.Remove(j*grid.X + i);
	}


	public static Vector3 OldPosSepEdgeH(Object target, int i, int j, int k){
		Grid grid = (Grid)target;

		Vector3 oldPos = (vertices[(j*grid.X + i)*5 + (2*k)] + vertices[(j*grid.X + i)*5 + (1+2*k)] )/2f;
		oldPos += 0.15f*(vertices[(j*(grid.X) + i)*5 + 4] - oldPos);
		return oldPos;
	}

	public static void AddSeparateEdgeH(Object target, int i, int j, int k){
		Grid grid = (Grid)target;

		SelectedVertices.Add((j*grid.X + i)*5 + (2*k));
		SelectedVertices.Add((j*grid.X + i)*5 + (1+2*k));
		SelectedFaces.Add(j*grid.X + i);
	}

	public static void RemoveSeparateEdgeH(Object target, int i, int j, int k){
		Grid grid = (Grid)target;
		
		SelectedVertices.Remove((j*grid.X + i)*5 + (2*k));
		SelectedVertices.Remove((j*grid.X + i)*5 + (1+2*k));
		SelectedFaces.Remove(j*grid.X + i);
	}
	#endregion

	#region Separate faces
	public static Vector3 OldPosSepFace(Object target, int i, int j){
		Grid grid = (Grid)target;
		return vertices[(j*(grid.X) + i)*5 + 4];;
	}

	public static void AddSeparateFace(Object target, int i, int j){
		Grid grid = (Grid)target;

		for (int n = 0; n < 5; n ++)
			SelectedVertices.Add((j*(grid.X) + i)*5 + n);
	}

	public static void RemoveSeparateFace(Object target, int i, int j){
		Grid grid = (Grid)target;
		
		for (int n = 0; n < 5; n ++)
			SelectedVertices.Remove((j*(grid.X) + i)*5 + n);
	}

	#endregion

	#endregion

	/*
	#region Increasing functions

	public static void IncreaseRight(Grid grid){
		// Copying Old Vertices
		for (int j = 0; j < grid.Z; j++) {							
			for (int i = 0; i < grid.X - 1; i++) {	// (j*(grid.X) + i) = face number
				grid.vertices[(j*(grid.X) + i)*5 + 0] = grid.mesh.vertices[(j*(grid.X-1) + i)*5 + 0];
				grid.vertices[(j*(grid.X) + i)*5 + 1] = grid.mesh.vertices[(j*(grid.X-1) + i)*5 + 1];
				grid.vertices[(j*(grid.X) + i)*5 + 2] = grid.mesh.vertices[(j*(grid.X-1) + i)*5 + 2];
				grid.vertices[(j*(grid.X) + i)*5 + 3] = grid.mesh.vertices[(j*(grid.X-1) + i)*5 + 3];
				grid.vertices[(j*(grid.X) + i)*5 + 4] = grid.mesh.vertices[(j*(grid.X-1) + i)*5 + 4];
			}
		}

		// New Vertices
		for (int j = 0; j < grid.Z; j++) {					//							
			for (int i = grid.X - 1; i < grid.X; i++) {		// (j*(grid.X) + i) = face number							
				grid.vertices [(j*(grid.X) + i)*5 	 ] = new Vector3 ((grid.mesh.vertices[grid.mesh.vertexCount - 2].x		)*grid.tileWidthX , 0,(grid.mesh.vertices[0].z+j) 	  *grid.tileWidthZ);
				grid.vertices [(j*(grid.X) + i)*5 + 1] = new Vector3 ((grid.mesh.vertices[grid.mesh.vertexCount - 2].x + 1	)*grid.tileWidthX , 0,(grid.mesh.vertices[0].z+j)	  *grid.tileWidthZ);
				grid.vertices [(j*(grid.X) + i)*5 + 2] = new Vector3 ((grid.mesh.vertices[grid.mesh.vertexCount - 2].x		)*grid.tileWidthX , 0,(grid.mesh.vertices[0].z+j +1)  *grid.tileWidthZ);
				grid.vertices [(j*(grid.X) + i)*5 + 3] = new Vector3 ((grid.mesh.vertices[grid.mesh.vertexCount - 2].x + 1	)*grid.tileWidthX , 0,(grid.mesh.vertices[0].z+j +1)  *grid.tileWidthZ);
				grid.vertices [(j*(grid.X) + i)*5 + 4] = new Vector3 ((grid.mesh.vertices[grid.mesh.vertexCount - 2].x +.5f	)*grid.tileWidthX , 0,(grid.mesh.vertices[0].z+j +.5f)*grid.tileWidthZ);		
			}																	
		}
	}

	public static void IncreaseLeft(Grid grid){
		// Copying Old Vertices
		for (int j = 0; j < grid.Z; j++) {							
			for (int i = 1; i < grid.X ; i++) {	// (j*(grid.X) + i) = face number
				grid.vertices[(j*(grid.X) + i)*5 + 0] = grid.mesh.vertices[(j*(grid.X-1) + i-1)*5 + 0];
				grid.vertices[(j*(grid.X) + i)*5 + 1] = grid.mesh.vertices[(j*(grid.X-1) + i-1)*5 + 1];
				grid.vertices[(j*(grid.X) + i)*5 + 2] = grid.mesh.vertices[(j*(grid.X-1) + i-1)*5 + 2];
				grid.vertices[(j*(grid.X) + i)*5 + 3] = grid.mesh.vertices[(j*(grid.X-1) + i-1)*5 + 3];
				grid.vertices[(j*(grid.X) + i)*5 + 4] = grid.mesh.vertices[(j*(grid.X-1) + i-1)*5 + 4];
			}
		}
		
		// New Vertices
		for (int j = 0; j < grid.Z; j++) {		//							
			for (int i = 0; i < 1; i++) {		// (j*(grid.X) + i) = face number							
				grid.vertices [(j*(grid.X) + i)*5 	 ] = new Vector3 ((grid.mesh.vertices[0].x - 1) *grid.tileWidthX 	 , 0,(grid.mesh.vertices[0].z+j)		*grid.tileWidthZ);
				grid.vertices [(j*(grid.X) + i)*5 + 1] = new Vector3 ( grid.mesh.vertices[0].x		*grid.tileWidthX	 , 0,(grid.mesh.vertices[0].z+j)		*grid.tileWidthZ);
				grid.vertices [(j*(grid.X) + i)*5 + 2] = new Vector3 ((grid.mesh.vertices[0].x - 1)	*grid.tileWidthX	 , 0,(grid.mesh.vertices[0].z+j + 1)	*grid.tileWidthZ);
				grid.vertices [(j*(grid.X) + i)*5 + 3] = new Vector3 ( grid.mesh.vertices[0].x		*grid.tileWidthX	 , 0,(grid.mesh.vertices[0].z+j + 1) 	*grid.tileWidthZ);
				grid.vertices [(j*(grid.X) + i)*5 + 4] = new Vector3 ((grid.mesh.vertices[0].x -.5f) *grid.tileWidthX 	 , 0,(grid.mesh.vertices[0].z+j + .5f)	*grid.tileWidthZ);		
			}																	
		}
	}

	public static void IncreaseUp(Grid grid){
		// Copying Old Vertices
		for (int j = 0; j < grid.Z - 1; j++) {							
			for (int i = 0; i < grid.X; i++) {	// (j*(grid.X) + i) = face number
				grid.vertices[(j*(grid.X) + i)*5 + 0] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 0];
				grid.vertices[(j*(grid.X) + i)*5 + 1] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 1];
				grid.vertices[(j*(grid.X) + i)*5 + 2] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 2];
				grid.vertices[(j*(grid.X) + i)*5 + 3] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 3];
				grid.vertices[(j*(grid.X) + i)*5 + 4] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 4];
			}
		}
		
		// New Vertices
		for (int j = grid.Z - 1; j < grid.Z; j++) {			//							
			for (int i = 0; i < grid.X; i++) {				// (j*(grid.X) + i) = face number							
				grid.vertices [(j*(grid.X) + i)*5 	 ] = new Vector3 (i + (grid.mesh.vertices[0].x		) , 0,(grid.mesh.vertices[grid.mesh.vertexCount - 2].z) 	);
				grid.vertices [(j*(grid.X) + i)*5 + 1] = new Vector3 (i + (grid.mesh.vertices[0].x + 1	) , 0,(grid.mesh.vertices[grid.mesh.vertexCount - 2].z)	  	);
				grid.vertices [(j*(grid.X) + i)*5 + 2] = new Vector3 (i + (grid.mesh.vertices[0].x		) , 0,(grid.mesh.vertices[grid.mesh.vertexCount - 2].z +1)  );
				grid.vertices [(j*(grid.X) + i)*5 + 3] = new Vector3 (i + (grid.mesh.vertices[0].x + 1	) , 0,(grid.mesh.vertices[grid.mesh.vertexCount - 2].z +1)  );
				grid.vertices [(j*(grid.X) + i)*5 + 4] = new Vector3 (i + (grid.mesh.vertices[0].x +.5f	) , 0,(grid.mesh.vertices[grid.mesh.vertexCount - 2].z +.5f));		
			}																	
		}
	}

	public static void IncreaseDown(Grid grid){
		// Copying Old Vertices
		for (int j = 1; j < grid.Z; j++) {							
			for (int i = 0; i < grid.X; i++) {	// (j*(grid.X) + i) = face number
				grid.vertices[(j*(grid.X) + i)*5 + 0] = grid.mesh.vertices[((j-1)*(grid.X) + i)*5 + 0];
				grid.vertices[(j*(grid.X) + i)*5 + 1] = grid.mesh.vertices[((j-1)*(grid.X) + i)*5 + 1];
				grid.vertices[(j*(grid.X) + i)*5 + 2] = grid.mesh.vertices[((j-1)*(grid.X) + i)*5 + 2];
				grid.vertices[(j*(grid.X) + i)*5 + 3] = grid.mesh.vertices[((j-1)*(grid.X) + i)*5 + 3];
				grid.vertices[(j*(grid.X) + i)*5 + 4] = grid.mesh.vertices[((j-1)*(grid.X) + i)*5 + 4];
			}
		}
		
		// New Vertices
		for (int j = 0; j < 1; j++) {			//							
			for (int i = 0; i < grid.X; i++) {				// (j*(grid.X) + i) = face number							
				grid.vertices [(j*(grid.X) + i)*5 	 ] = new Vector3 (i + (grid.mesh.vertices[0].x		) , 0,(grid.mesh.vertices[0].z - 1) );
				grid.vertices [(j*(grid.X) + i)*5 + 1] = new Vector3 (i + (grid.mesh.vertices[0].x + 1	) , 0,(grid.mesh.vertices[0].z - 1)	);
				grid.vertices [(j*(grid.X) + i)*5 + 2] = new Vector3 (i + (grid.mesh.vertices[0].x		) , 0,(grid.mesh.vertices[0].z )  	);
				grid.vertices [(j*(grid.X) + i)*5 + 3] = new Vector3 (i + (grid.mesh.vertices[0].x + 1	) , 0,(grid.mesh.vertices[0].z )  	);
				grid.vertices [(j*(grid.X) + i)*5 + 4] = new Vector3 (i + (grid.mesh.vertices[0].x +.5f	) , 0,(grid.mesh.vertices[0].z -.5f));		
			}																	
		}
	}
 
	#endregion

	#region Decreasing functions

	public static void DecreaseRight(Grid grid){
		// Copying Old Vertices
		for (int j = 0; j < grid.Z; j++) {							
			for (int i = 0; i < grid.X; i++) {	// (j*(grid.X) + i) = face number
				grid.vertices[(j*(grid.X) + i)*5 + 0] = grid.mesh.vertices[(j*(grid.X+1) + i)*5 + 0];
				grid.vertices[(j*(grid.X) + i)*5 + 1] = grid.mesh.vertices[(j*(grid.X+1) + i)*5 + 1];
				grid.vertices[(j*(grid.X) + i)*5 + 2] = grid.mesh.vertices[(j*(grid.X+1) + i)*5 + 2];
				grid.vertices[(j*(grid.X) + i)*5 + 3] = grid.mesh.vertices[(j*(grid.X+1) + i)*5 + 3];
				grid.vertices[(j*(grid.X) + i)*5 + 4] = grid.mesh.vertices[(j*(grid.X+1) + i)*5 + 4];
			}
		}
	}

	public static void DecreaseLeft(Grid grid){
		// Copying Old Vertices
		for (int j = 0; j < grid.Z; j++) {							
			for (int i = 0; i < grid.X ; i++) {	// (j*(grid.X) + i) = face number
				grid.vertices[(j*(grid.X) + i)*5 + 0] = grid.mesh.vertices[(j*(grid.X+1) + i+1)*5 + 0];
				grid.vertices[(j*(grid.X) + i)*5 + 1] = grid.mesh.vertices[(j*(grid.X+1) + i+1)*5 + 1];
				grid.vertices[(j*(grid.X) + i)*5 + 2] = grid.mesh.vertices[(j*(grid.X+1) + i+1)*5 + 2];
				grid.vertices[(j*(grid.X) + i)*5 + 3] = grid.mesh.vertices[(j*(grid.X+1) + i+1)*5 + 3];
				grid.vertices[(j*(grid.X) + i)*5 + 4] = grid.mesh.vertices[(j*(grid.X+1) + i+1)*5 + 4];
			}
		}
	}

	public static void DecreaseUp(Grid grid){
		// Copying Old Vertices
		for (int j = 0; j < grid.Z; j++) {							
			for (int i = 0; i < grid.X ; i++) {	// (j*(grid.X) + i) = face number
				grid.vertices[(j*(grid.X) + i)*5 + 0] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 0];
				grid.vertices[(j*(grid.X) + i)*5 + 1] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 1];
				grid.vertices[(j*(grid.X) + i)*5 + 2] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 2];
				grid.vertices[(j*(grid.X) + i)*5 + 3] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 3];
				grid.vertices[(j*(grid.X) + i)*5 + 4] = grid.mesh.vertices[(j*(grid.X) + i)*5 + 4];
			}
		}
	}

	public static void DecreaseDown(Grid grid){
		// Copying Old Vertices
		for (int j = 0; j < grid.Z; j++) {							
			for (int i = 0; i < grid.X ; i++) {	// (j*(grid.X) + i) = face number
				grid.vertices[(j*(grid.X) + i)*5 + 0] = grid.mesh.vertices[((j+1)*(grid.X) + i)*5 + 0];
				grid.vertices[(j*(grid.X) + i)*5 + 1] = grid.mesh.vertices[((j+1)*(grid.X) + i)*5 + 1];
				grid.vertices[(j*(grid.X) + i)*5 + 2] = grid.mesh.vertices[((j+1)*(grid.X) + i)*5 + 2];
				grid.vertices[(j*(grid.X) + i)*5 + 3] = grid.mesh.vertices[((j+1)*(grid.X) + i)*5 + 3];
				grid.vertices[(j*(grid.X) + i)*5 + 4] = grid.mesh.vertices[((j+1)*(grid.X) + i)*5 + 4];
			}
		}
	}

	#endregion
	*/

	#region Helper functions
	public static float round(float input, float snapValue){
		float snappedValue; 
		snappedValue = snapValue * Mathf.Round((input / snapValue));
		return(snappedValue);
	}

	public static void DrawGrid (Object target){
		Grid grid = (Grid)target;
		
		Material mat, mat2;

		if (grid.gameObject.GetComponent<Renderer>().sharedMaterial != null)
			mat = grid.gameObject.GetComponent<Renderer>().sharedMaterial;
		
		else
			return;
			
		GL.PushMatrix();

		GL.LoadIdentity();
		GL.MultMatrix(Camera.current.worldToCameraMatrix);


		if (grid.gameObject.GetComponent<Renderer>().sharedMaterials.Length >= 2){
			if (grid.gameObject.GetComponent<Renderer>().sharedMaterials[1] != null){
				mat2 = grid.gameObject.GetComponent<Renderer>().sharedMaterials[1];
				Shader matS = mat2.shader;
				mat2.shader = Shader.Find("Unlit/Texture");
				
				GL.Begin(GL.TRIANGLES);
				mat2.SetPass(0);
				
				foreach (int i in SelectedFaces){
					Vector3 Mean4 = (SelectedVertices.Contains(i*5) ? vertices[i*5] + Vector3.up * HandleDelta : vertices[i*5]) + 
							(SelectedVertices.Contains(i*5 + 1) ? vertices[i*5 + 1] + Vector3.up * HandleDelta : vertices[i*5 + 1]) + 
							(SelectedVertices.Contains(i*5 + 2) ? vertices[i*5 + 2] + Vector3.up * HandleDelta : vertices[i*5 + 2]) + 
							(SelectedVertices.Contains(i*5 + 3) ? vertices[i*5 + 3] + Vector3.up * HandleDelta : vertices[i*5 + 3]) ;
					
					Mean4 /= 4f;
					
					//Down Triangle
					GL.TexCoord2 (vertices[i*5].x / grid.X, vertices[i*5].z / grid.Z);
					GL.Vertex( SelectedVertices.Contains(i*5) ? vertices[i*5] + Vector3.up * HandleDelta : vertices[i*5]);
					
					GL.TexCoord2 (vertices[i*5+4].x / grid.X, vertices[i*5+4].z / grid.Z);
					GL.Vertex( Mean4);
					
					GL.TexCoord2 (vertices[i*5+1].x / grid.X, vertices[i*5+1].z / grid.Z);
					GL.Vertex( SelectedVertices.Contains(i*5 + 1) ? vertices[i*5 + 1] + Vector3.up * HandleDelta : vertices[i*5 + 1]);
					
					
					//Right Triangle
					GL.TexCoord2 (vertices[i*5+1].x / grid.X, vertices[i*5+1].z / grid.Z);
					GL.Vertex( SelectedVertices.Contains(i*5 + 1) ? vertices[i*5 + 1] + Vector3.up * HandleDelta : vertices[i*5 + 1]);
					
					GL.TexCoord2 (vertices[i*5+4].x / grid.X, vertices[i*5+4].z / grid.Z);
					GL.Vertex( Mean4);
					
					GL.TexCoord2 (vertices[i*5+3].x / grid.X, vertices[i*5+3].z / grid.Z);
					GL.Vertex( SelectedVertices.Contains(i*5 + 3) ? vertices[i*5 + 3] + Vector3.up * HandleDelta : vertices[i*5 + 3]);
					
					
					//Up Triangle
					GL.TexCoord2 (vertices[i*5+2].x / grid.X, vertices[i*5+2].z / grid.Z);
					GL.Vertex( SelectedVertices.Contains(i*5 + 2) ? vertices[i*5 + 2] + Vector3.up * HandleDelta : vertices[i*5 + 2]);
					
					GL.TexCoord2 (vertices[i*5+3].x / grid.X, vertices[i*5+3].z / grid.Z);
					GL.Vertex( SelectedVertices.Contains(i*5 + 3) ? vertices[i*5 + 3] + Vector3.up * HandleDelta : vertices[i*5 + 3]);
					
					GL.TexCoord2 (vertices[i*5+4].x / grid.X, vertices[i*5+4].z / grid.Z);
					GL.Vertex( Mean4);
					
					
					//Left Triangle
					GL.TexCoord2 (vertices[i*5].x / grid.X, vertices[i*5].z / grid.Z);
					GL.Vertex( SelectedVertices.Contains(i*5) ? vertices[i*5] + Vector3.up * HandleDelta : vertices[i*5]);
					
					GL.TexCoord2 (vertices[i*5+2].x / grid.X, vertices[i*5+2].z / grid.Z);
					GL.Vertex( SelectedVertices.Contains(i*5 + 2) ? vertices[i*5 + 2] + Vector3.up * HandleDelta : vertices[i*5 + 2]);
					
					GL.TexCoord2 (vertices[i*5+4].x / grid.X, vertices[i*5+4].z / grid.Z);
					GL.Vertex( Mean4);
				}
				
				GL.End();

				GL.PopMatrix();

				mat2.shader = matS;
			}
		}

		else{
			GL.Begin(GL.TRIANGLES);
			mat.SetPass(0);
			GL.Color(new Color(.5f,.5f,.5f,.5f));


			foreach (int i in SelectedFaces){
				Vector3 Mean4 = (SelectedVertices.Contains(i*5) ? vertices[i*5] + Vector3.up * HandleDelta : vertices[i*5]) + 
								(SelectedVertices.Contains(i*5 + 1) ? vertices[i*5 + 1] + Vector3.up * HandleDelta : vertices[i*5 + 1]) + 
								(SelectedVertices.Contains(i*5 + 2) ? vertices[i*5 + 2] + Vector3.up * HandleDelta : vertices[i*5 + 2]) + 
								(SelectedVertices.Contains(i*5 + 3) ? vertices[i*5 + 3] + Vector3.up * HandleDelta : vertices[i*5 + 3]) ;

				Mean4 /= 4f;

				//Down Triangle
				GL.TexCoord2 (0,0);
				GL.Vertex( SelectedVertices.Contains(i*5) ? vertices[i*5] + Vector3.up * HandleDelta : vertices[i*5]);

				GL.TexCoord2 (.5f,.5f);
				GL.Vertex( Mean4);

				GL.TexCoord2 (1,0);
				GL.Vertex( SelectedVertices.Contains(i*5 + 1) ? vertices[i*5 + 1] + Vector3.up * HandleDelta : vertices[i*5 + 1]);


				//Right Triangle
				GL.TexCoord2 (1,0);
				GL.Vertex( SelectedVertices.Contains(i*5 + 1) ? vertices[i*5 + 1] + Vector3.up * HandleDelta : vertices[i*5 + 1]);
				
				GL.TexCoord2 (.5f,.5f);
				GL.Vertex( Mean4);
				
				GL.TexCoord2 (1,1);
				GL.Vertex( SelectedVertices.Contains(i*5 + 3) ? vertices[i*5 + 3] + Vector3.up * HandleDelta : vertices[i*5 + 3]);


				//Up Triangle
				GL.TexCoord2 (0,1);
				GL.Vertex( SelectedVertices.Contains(i*5 + 2) ? vertices[i*5 + 2] + Vector3.up * HandleDelta : vertices[i*5 + 2]);
							
				GL.TexCoord2 (1,1);
				GL.Vertex( SelectedVertices.Contains(i*5 + 3) ? vertices[i*5 + 3] + Vector3.up * HandleDelta : vertices[i*5 + 3]);
				
				GL.TexCoord2 (.5f,.5f);
				GL.Vertex( Mean4);


				//Left Triangle
				GL.TexCoord2 (0,0);
				GL.Vertex( SelectedVertices.Contains(i*5) ? vertices[i*5] + Vector3.up * HandleDelta : vertices[i*5]);
				
				GL.TexCoord2 (0,1);
				GL.Vertex( SelectedVertices.Contains(i*5 + 2) ? vertices[i*5 + 2] + Vector3.up * HandleDelta : vertices[i*5 + 2]);
				
				GL.TexCoord2 (.5f,.5f);
				GL.Vertex( Mean4);
			}

			GL.End();

			GL.PopMatrix();
		}
	
	}

	
	public static void UpdateGridMesh(Object target){
		Grid grid = (Grid)target;

		MeshFilter meshFilter = grid.gameObject.GetComponent<MeshFilter>();

		if (meshFilter.sharedMesh != null){
			meshFilter.sharedMesh.Clear();		}
		
		else {
			meshFilter.sharedMesh      	= new Mesh();
			meshFilter.sharedMesh.name 	= "Grid";		}
		
		meshFilter.sharedMesh.vertices 	= vertices;
		
		meshFilter.sharedMesh.triangles = grid.CalculateTriangles	(grid.X , grid.Z);
		meshFilter.sharedMesh.normals 	= grid.CalculateNormals		(grid.X * grid.Z * 5);
		meshFilter.sharedMesh.uv2		= grid.CalculateUV			(grid.X * grid.Z);
		meshFilter.sharedMesh.uv 		= grid.CalculateUV2			(grid.X * grid.Z * 5);
		
		grid.gameObject.GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;

		grid.UnpaintAllFaces();
	}

	public static void SmoothFace(int faceNumber){
		float MeanY = vertices [faceNumber * 5].y +
			vertices [faceNumber * 5 + 1].y +
				vertices [faceNumber * 5 + 2].y +
				vertices [faceNumber * 5 + 3].y;
		
		vertices [faceNumber * 5 + 4].y = MeanY/4f;
	}

	#endregion

}