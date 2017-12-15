using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Operand{
	public string	Operator;
	public float 	Number;

	public Operand(){
		Operator = "C";
		Number 	 = 0;
	}

	public Operand(string Op, float N){
		Operator = Op;
		Number 	 = N;
	}

	public int OperatorPrecedence(){
		if 		(Operator == "(" || Operator == ")") 	return 1;
		else if (Operator == "+" || Operator == "-") 	return 2;
		else if (Operator == "*" || Operator == "/") 	return 3;
		else if (Operator == "pw") 		 				return 4;
		
		else {
			Debug.LogError("Not an operator");
			return -1;
		}
	}

	public int OperatorPrecedence(string Operator){
		if 		(Operator == "(" || Operator == ")") 	return 1;
		else if (Operator == "+" || Operator == "-") 	return 2;
		else if (Operator == "*" || Operator == "/") 	return 3;
		else if (Operator == "pw") 		 				return 4;
		
		else {
			Debug.LogError("Not an operator");
			return -1;
		}
	}
	
	public bool isLeftAssociative(){
		if (Operator == "(" || Operator == ")"	
		||	Operator == "+" || Operator == "-"
		|| 	Operator == "*" || Operator == "/")
			return true;
		
		else if(Operator == "pw") 		 	
			return false;
		
		else{
			Debug.LogError("Not an operator");
			return false; 
		}
	}
	
	public bool isOperator(){
		if (Operator == "(" || Operator == ")"	
		    ||	Operator == "+" || Operator == "-"
		    || 	Operator == "*" || Operator == "/"	
		    ||	Operator == "pw") 		 	
			return true;
		
		else return false;}
	
	public bool isParenthesis (){
		if (Operator == "(" || Operator == ")")
			return true;
		else
			return false;
	}
}
