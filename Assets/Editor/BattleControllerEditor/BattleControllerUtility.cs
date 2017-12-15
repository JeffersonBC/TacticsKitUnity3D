using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleControllerUtility {

	public static int OperatorPrecedence(string Operator){
		if 		(Operator == "(" || Operator == ")") 	return 1;
		else if (Operator == "+" || Operator == "-") 	return 2;
		else if (Operator == "*" || Operator == "/") 	return 3;
		else if (Operator == "pw") 		 				return 4;
		
		else {
			Debug.LogError("Not an operator");
			return -1;
		}
	}

	public static string AtkFormulaInfixString(List<Operand> AtkAlgOperands, List<string> StatusList){
		string formula = string.Empty;
		
		for (int i = 0; i < AtkAlgOperands.Count; i++){
			if (AtkAlgOperands[i].Operator == "A" || AtkAlgOperands[i].Operator == "D"){
				formula += AtkAlgOperands[i].Operator + '.' + 
					StatusList[(int)AtkAlgOperands[i].Number] + ' ';		}
			
			else if (AtkAlgOperands[i].Operator == "C"){
				formula += AtkAlgOperands[i].Number.ToString() + ' '; 	}
			
			else if (AtkAlgOperands[i].Operator == "+" || AtkAlgOperands[i].Operator == "-" ||
			         AtkAlgOperands[i].Operator == "*" || AtkAlgOperands[i].Operator == "/" ||
			         AtkAlgOperands[i].Operator == "(" || AtkAlgOperands[i].Operator == ")"){
			         				
				formula += AtkAlgOperands[i].Operator + ' ';
			}

			else if (AtkAlgOperands[i].Operator == "pw"){
				formula += "^ ";
			}
		}
		
		return formula;
	}

	public static string AtkFormulaRPNString(List<Operand> AtkAlgOperands, List<string> StatusList){
		string output = string.Empty;
		Stack<string> stack = new Stack<string>();
		
		for (int i = 0; i < AtkAlgOperands.Count; i++){
			//token is a constant
			if (AtkAlgOperands[i].Operator == "C"){
				//Debug.Log(AtkAlgOperands[i].Number.ToString() + " added to output");
				output += AtkAlgOperands[i].Number.ToString() + " ";	
			}
			
			//token is a status
			if (AtkAlgOperands[i].Operator == "A" || AtkAlgOperands[i].Operator == "D"){
				//Debug.Log(AtkAlgOperands[i].Operator + '.' + StatusList[(int)AtkAlgOperands[i].Number].ToString() + " added to output");
				output += AtkAlgOperands[i].Operator + '.' + StatusList[(int)AtkAlgOperands[i].Number].ToString() + ' ';	
			}
			
			//token is a operator
			if (AtkAlgOperands[i].isOperator() && !AtkAlgOperands[i].isParenthesis() ){
				while (stack.Count != 0 && 
				       ( (AtkAlgOperands[i].isLeftAssociative() && AtkAlgOperands[i].OperatorPrecedence() <= BattleControllerUtility.OperatorPrecedence(stack.Peek()) ) 
				 || AtkAlgOperands[i].OperatorPrecedence() < BattleControllerUtility.OperatorPrecedence(stack.Peek() ) ) ){
					
					//Debug.Log (stack.Peek().ToString() + " popped from stack into output");
					output += stack.Pop() + ' ';	
				}
				
				//Debug.Log (AtkAlgOperands[i].Operator + " pushed into stack");
				stack.Push(AtkAlgOperands[i].Operator);
			}
			
			//token is a left parenthesis
			if (AtkAlgOperands[i].Operator == "("){
				//Debug.Log ("( pushed into stack");
				stack.Push("(");
			}
			
			//token is a right parenthesis
			if (AtkAlgOperands[i].Operator == ")"){
				//Debug.Log (") pushed into stack");
				while (stack.Count > 0){
					if (stack.Peek() != "("){
						
						//Debug.Log (stack.Peek().ToString() + " popped from stack into output");
						output += stack.Pop() + ' '; }
					
					else
						break;
				}
				
				if (stack.Count == 0){
					Debug.Log("Mismathced parenthesis");
					break;
				}
				//Debug.Log (stack.Peek().ToString() + " popped from stack");
				stack.Pop();
			}
		}
		
		while (stack.Count > 0){
			if (stack.Peek() == "(" || stack.Peek() == ")")
				Debug.Log("Mismathced parenthesis");
			
			//Debug.Log (stack.Peek().ToString() + " popped from stack into output");
			output += stack.Pop() + ' ';
		}
		
		return output;
	}

	public static List<Operand> AtkAlgDamageStackRPN(List<Operand> AtkAlgOperands){
		List<Operand> AtkAlgOperandsRPN = new List<Operand>();
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
				       ( (AtkAlgOperands[i].isLeftAssociative() && AtkAlgOperands[i].OperatorPrecedence() <= BattleControllerUtility.OperatorPrecedence(stack.Peek().Operator) ) 
				 || AtkAlgOperands[i].OperatorPrecedence() < BattleControllerUtility.OperatorPrecedence(stack.Peek().Operator) ) ){
					
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

		return AtkAlgOperandsRPN;
	}

	public static string TruncateLongString(string str, int maxLength)
	{
		return str.Substring(0, Mathf.Min(str.Length, maxLength));
	}
}
