using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(BattleController))]
public class BattleControllerEditor : Editor {
	BattleController Controller;

	private static GUIContent moveUpButtonContent 		= new GUIContent("↑", "Move up");
	private static GUIContent moveDownButtonContent 	= new GUIContent("↓", "Move down");
	private static GUIContent deleteButtonContent		= new GUIContent("x", "Delete status");
	private static GUIContent addButtonContent 			= new GUIContent("+", "Add new status");

	private static bool AtkAlgFoldout;
	private float Constant;

	private static bool ColorFoldout;

	public void OnEnable(){
		Controller = (BattleController)target;
		Controller.UpdateUnitsList();
	}

	#region StatusList
	void StatusList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded) {
			EditorGUI.indentLevel += 1;	
			
			EditorGUILayout.BeginHorizontal();
			
			GUILayout.Label ("Status Count: " + Controller.StatusList.Count.ToString() );
			if (GUILayout.Button(addButtonContent) ){
				list.InsertArrayElementAtIndex(Controller.StatusList.Count);
				foreach(Unit unit in Controller.UnitsList){
					unit.addStatus();
				}
			}
			
			EditorGUILayout.EndHorizontal();
			
			bool hasDeleted = false; int indexDeleted = 0;
			
			for (int i = 0; i < Controller.StatusList.Count; i++) {
				GUILayout.BeginHorizontal();
				
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent ("Status " + i.ToString() ) ); 
				
				if (GUILayout.Button(moveUpButtonContent, EditorStyles.miniButtonLeft, GUILayout.Width(20f)) && i > 0 ){
					list.MoveArrayElement (i, i - 1);
					foreach(Unit unit in Controller.UnitsList){
						unit.moveStatusFromTo(i, i-1);
					}
				}
				
				if (GUILayout.Button(moveDownButtonContent, EditorStyles.miniButtonMid, GUILayout.Width(20f)) && (i < Controller.StatusList.Count - 1)  ){
					list.MoveArrayElement (i, i + 1);
					foreach(Unit unit in Controller.UnitsList){
						unit.moveStatusFromTo(i, i + 1);
					}
				}
				
				if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(20f)) ){
					hasDeleted = true;
					indexDeleted = i; 
				}
				
				GUILayout.EndHorizontal();
			}
			
			if (hasDeleted){
				list.DeleteArrayElementAtIndex(indexDeleted);
				foreach(Unit unit in Controller.UnitsList){
					unit.removeStatusAt(indexDeleted);
				}
			}
			EditorGUI.indentLevel -= 1;
			
		}
	}
	#endregion

	#region UnitsList
	void UnitsList(){
		SerializedProperty list = serializedObject.FindProperty("UnitsList");
		EditorGUILayout.PropertyField(list,true);
	}
	#endregion

	#region AtkOperandsList
	void AtkOperandsList(SerializedProperty list){
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded) {
			EditorGUI.indentLevel += 1;	
			
			EditorGUILayout.BeginHorizontal();
			
			GUILayout.Label ("Operand Count: " + Controller.AtkAlgOperands.Count.ToString() );
			if (GUILayout.Button(addButtonContent) ){
				list.InsertArrayElementAtIndex(Controller.AtkAlgOperands.Count);

			}
			
			EditorGUILayout.EndHorizontal();
			
			bool hasDeleted = false; int indexDeleted = 0;
			
			for (int i = 0; i < Controller.AtkAlgOperands.Count; i++) {
				GUILayout.BeginHorizontal();
				
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i) , new GUIContent ("Operand " + (i).ToString() ) ); 

				if (GUILayout.Button(moveUpButtonContent, EditorStyles.miniButtonLeft, GUILayout.Width(20f)) ){
					list.MoveArrayElement (i, i-1);		}
				
				if (GUILayout.Button(moveDownButtonContent, EditorStyles.miniButtonMid, GUILayout.Width(20f)) ){
					list.MoveArrayElement (i, i+1);		}
				
				if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(20f)) ){
					hasDeleted = true;
					indexDeleted = i;					}

				GUILayout.EndHorizontal();
			}
			
			if (hasDeleted){
				list.DeleteArrayElementAtIndex(indexDeleted);
			}
			EditorGUI.indentLevel -= 1;
			
		}
	}
	#endregion

	#region AtkAlgCalculator
	void AtkAlgCalculator(){

		AtkAlgFoldout = EditorGUILayout.Foldout(AtkAlgFoldout,"Attack Algorithm Calculator: ");

		if (AtkAlgFoldout){

			Undo.RecordObject(target, "Moddified attack algorithm ");

			//Attacker Buttons
			GUILayout.Label("Attacker: ");
			GUILayout.BeginHorizontal();
			for (int i = 0; i < Controller.StatusList.Count; i++){
				if( i%4 == 0) {
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();	}

				if(GUILayout.Button(BattleControllerUtility.TruncateLongString(Controller.StatusList[i],6) ) ) {
					Controller.AtkAlgOperands.Add(new Operand("A",i));
				}
			}
			GUILayout.EndHorizontal();


			//Defender Buttons
			GUILayout.Label("Defender: ");			
			GUILayout.BeginHorizontal();			
			for (int i = 0; i < Controller.StatusList.Count; i++){
				if( i%4 == 0) {
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();	}

				if(GUILayout.Button(BattleControllerUtility.TruncateLongString(Controller.StatusList[i],6) ) ) {
					Controller.AtkAlgOperands.Add(new Operand("D",i));
				}
			}			
			GUILayout.EndHorizontal();


			//Constant Buttons
			GUILayout.Label("Constant: ");
			GUILayout.BeginHorizontal();

			Constant = EditorGUILayout.FloatField(Constant);
			if(GUILayout.Button("Add"))
				Controller.AtkAlgOperands.Add(new Operand("C", Constant));

			GUILayout.EndHorizontal();


			//Operators Buttons
			GUILayout.Label("Operators: ");	
			GUILayout.BeginHorizontal();
			
			if(GUILayout.Button("+") )		{ Controller.AtkAlgOperands.Add(new Operand("+", 0)); }
			if(GUILayout.Button("-") )		{ Controller.AtkAlgOperands.Add(new Operand("-", 0)); }
			if(GUILayout.Button("*") )		{ Controller.AtkAlgOperands.Add(new Operand("*", 0)); }
			if(GUILayout.Button("/") )		{ Controller.AtkAlgOperands.Add(new Operand("/", 0)); }
			if(GUILayout.Button("^") )		{ Controller.AtkAlgOperands.Add(new Operand("pw", 0));}
			if(GUILayout.Button("(") )		{ Controller.AtkAlgOperands.Add(new Operand("(", 0)); }
			if(GUILayout.Button(")") )		{ Controller.AtkAlgOperands.Add(new Operand(")", 0)); }

			GUILayout.EndHorizontal();


			//Misc Buttons
			GUILayout.BeginHorizontal();

			if(GUILayout.Button("←") ){}
			if(GUILayout.Button("→") ){}

			if(GUILayout.Button("Erase") ){ 
				if ( Controller.AtkAlgOperands.Count > 0)
					Controller.AtkAlgOperands.RemoveAt( Controller.AtkAlgOperands.Count-1); 
			}

			if(GUILayout.Button("Clear") ){
				Controller.AtkAlgOperands.Clear();
			}

			GUILayout.EndHorizontal();


			Controller.AFormInf = BattleControllerUtility.AtkFormulaInfixString(Controller.AtkAlgOperands, Controller.StatusList);

			if (GUI.changed) EditorUtility.SetDirty(target);


			GUILayout.Label("Attack formula: ");
			GUILayout.TextArea(Controller.AFormInf);
			
			//Test
			if (Controller.UnitsList.Count >= 2)
				GUILayout.Label("Test Damage:\n" + Controller.UnitsList[0].UnitName 
				                + " attacking " + Controller.UnitsList[1].UnitName + ": " + Controller.CalcDamage.ToString());
			
			if(GUILayout.Button("Update attack formula")){
				Controller.UpdateAtkAlg();
				Controller.CalcDamage = Controller.Damage( Controller.UnitsList[0], Controller.UnitsList[1] );
			}

		}

	}
	#endregion

	public override void OnInspectorGUI(){
		serializedObject.Update();

		SerializedProperty status = serializedObject.FindProperty("StatusList");
		StatusList(status);

		AtkAlgCalculator();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentTurn"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentTeam"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("TeamsList"),true);

		GUILayout.Space(10f);
				
		ColorFoldout = EditorGUILayout.Foldout (ColorFoldout, "Colors");
		if (ColorFoldout){
			EditorGUILayout.PropertyField(serializedObject.FindProperty("walkColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("attackColor"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("selectColor"));
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty("conditions"),true);


		serializedObject.ApplyModifiedProperties();
	}
	
}
