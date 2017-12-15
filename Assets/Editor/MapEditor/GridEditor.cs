using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor (typeof(Grid))]
public class GridEditor : Editor
{
	Grid gridObject;
	SerializedObject grid;
	
	SerializedProperty X, Z;
	SerializedProperty snap;
			
	Tool LastTool = Tool.None;

	int hash = "MouseDragHash".GetHashCode();

	#region On Enable/ Disable
	void OnEnable() {
		//Hide Unity's Handles
  	    LastTool = Tools.current;
        Tools.current = Tool.None;

		//Hide WireFrame
		gridObject = (Grid)target;
		EditorUtility.SetSelectedWireframeHidden(gridObject.gameObject.GetComponent<Renderer>(), true);

		if (!EditorApplication.isPlaying){
			if (gridObject.GetComponent<MeshFilter>().sharedMesh == null){
				gridObject.X = 0;
				gridObject.Z = 0;

				GridEditorUtility.NewGrid(target);
				GridEditorUtility.UpdateGridMesh(target);
			}

			GridEditorUtility.vertices = gridObject.GetComponent<MeshFilter>().sharedMesh.vertices;
		}

		//Editor Serialized Properties
		grid = new SerializedObject (target);

		X = grid.FindProperty("X");
		Z = grid.FindProperty("Z");
		
		snap = grid.FindProperty("snap");
    }

    void OnDisable() {
		//Hide Unity's Handles
        Tools.current = LastTool;

		GridEditorUtility.IsMouseDown = false;

		if (gridObject != null){
			gridObject.GetComponent<MeshCollider>().sharedMesh = gridObject.GetComponent<MeshFilter>().sharedMesh;
			gridObject.UnpaintAllFaces();
		}

    }
	#endregion

	#region On Inspector/ Scene GUI
	public override void OnInspectorGUI() {
		grid.Update();

		GUILayout.Space(10f);

		GUILayout.BeginHorizontal();		
		if (GUILayout.Button(new GUIContent("<-", "Undo"), EditorStyles.miniButtonLeft, GUILayout.Width(25f)) ){
			if (GridEditorUtility.MeshUndo.Count > 0){
				VertexUndo undo = GridEditorUtility.MeshUndo.Pop();
				
				foreach (int n in undo.vertices) 	GridEditorUtility.vertices[n].y -= undo.added;
				foreach (int n in undo.faces) 		GridEditorUtility.SmoothFace(n);
				
				GridEditorUtility.MeshRedo.Push(undo);

				gridObject.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices 	= GridEditorUtility.vertices;

				gridObject.gameObject.GetComponent<MeshCollider>().sharedMesh = 
					gridObject.gameObject.GetComponent<MeshFilter>().sharedMesh;
				
				SceneView.RepaintAll();
			}
		}
		
		if (GUILayout.Button(new GUIContent("->", "Redo"), EditorStyles.miniButtonRight, GUILayout.Width(25f)) ){
			if (GridEditorUtility.MeshRedo.Count > 0){
				VertexUndo undo = GridEditorUtility.MeshRedo.Pop();
				
				foreach (int n in undo.vertices) 	GridEditorUtility.vertices[n].y += undo.added;
				foreach (int n in undo.faces) 		GridEditorUtility.SmoothFace(n);
				
				GridEditorUtility.MeshUndo.Push(undo);

				gridObject.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices 	= GridEditorUtility.vertices;
				
				gridObject.gameObject.GetComponent<MeshCollider>().sharedMesh = 
					gridObject.gameObject.GetComponent<MeshFilter>().sharedMesh;
				
				SceneView.RepaintAll();
			}
		}
		GUILayout.EndHorizontal();
		
		
		GUILayout.Space(10f);
		
		
		GUILayout.BeginHorizontal();		
		if (GUILayout.Toggle((gridObject.Edition == 1 && gridObject.EditionType == 1)? true : false ,"1 1", "Button") ) {
			gridObject.Edition = 1;
			gridObject.EditionType = 1;
			SceneView.RepaintAll();
		}
		
		if (GUILayout.Toggle((gridObject.Edition == 2 && gridObject.EditionType == 1)? true : false ,"2 1", "Button") ) {
			gridObject.Edition = 2;
			gridObject.EditionType = 1;
			SceneView.RepaintAll();
		}
		
		if (GUILayout.Toggle((gridObject.Edition == 3 && gridObject.EditionType == 1)? true : false ,"3 1", "Button") ) {
			gridObject.Edition = 3;
			gridObject.EditionType = 1;
			SceneView.RepaintAll();
		}		
		GUILayout.EndHorizontal();


		GUILayout.BeginHorizontal();		
		if (GUILayout.Toggle((gridObject.Edition == 1 && gridObject.EditionType == 2)? true : false ,"1 2", "Button") ) {
			gridObject.Edition = 1;
			gridObject.EditionType = 2;
			SceneView.RepaintAll();
		}
		
		if (GUILayout.Toggle((gridObject.Edition == 2 && gridObject.EditionType == 2)? true : false ,"2 2", "Button") ) {
			gridObject.Edition = 2;
			gridObject.EditionType = 2;
			SceneView.RepaintAll();
		}
		
		if (GUILayout.Toggle((gridObject.Edition == 3 && gridObject.EditionType == 2)? true : false ,"3 2", "Button") ) {
			gridObject.Edition = 3;
			gridObject.EditionType = 2;
			SceneView.RepaintAll();
		}		
		GUILayout.EndHorizontal();
		
		
		GUILayout.Space(10f);
		
		
		GUILayout.BeginHorizontal();		
		if (GUILayout.Toggle((gridObject.Edition == 4)? true : false ,"4", "Button") ) {
			gridObject.Edition = 4;
			SceneView.RepaintAll();
		}
		
		if (GUILayout.Toggle((gridObject.Edition == 5)? true : false ,"5", "Button") ) {
			gridObject.Edition = 5;
			SceneView.RepaintAll();
		}		
		GUILayout.EndHorizontal();



		
		EditorGUILayout.PropertyField(X);
		EditorGUILayout.PropertyField(Z);
		
		if(grid.ApplyModifiedProperties() ){
			GridEditorUtility.NewGrid(target);
			GridEditorUtility.UpdateGridMesh(target);
		}
		
		EditorGUILayout.PropertyField(snap);	
		
		if (grid.ApplyModifiedProperties()){
			SceneView.RepaintAll();		
			GridEditorUtility.DeselectAll();
		}
    }
    
