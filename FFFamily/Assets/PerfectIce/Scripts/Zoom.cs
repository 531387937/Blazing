using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : MonoBehaviour 
{
	private float minPinchSpeed = 5.0F;
	private float minDistance = 5.0F;
	private float touchDelta;
	private Vector2 previousDistance;
	private Vector2 currentDistance;
	private float speedTouch0;
	private float speedTouch1;
	int speed = 3;
	int maxOut = 60;
	int maxIn = 10;

	void Update () 
	{
		if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved) 
		{
			currentDistance = Input.GetTouch(0).position - Input.GetTouch(1).position;
			previousDistance = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition));
			touchDelta = currentDistance.magnitude - previousDistance.magnitude;
			speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(0).deltaTime;
			speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude / Input.GetTouch(1).deltaTime;
			if ((touchDelta + minDistance <= 5) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
			{
				this.GetComponent<Camera>().fieldOfView = Mathf.Clamp(this.GetComponent<Camera>().fieldOfView + (1 * speed),maxIn,maxOut);
			}
			if ((touchDelta + minDistance > 5) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
			{
				this.GetComponent<Camera>().fieldOfView = Mathf.Clamp(this.GetComponent<Camera>().fieldOfView - (1 * speed),maxIn,maxOut);
			}
		}

		if( Input.GetAxis("Mouse ScrollWheel") < 0) 
		{
			this.GetComponent<Camera>().fieldOfView = Mathf.Clamp(this.GetComponent<Camera>().fieldOfView + (1 * speed),maxIn,maxOut);

		} 
		else if( Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			this.GetComponent<Camera>().fieldOfView = Mathf.Clamp(this.GetComponent<Camera>().fieldOfView - (1 * speed),maxIn,maxOut);		
		}
	}
}