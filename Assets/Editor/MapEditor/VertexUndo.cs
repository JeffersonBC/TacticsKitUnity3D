using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VertexUndo{
	public HashSet<int> vertices;
	public HashSet<int> faces;
	public float added;
	
	public VertexUndo(HashSet<int> selectedVertices, HashSet<int> selectedFaces, float amountAdded){
		vertices 	= new HashSet<int> (selectedVertices);
		faces 		= new HashSet<int> (selectedFaces);
		added 		= amountAdded;
	}
}
