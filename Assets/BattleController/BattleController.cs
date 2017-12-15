using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleController : MonoBehaviour {

	public string [] TeamsList = new string[2];
	public int CurrentTeam;

	public int CurrentTurn;	//A Turn passes after all Teams have played once

	public List <string> StatusList = new List<string>();
	public List <Unit> 	 UnitsList  = new List<Unit>();

	public  List <Operand>	AtkAlgOperands  	= new List<Operand>();
	private List <Operand>	AtkAlgOperandsRPN 	= new List<Operand>();

	public Grid 		grid;
	public GameObject 	gridObject;

	public Unit selectedUnit;
	public bool hasSelecetedUnit = false;

	public bool 			showMenu = false;
	public BattleGUI 		battleGui;
	public Canvas			BattleGUICanvas;
	public Announcements 	announcements;

	public BattleStatus status = BattleStatus.Paused;

	public Color32 walkColor;
	public Color32 attackColor;
	public Color32 selectColor;

	public Conditions conditions = new Conditions();

	//Custom Editor variables
	public string AFormInf;
	public string AFormRPN;
	public float CalcDamage;


	#region MonoBehaviour Functions
	void Start(){
		battleGui 		= gameObject.GetComponent<BattleGUI> ();
		announcements 	= gameObject.GetComponentInChildren<Announcements> ();
	}
	
	void Update(){
		if (status == BattleStatus.Playing){
			PlayerTurn();
		}
	}
	#endregion


	#region UpdateAtkAlg
	//Used to assing the attack algorithm defined at the Editor
	public void UpdateAtkAlg(){
		AtkAlgOperandsRPN = new List<Operand>();
		Stack<Operand> stack = new Stack<Operand>();
		
		for (int i = 0; i < AtkAlgOperands.Count; i++){
			//token is a constant
			if (AtkAlgOperands[i].Operator == "C"){
				AtkAlgOperandsRPN.Add( AtkAlgOperands[i] );		}
			
			//token is a status
			if (AtkAlgOperands[i].Operator == "A" || AtkAlgOperands[i].Operator == "D"){
				AtkAlgOperandsRPN.Add( AtkAlgOperands[i] );		}
			
			//token is a operator
			if (AtkAlgOperands[i].isOperator() && !AtkAlgOperands[i].isParenthesis() ){
				while (stack.Count != 0 && 
				       ( (AtkAlgOperands[i].isLeftAssociative() && AtkAlgOperands[i].OperatorPrecedence() <= AtkAlgOperands[i].OperatorPrecedence(stack.Peek().Operator) ) 
				 || AtkAlgOperands[i].OperatorPrecedence() < AtkAlgOperands[i].OperatorPrecedence(stack.Peek().Operator) ) ){
					
					AtkAlgOperandsRPN.Add(stack.Pop() );	
				}
				
				stack.Push(AtkAlgOperands[i]);		}
			
			//token is a left parenthesis
			if (AtkAlgOperands[i].Operator == "("){
				stack.Push(AtkAlgOperands[i]);		}
			
			//token is a right parenthesis
			if (AtkAlgOperands[i].Operator == ")"){
				while (stack.Count > 0){
					if (stack.Peek().Operator != "("){
						AtkAlgOperandsRPN.Add(stack.Pop() );	}
					
					else
						break;
				}
				
				if (stack.Count == 0){
					Debug.Log("Mismathced parenthesis");
					break;
				}
				
				stack.Pop();
			}
		}
		
		while (stack.Count > 0){
			if (stack.Peek().Operator == "(" || stack.Peek().Operator == ")")
				Debug.Log("Mismathced parenthesis");
			
			AtkAlgOperandsRPN.Add(stack.Pop() );
		}

	}
	#endregion

	#region UpdateUnitsList
	//Makes sure all Units in scene are in the UnitsList
	public void UpdateUnitsList (){
		UnitsList.Clear();
		foreach (GameObject unit in (GameObject.FindGameObjectsWithTag("Unit"))){
			if (unit.GetComponent<Unit>().state != UnitState.Dying)
				UnitsList.Add(unit.GetComponent<Unit>());
		}
	}
	#endregion


	#region BattleStart
	public void BattleStart(){
		gridObject 	= GameObject.FindGameObjectWithTag("Grid");
		grid		= gridObject.GetComponent<Grid>();
		
		UpdateUnitsList();
		UpdateAtkAlg();

		CurrentTeam = 0;
		CurrentTurn = 1;

		if (UnitsList.Find (unit => (unit.isSpecial && unit.aliance == Aliance.Enemy) ) == null ) conditions.KillAllSpecial = false;
		if (UnitsList.Find (unit => (unit.isSpecial && unit.aliance == Aliance.Ally) )  == null ) conditions.AllSpecialDie  = false;

		conditions.UniqueEnemies 	= UnitsList.FindAll (unit => unit.isUnique && unit.aliance == Aliance.Enemy).Count;
		conditions.UniqueAllies 	= UnitsList.FindAll (unit => unit.isUnique && unit.aliance == Aliance.Ally) .Count;

		StartOccupiedTraversable();
		UpdateOccupiedTraversable();
	}
	#endregion

	#region PlayerTurn
	//Handles a human player turn
	public void PlayerTurn() {

		#region There's no selected unit
		if (!hasSelecetedUnit){
			if (Input.GetMouseButtonUp(0)){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (gridObject.GetComponent<MeshCollider>().Raycast(ray, out hit, 1000f)){
					selectedUnit = GetUnitInPosition(hit.point);

					//If mouse clicked in a unit
					if (selectedUnit != null){
						if(selectedUnit.Team == CurrentTeam){
							hasSelecetedUnit = true;
							battleGui.ActionsMenuCanvas.enabled = true;

							PaintFaces();
						}
					}

				}

			}	
		}
		#endregion

		#region Selected unit logic
		else {
			//Unselect unit
			if(Input.GetMouseButtonUp(1)){
				if (selectedUnit.state == UnitState.Idle){
					hasSelecetedUnit = false;
					battleGui.ActionsMenuCanvas.enabled = false;

					grid.UnpaintAllFaces();
				}

				else if (selectedUnit.state == UnitState.Attacking){
					selectedUnit.state = UnitState.Idle;

					hasSelecetedUnit = true;
					battleGui.ActionsMenuCanvas.enabled = true;

					PaintFaces();
				}
			}

			if (Input.GetMouseButtonUp(0) ){
				if (!selectedUnit.hasMoved && selectedUnit.state == UnitState.Idle) Move();
				if (selectedUnit.state == UnitState.Attacking) 						Attack();

			}	
		}
		#endregion

	}
	#endregion

	#region EndTurn
	public void EndTurn(){
		grid.UnpaintAllFaces();

		hasSelecetedUnit = false;
		battleGui.ActionsMenuCanvas.enabled = false;

		//Update Current Team/ Turn
		if (CurrentTeam == TeamsList.Length - 1) {
			CurrentTeam = 0;
			CurrentTurn++;
		}

		else{ 
			do { 
				CurrentTeam++;

				if (CurrentTeam == TeamsList.Length){
					CurrentTeam = 0;
					CurrentTurn++;
				}
			}
			while (UnitsList.Find (unit => unit.Team == CurrentTeam) == null);
		}

		//Update Grid Graph
		foreach (Unit unit in UnitsList){
			unit.hasMoved 	 = false;
			unit.hasAttacked = false;

			if (unit.Team != CurrentTeam){ 
				grid.pathGraph.Vertices[unit.currentTile].traversable = false;
				grid.pathGraph.MakeUnwalkable(unit.currentTile);
			}

			else {
				grid.pathGraph.Vertices[unit.currentTile].traversable = true;
				grid.pathGraph.MakeWalkable(unit.currentTile);
			}
		}

		//Turn announcement
		status = BattleStatus.Paused;
		announcements.NextTurn (TeamsList [CurrentTeam] + "'s turn");

	}
	#endregion

	#region Victory/ Defeat
	public void Victory (){
		status = BattleStatus.Paused;
		gameObject.GetComponent<BattleGUI>().Victory();
	}

	public void Defeat (){
		status = BattleStatus.Paused;
		gameObject.GetComponent<BattleGUI>().Defeat();
	}
	#endregion
	
	#region Unit functions

	#region Attack
	public void Attack (){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (gridObject.GetComponent<MeshCollider>().Raycast(ray, out hit, 1000f)){
			if (grid.pathGraph.Vertices[GetFaceFromPoint(hit.point)].state == VertexState.paintedAsAttackable){
				Unit defender = GetUnitInPosition(hit.point);
				
				if (defender != null){
					if (defender.aliance != selectedUnit.aliance){
						Debug.Log (selectedUnit.UnitName + " dealt " + ((int)Damage(selectedUnit, defender)).ToString() 
						           + " damage in " + defender.UnitName);
						
						defender.CurrentHP -= (int)Damage(selectedUnit, defender);

						selectedUnit.hasAttacked 	= true;
						selectedUnit.state			= UnitState.AttackAnimation;
						selectedUnit.GetComponent<Animation>().Play("attack");
						
						grid.UnpaintAllFaces();

						battleGui.ActionsMenuCanvas.enabled = false;

						Vector3 dir = (defender.transform.position - selectedUnit.transform.position).normalized;
						Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, Mathf.PI, 0f);
						selectedUnit.transform.rotation = Quaternion.LookRotation(newDir);

						//If defender died
						if (defender.CurrentHP <= 0){
						grid.pathGraph.MakeWalkable(defender.currentTile);
						grid.pathGraph.Vertices[defender.currentTile].occupied 		= false;
						grid.pathGraph.Vertices[defender.currentTile].traversable 	= true;

						defender.state = UnitState.Dying;
						UpdateUnitsList();

						defender.GetComponent<Animation>().Play("die");
					}
					}
				}

			}
		}			
	}
	#endregion

	#region Move
	public void Move (){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (gridObject.GetComponent<MeshCollider>().Raycast(ray, out hit, 1000f)){
			if (grid.pathGraph.Vertices[GetFaceFromPoint(hit.point)].state == VertexState.paintedAsWalkable){
				
				selectedUnit.WalkTo(hit.point);
				grid.UnpaintAllFaces();
			}
		}
	}
	#endregion

	#region Damage
	//Returns the Damage dealt to the defender by the attacker
	public float Damage(Unit attacker, Unit defender){
		Stack<float> stack = new Stack<float>();
		float a = 0, b = 0;
		
		for (int i = 0; i < AtkAlgOperandsRPN.Count; i++){
			if (!AtkAlgOperandsRPN[i].isOperator()){
				if (AtkAlgOperandsRPN[i].Operator == "C"){
					stack.Push((float)AtkAlgOperandsRPN[i].Number);	}
				
				else if (AtkAlgOperandsRPN[i].Operator == "A"){
					stack.Push((float)attacker.StatusList[1] );		}
				
				else if (AtkAlgOperandsRPN[i].Operator == "D"){
					stack.Push((float)defender.StatusList[2] );		}
			}
			
			else{
				if (stack.Count > 0) b = stack.Pop(); else {Debug.LogError("Error in attack algorithm"); return 0;}
				if (stack.Count > 0) a = stack.Pop(); else {Debug.LogError("Error in attack algorithm"); return 0;}
				
				if 		(AtkAlgOperandsRPN[i].Operator == "+") 	stack.Push(a + b);
				else if (AtkAlgOperandsRPN[i].Operator == "-") 	stack.Push(a - b);
				else if (AtkAlgOperandsRPN[i].Operator == "*") 	stack.Push(a * b);
				else if (AtkAlgOperandsRPN[i].Operator == "/") 	stack.Push(a / b);
				else if (AtkAlgOperandsRPN[i].Operator == "pw") 	stack.Push(Mathf.Pow(a,b) );
			}
		}
		

		if (stack.Count == 0 ){
			Debug.LogError("Null attack algorithm");
			return 0;	}

		
		else if (stack.Count > 1 ){
			Debug.LogError("Error in attack algorithm");
			return 0;	}
		
		else
			return stack.Pop();
	}
	#endregion

	#endregion

	#region Helper functions

	#region Add/ Remove Enemies from graph
	//Used when functions need the enemy's units to be in the graph
	public void AddEnemiesToGraph(){
		foreach (Unit unit in UnitsList){
			if (unit.Team != CurrentTeam){ 
				grid.pathGraph.MakeWalkable(unit.currentTile);
			}
		}
	}
	
	//Used when functions need the enemy's units not to be in the graph
	public void RemoveEnemiesFromGraph(){
		foreach (Unit unit in UnitsList){
			if (unit.Team != CurrentTeam){ 
				grid.pathGraph.MakeUnwalkable(unit.currentTile);
			}
		}
	}
	#endregion

	#region Get Unit/ Tile from point
	public Unit GetUnitInPosition(Vector3 position){
		int postionTile = GetFaceFromPoint(position);
		
		foreach (Unit unit in UnitsList){
			if (unit.currentTile == postionTile)
				return unit; }
		
		return null;
	}

	public int GetFaceFromPoint(Vector3 position){
		return ( (Mathf.FloorToInt(position.z) - (int)grid.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[0].z) * grid.X 
		        + (Mathf.FloorToInt(position.x) - (int)grid.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices[0].x) );
	}
	#endregion

	#region Occupied/ Traversable
	public void StartOccupiedTraversable (){
		for (int i = 0; i < grid.X * grid.Z; i++){
			grid.pathGraph.Vertices[i].occupied 	= false;
			grid.pathGraph.Vertices[i].traversable 	= true;
		}
	}

	public void UpdateOccupiedTraversable (){
		foreach (Unit unit in UnitsList){
			grid.pathGraph.Vertices[unit.currentTile].occupied = true;

			if (unit.Team != CurrentTeam){ 
				grid.pathGraph.Vertices[unit.currentTile].traversable = false;
				grid.pathGraph.MakeUnwalkable(unit.currentTile);
			}
		}
	}
	#endregion

	#region PaintFaces
	//Paint faces appropriately according to the current state
	public void PaintFaces(){
		grid.pathGraph.SearchFrom(selectedUnit.currentTile, selectedUnit.WalkDistance);
				
		if(selectedUnit.state == UnitState.Idle){
			//If hasn't moved and attacked, paint walkable tiles and attackable tile
			if(!selectedUnit.hasMoved && !selectedUnit.hasAttacked ){
				grid.PaintWalkableFaces(selectedUnit.currentTile ,selectedUnit.WalkDistance, walkColor);

				AddEnemiesToGraph();
				grid.PaintAttackableFaces(selectedUnit.AttackRange, attackColor);
				RemoveEnemiesFromGraph();
			}

			//If hasn't moved but has attacked, paint walkable tiles
			else if(!selectedUnit.hasMoved && selectedUnit.hasAttacked){
				grid.PaintWalkableFaces(selectedUnit.currentTile ,selectedUnit.WalkDistance, walkColor);
			}
		}
		
		//If has moved but hasn't attacked, or is reading to attack, paint attackable tiles
		if( (selectedUnit.hasMoved && !selectedUnit.hasAttacked) || selectedUnit.state == UnitState.Attacking ){
			grid.pathGraph.walkableBorder = new List<int>(){selectedUnit.currentTile};

			AddEnemiesToGraph();
			grid.PaintAttackableFaces(selectedUnit.AttackRange, attackColor);
			RemoveEnemiesFromGraph();
		}

		//Paint unit's current face green
		grid.PaintFace(selectedUnit.currentTile, selectColor);


		grid.pathGraph.SearchFrom(selectedUnit.currentTile, selectedUnit.WalkDistance);

		grid.AssignColorToMesh();
	}
	#endregion

	#endregion
}

public enum BattleStatus{
	Paused,
	Playing
}