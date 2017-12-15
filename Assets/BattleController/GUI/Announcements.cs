using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Announcements : MonoBehaviour {

	public Animator animator;
	public Text text;

	void Start(){
		animator = gameObject.GetComponent<Animator> ();
		text = gameObject.GetComponent<Text> ();
	}

	public void BattleStart(){
		text.text = "Battle Start!!!";
		animator.SetBool ("Start", true);
	}

	public void NextTurn(string s){
		text.text = s;
		animator.SetBool ("NextTurn", true);
	}

	public void BattleResume(){
		gameObject.GetComponentInParent<BattleController> ().status = BattleStatus.Playing;
		animator.SetBool ("Start", false);
		animator.SetBool ("NextTurn", false);
	}
}
