using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Unit : MonoBehaviour {
	//Mandatory Status
	public int 		Team;
	public Aliance 	aliance;

	public string 	UnitName;
	public int 		Level;

	public int WalkDistance;
	public int AttackRange;
	public int TotalHP;
	public int CurrentHP;

	//Custom Status
	public List <int> StatusList;

	//Current State
	public UnitState state;
	public bool hasMoved;
	public bool hasAttacked;
	public int	currentTile;

	//Pointers
	public Grid 			grid;
	public BattleController controller;

	//Victory Conditions Helpers
	public bool isSpecial; 	//A Team is defeated if every 'isSpecial' Unit is defeated
	public bool isUnique;	//A Team is defeated if one 'isUnique' Unit is defeated

	#region Editor functions
	#if UNITY_EDITOR
	//Functions to Handle BattleController Status
	public void addStatus(){
		Undo.RecordObject(this, "Add Unit Status");
		StatusList.Add(0);
		EditorUtility.SetDirty(this);
	}
	
	public void removeStatusAt(int index){
		Undo.RecordObject(this, "Remove Unit Status");
		StatusList.RemoveAt(index);
		EditorUtility.SetDirty(this);
	}
	
	public void moveStatusFromTo(int from, int to){
		Undo.RecordObject(this, "Move Unit Status");
		
		int temp 			= StatusList[from];
		StatusList[from] 	= StatusList[to];
		StatusList[to] 		= temp;
		
		EditorUtility.SetDirty(this);
	}
	#endif
	#endregion

	#region MonoBehaviour functions
	void Awake() {
		grid 		= GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
		controller	= GameObject.FindGameObjectWithTag("BattleController").GetComponent<BattleController>();

		currentTile = GetFaceFromPoint(transform.position);
	}
	
	void Start() {
		state 		= UnitState.Idle;
		hasMoved 	= false;
		hasAttacked = false;

		GetComponent<Animation>().Play("idlebattle");
	}
	
	void Update() { 
		if (state == UnitState.Walking){
			Move ();
		}

		else if (state == UnitState.AttackAnimation){
			if(!GetComponent<Animation>().IsPlaying("attack")){
				GetComponent<Animation>().Play("idlebattle");
				state = UnitState.Idle;

				//If battle hasn't ended
				if (controller.conditions.CheckConditions() == ConditionsStatus.Nothing){
					controller.PaintFaces();
					controller.battleGui.ActionsMenuCanvas.enabled = true;
				}

				else if (controller.conditions.CheckConditions() == ConditionsStatus.Victory)	controller.Victory();
				else if (controller.conditions.CheckConditions() == ConditionsStatus.Defeat)	controller.Defeat();

			}

		}

		else if (state == UnitState.Dying){
			if(!GetComponent<Animation>().IsPlaying("die")){
				GameObject.Destroy(gameObject);
			}
		}
	}
	#endregion

	#region WalkTo
	//Calculates path and set this unit in motion
	public void WalkTo(Vector3 destination){
		int destinationTile = GetFaceFromPoint(destination);

		grid.pathGraph.SearchFrom(currentTile);
		grid.pathGraph.CalculatePathTo(destinationTile);

		grid.pathGraph.Vertices[currentTile].occupied 		= false;
		grid.pathGraph.Vertices[destinationTile].occupied 	= true;

		GetComponent<Animation>().Play("run");

		controller.battleGui.ActionsMenuCanvas.enabled = false;

		state = UnitState.Walking;
	}
	#endregion

	#region Move
	//Moves unit along calculated path
	void Move (){

		if(grid.pathGraph.Path.Count == 0){
			state = UnitState.Idle;
			GetComponent<Animation>().Play("idlebattle");
			hasMoved = true;
			currentTile = GetFaceFromPoint(transform.position);

			controller.PaintFaces();
			controller.battleGui.ActionsMenuCanvas.enabled = true;
		}

		else {
			Vector3 dir = (grid.pathGraph.Path[0] - transform.position).normalized;

			Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, Mathf.PI, 0f);
			transform.rotation = Quaternion.LookRotation(newDir);

			dir *= 5f * Time.fixedDeltaTime;
			transform.position += dir;

			if ( (transform.position - grid.pathGraph.Path[0]).magnitude <= 0.05){
				transform.position = grid.pathGraph.Path[0];
				grid.pathGraph.Path.RemoveAt(0);
			}
		}
	}
	#endregion

	#region Helper functions
	public int GetFaceFromPoint(Vector3 position){
		return ( (Mathf.FloorToInt(position.z) - (int)grid.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[0].z) * grid.X + 
		        (Mathf.FloorToInt(position.x) - (int)grid.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[0].x));
	}
	#endregion
}

public enum UnitState{
	Idle,
	Walking,
	Attacking,
	AttackAnimation,
	Dying
}

public enum Aliance{
	Ally,
	Enemy
}