	void OnSceneGUI () {
		Tools.current = Tool.None;	//Hide move handle

		//PathUtility.GraphDebug(target);
		//PathUtility.DrawWalkablePath(target);

		EditionKeyboard();			//Keyboard shortcuts handling for grid mesh editing
		SelectionKeyboard();		//Keyboard shortcuts handling for handles multiselection. Also calls Mesh/ Graph editing functions

		Handles.Label( Camera.current.ScreenToWorldPoint( new Vector3(50, 150, Camera.current.nearClipPlane) ), 


		              "Mouse Pos 0: " + GridEditorUtility.MousePos0.ToString() 
		              +	"\nMouse Pos F: " + GridEditorUtility.MousePosF.ToString() 
		              +	"\nLeft Mouse is down: " + GridEditorUtility.IsMouseDown.ToString()
		              /*
		              +	"\nLeft Mouse was down: " + GridEditorUtility.WasMouseDown.ToString()
		              +	"\nContains: " + GridEditorUtility.SelectedHandles.Contains( GridEditorUtility.handleJ * (gridObject.X+1) 
		                                                              + GridEditorUtility.handleI).ToString()
		              + "\nWDist: " + Vector2.Distance( Camera.current.WorldToViewportPoint(Vector2.zero), 
		                   Camera.current.WorldToViewportPoint(Vector2.right) ).ToString()
		              */ 
		              + "\nDelta: " + GridEditorUtility.HandleDelta.ToString()
		              + "\nSelection Count: " + GridEditorUtility.SelectedVertices.Count.ToString()
		              + "\nUndo Stack: " + GridEditorUtility.MeshUndo.Count.ToString()
		              );


	}
	#endregion

	#region EditionKeyboard functions
	void EditionKeyboard(){

		Grid grid = (Grid)target;

		#region Grid editing shortcuts
		if(!Event.current.shift) {
			if(Event.current.type == EventType.keyDown){ 
							   
				if((Event.current.character == 'a' || Event.current.character == 'A') && grid.Edition != 1) 	ChangeEdition(1,target);
				else if((Event.current.character == 's' || Event.current.character == 'S')&& grid.Edition != 2)	ChangeEdition(2,target);
				else if((Event.current.character == 'd' || Event.current.character == 'D')&& grid.Edition != 3) ChangeEdition(3,target);
				else if((Event.current.character == 'v' || Event.current.character == 'V')&& grid.Edition != 3) ChangeEdition(0,target);

				else if(Event.current.character == 'c' || Event.current.character == 'C'){
					if 		(grid.EditionType == 1) ChangeEditionType(2,target);
					else if (grid.EditionType == 2) ChangeEditionType(1,target); }

				else if(Event.current.character == 'z' || Event.current.character == 'Z'){
					ChangeEdition(4,target);
					PathUtility.UpdatePathLines(target); }

				else if(Event.current.character == 'x' || Event.current.character == 'X'){
					ChangeEdition(5,target);
					PathUtility.UpdatePathLines(target); }
			}
		}
		#endregion

		#region Increase/ Decrease grid shortcuts
		if(Event.current.type == EventType.keyDown){ 
			if 	   (Event.current.character == 'q' || Event.current.character == 'Q') GridEditorUtility.IncreaseGrid(target,1);
			else if(Event.current.character == 'w' || Event.current.character == 'W') GridEditorUtility.IncreaseGrid(target,2);
			else if(Event.current.character == 'e' || Event.current.character == 'E') GridEditorUtility.IncreaseGrid(target,3);
			else if(Event.current.character == 'r' || Event.current.character == 'R') GridEditorUtility.IncreaseGrid(target,4);

			else if(Event.current.character == 'y' || Event.current.character == 'Y') GridEditorUtility.DecreaseGrid(target,1);
			else if(Event.current.character == 'u' || Event.current.character == 'U') GridEditorUtility.DecreaseGrid(target,2);
			else if(Event.current.character == 'i' || Event.current.character == 'I') GridEditorUtility.DecreaseGrid(target,3);
			else if(Event.current.character == 'o' || Event.current.character == 'O') GridEditorUtility.DecreaseGrid(target,4);
		}
		#endregion

	}


