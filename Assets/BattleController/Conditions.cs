using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Conditions {

	public bool KillAllEnemies 	= true;
	public bool AllAlliesDie 	= true;

	public bool KillAllSpecial	= false;
	public bool AllSpecialDie	= false;

	public bool UniqueEnemyDies	= false;
	public bool UniqueAllyDies	= false;
	public int 	UniqueAllies;
	public int 	UniqueEnemies;

	public bool SurviveTurns		= false;
	public bool MaxTurns			= false;
	public int 	StatTurn			= 1;
	public int 	NumberOfTurnsToWait = 10;

	public bool SeizePosition	= false;
	public bool LosePosition	= false;



	public ConditionsStatus CheckConditions(){
		BattleController controller = GameObject.FindGameObjectWithTag("BattleController").GetComponent<BattleController>();

		//Check for Victory
		if (KillAllEnemies)
			if (controller.UnitsList.Find (unit => unit.aliance == Aliance.Enemy) == null )
				return ConditionsStatus.Victory;

		if (KillAllSpecial)
			if (controller.UnitsList.Find (unit => (unit.isSpecial == true && unit.aliance == Aliance.Enemy) ) == null )
				return ConditionsStatus.Victory;

		if (UniqueEnemyDies)
			if (controller.UnitsList.FindAll (unit => (unit.isUnique && unit.aliance == Aliance.Enemy) ).Count < UniqueEnemies )
				return ConditionsStatus.Victory; 



		//Check for Defeat
		if (AllAlliesDie)
			if (controller.UnitsList.Find (unit => unit.aliance == Aliance.Ally) == null )
				return ConditionsStatus.Defeat;

		if (AllSpecialDie)
			if (controller.UnitsList.Find (unit => (unit.isSpecial == true && unit.aliance == Aliance.Ally) ) == null )
				return ConditionsStatus.Defeat;

		if (UniqueAllyDies)
			if (controller.UnitsList.FindAll (unit => (unit.isUnique && unit.aliance == Aliance.Ally) ).Count < UniqueAllies )
				return ConditionsStatus.Defeat; 

		return ConditionsStatus.Nothing;
	}


}

public enum ConditionsStatus{
	Victory,
	Defeat,
	Nothing
}