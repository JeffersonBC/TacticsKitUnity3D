using UnityEngine;
using System.Collections;

public class BattleGUI : MonoBehaviour {

	BattleController controller;

	public Canvas ActionsMenuCanvas;
	public Canvas AnnouncementsCanvas;
	public Canvas PreBattleCanvas;

	void Start(){
		controller = gameObject.GetComponent<BattleController>();

		if (ActionsMenuCanvas == null)
			ActionsMenuCanvas 	= transform.Find ("ActionsMenuCanvas").gameObject.GetComponent<Canvas>();

		if (AnnouncementsCanvas == null)
			AnnouncementsCanvas = transform.Find ("AnnouncementsCanvas").gameObject.GetComponent<Canvas>();

		if (PreBattleCanvas == null)
			PreBattleCanvas 	= transform.Find ("PreBattleCanvas").gameObject.GetComponent<Canvas>();
	}

	public void Attack(){
		if (!controller.selectedUnit.hasAttacked){
			controller.grid.UnpaintAllFaces();
			
			controller.selectedUnit.state = UnitState.Attacking;
			controller.PaintFaces();
		}
	}

	public void Victory(){
		controller.announcements.animator.SetBool ("Victory", true);
		controller.announcements.text.text = "Victory!!!";
		controller.status = BattleStatus.Paused;
	}

	public void Defeat(){
		controller.announcements.animator.SetBool ("Defeat", true);
		controller.announcements.text.text = "Defeat...";
		controller.status = BattleStatus.Paused;
	}
}

public enum GUIStatus{
	ShowStart,
	ShowVictory,
	ShowDefeat,
	NotShowing
}
