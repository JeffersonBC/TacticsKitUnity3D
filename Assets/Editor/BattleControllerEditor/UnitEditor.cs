using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Unit))]
public class UnitEditor : Editor {
	BattleController Controller;

	public void OnEnable(){
		Controller = GameObject.FindGameObjectWithTag("BattleController").GetComponent<BattleController>();

		CheckStatus();
	}

	void StatusList(){
		SerializedProperty list = serializedObject.FindProperty("StatusList");
		
		EditorGUILayout.PropertyField(list);
		if (list.isExpanded) {
			EditorGUI.indentLevel += 1;	
									
			for (int i = 0; i < Controller.StatusList.Count; i++) {
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent (Controller.StatusList[i] ) ); 
			}
						
			EditorGUI.indentLevel -= 1;		
		}
	}

	void CheckStatus(){
		Unit unit = (Unit)target;
		int bcCount 	= Controller.StatusList.Count;
		int unitCount 	= unit.StatusList.Count;

		if (bcCount != unitCount){

			if (bcCount > unitCount){
				for (int i = 0; i < bcCount - unitCount; i++){
					unit.StatusList.Add(0);
				}
			}

			else if (bcCount < unitCount){
				for (int i = 0; i < unitCount - bcCount; i++){
					unit.StatusList.RemoveAt(unit.StatusList.Count - 1);
				}
			}


		}

	}
	
	/*
	public override void OnInspectorGUI(){
		serializedObject.Update();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("UnitName"));

		SerializedProperty team = serializedObject.FindProperty("Team");
		team.intValue = EditorGUILayout.Popup("Team: ",team.intValue, Controller.TeamsList);

		EditorGUILayout.PropertyField(serializedObject.FindProperty("aliance"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("Level"));

		EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkDistance"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("AttackRange"));

		EditorGUILayout.PropertyField(serializedObject.FindProperty("TotalHP"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentHP"));

		EditorGUILayout.PropertyField(serializedObject.FindProperty("isSpecial"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("isUnique"));

		StatusList();

		serializedObject.ApplyModifiedProperties();
	}
	*/
}