	void ChangeEdition(int Edition, Object target){
		Grid grid = (Grid)target;
		
		GUIUtility.hotControl = 0;
		GridEditorUtility.DeselectAll();
		grid.Edition = Edition;
	}
	
	void ChangeEditionType(int EditionType, Object target){
		Grid grid = (Grid)target;
		
		GUIUtility.hotControl = 0;
		GridEditorUtility.DeselectAll();
		grid.EditionType = EditionType;
	}
	#endregion

	#region SelectionKeyboard functions
	void SelectionKeyboard(){	// Mesh/Graph Editing is called here
		if( Event.current.shift) {
			if (Event.current.character == 'Z' || Event.current.character == 'z'){
				GridEditorUtility.DeselectAll();
				GridEditorUtility.EditingGrid(target); }
			else {
				GridEditorUtility.Selecting (target); 
			} 

			UpdateDragBox();
		}

		else {
			//CheckInput();
			GridEditorUtility.EditingGrid(target);
			PathUtility.EditingGraph (target);
		}
	}

	void UpdateDragBox(){
		int ID = GUIUtility.GetControlID(hash, FocusType.Passive);

		switch (Event.current.GetTypeForControl(ID)){

		case EventType.Layout:
			HandleUtility.AddDefaultControl(ID);
			break;

		case EventType.MouseDown:
			if(Event.current.button == 0 && HandleUtility.nearestControl == ID){
				GridEditorUtility.MousePos0 = Camera.current.ScreenToViewportPoint(Event.current.mousePosition);
				GridEditorUtility.MousePos0.y = 1f - GridEditorUtility.MousePos0.y;
				
				GridEditorUtility.IsMouseDown = true;
				
				GUIUtility.hotControl = ID;
				Event.current.Use();
				EditorGUIUtility.SetWantsMouseJumping(1);
			}
			break;

		case EventType.MouseDrag:
			if (Event.current.button == 0) {

				GUI.changed = true;
				Event.current.Use();
			}
			break;

		case EventType.MouseUp:
			if (Event.current.button == 0) {
				GridEditorUtility.MousePosF = Camera.current.ScreenToViewportPoint(Event.current.mousePosition);
				GridEditorUtility.MousePosF.y = 1f - GridEditorUtility.MousePosF.y;
				
				GridEditorUtility.DragSelecting(target);
				
				GridEditorUtility.IsMouseDown = false;
				
				if (GUIUtility.hotControl == ID){
					GUIUtility.hotControl = 0;
					Event.current.Use();
					EditorGUIUtility.SetWantsMouseJumping(0);
				}
			}
			break;

		case EventType.Repaint:
			if(GridEditorUtility.IsMouseDown)
				DrawDragBox();
			break;

		}

	}

	void DrawDragBox(){
		GL.PushMatrix();

		Texture tex = gridObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
		gridObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = null;

		gridObject.GetComponent<MeshRenderer>().sharedMaterial.SetPass(0);


		GL.LoadOrtho();
		GL.Begin(GL.LINES);
		GL.Color(Color.white);

		Vector3 mousePosition = Camera.current.ScreenToViewportPoint(Event.current.mousePosition);
		mousePosition.y = 1f - mousePosition.y;

		GL.Vertex3(GridEditorUtility.MousePos0.x, GridEditorUtility.MousePos0.y, 0.999f);
		GL.Vertex3(mousePosition.x, GridEditorUtility.MousePos0.y, 0.999f);
		
		GL.Vertex3(GridEditorUtility.MousePos0.x, mousePosition.y, 0.999f);
		GL.Vertex3(mousePosition.x, mousePosition.y, 0.999f);
		
		
		GL.Vertex3(GridEditorUtility.MousePos0.x, GridEditorUtility.MousePos0.y, 0.999f);
		GL.Vertex3(GridEditorUtility.MousePos0.x, mousePosition.y, 0.999f);
		
		GL.Vertex3(mousePosition.x, GridEditorUtility.MousePos0.y, 0.999f);
		GL.Vertex3(mousePosition.x, mousePosition.y, 0.999f);
		
		
		GL.End();
		GL.PopMatrix();

		gridObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
	}
	#endregion
}