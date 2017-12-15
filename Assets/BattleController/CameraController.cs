using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	#if UNITY_STANDALONE || UNITY_PS3 || UNITY_XBOX360
	public float CameraSpeed = 0.5f;	//Camera's movement speed
	public float MouseWheelZoom = 2;	//Camera's zoom speed
	
	private float proportionalDrag;
	#endif
	
	
	#if UNITY_ANDROID || UNITY_IPHONE
	private float previousDistance;
			
	public float zoomSpeed = 0.03f;
	public float dragSpeedX = 0.010f;
	public float dragSpeedY = 0.015f;
	
	private float proportionalDrag;
	#endif
	
	public float MaxZoom = 8;
	public float MinZoom = 3;
	
	void Update () 
	{
		#if UNITY_STANDALONE || UNITY_PS3 || UNITY_XBOX360
			ZoomAndMove();
		#endif
		
		
		#if UNITY_ANDROID || UNITY_IPHONE
			ZoomAndMoveMobile();
		#endif	
		
	}
	
	#if UNITY_STANDALONE || UNITY_PS3 || UNITY_XBOX360
	void ZoomAndMove ()
	{
		proportionalDrag = ((gameObject.GetComponent<Camera>().orthographicSize - 3) / 5) * 2 + 1;			
		
		gameObject.transform.position += 
			(Input.GetAxis("Horizontal") * CameraSpeed / proportionalDrag * gameObject.transform.right) +
			(Input.GetAxis("Vertical") 	 * CameraSpeed / proportionalDrag * (Quaternion.Euler (new Vector3 (0,-90,0)) * gameObject.transform.right));
		
		gameObject.GetComponent<Camera>().orthographicSize += Input.GetAxis("Mouse ScrollWheel") * MouseWheelZoom;
		
		if (gameObject.GetComponent<Camera>().orthographicSize > MaxZoom)
				gameObject.GetComponent<Camera>().orthographicSize = MaxZoom;
			
			if (gameObject.GetComponent<Camera>().orthographicSize < MinZoom)
				gameObject.GetComponent<Camera>().orthographicSize = MinZoom;	
	}
	#endif
	
	#if UNITY_ANDROID || UNITY_IPHONE
	void ZoomAndMoveMobile ()
	{
		if(Input.touchCount == 2 && 
			(Input.GetTouch(0).phase == TouchPhase.Began || 
			Input.GetTouch(1).phase == TouchPhase.Began) )
		{
			//calibrate previous distance
			previousDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
		}
		
		else if (Input.touchCount == 2 && 
			(Input.GetTouch(0).phase == TouchPhase.Moved || 
			Input.GetTouch(1).phase == TouchPhase.Moved) )
			
		{
			float distance;
			Vector2 touch1 = Input.GetTouch(0).position;
			Vector2 touch2  = Input.GetTouch(1).position;
			distance = Vector2.Distance(touch1, touch2);
			
			//move camera on the y based on the distance of the pinch
			gameObject.GetComponent<Camera>().orthographicSize += ((previousDistance - distance) * zoomSpeed);
			
			if (gameObject.GetComponent<Camera>().orthographicSize > MaxZoom)
				gameObject.GetComponent<Camera>().orthographicSize = MaxZoom;
			
			if (gameObject.GetComponent<Camera>().orthographicSize < MinZoom)
				gameObject.GetComponent<Camera>().orthographicSize = MinZoom;
			
			previousDistance = distance;
		}
		
		else if(Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			proportionalDrag = ((gameObject.GetComponent<Camera>().orthographicSize - 3) / 5) * 2 + 1;
			
		    gameObject.transform.position -= 	
				(Input.GetTouch(0).deltaPosition.x * dragSpeedX * proportionalDrag) * gameObject.transform.right +			
		    	(Input.GetTouch(0).deltaPosition.y * dragSpeedY * proportionalDrag) * (Quaternion.Euler (new Vector3 (0,-90,0)) * gameObject.transform.right);
		}
	}
	#endif	
	
	
}
