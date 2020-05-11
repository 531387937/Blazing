using UnityEngine;

public class HoriPivot : MonoBehaviour 
{
	private float speed;
	private float holdTimer = 0.0f;
	private float xAxis = 0.0f;
	private int lastTouch = 0;
	public bool TrueOrbit = true;

	void Update () 
	{
		if(Input.touchCount < 2 && lastTouch < 2) 
		{
			if(Input.touchCount == 0) lastTouch = 0;
			if(Input.GetMouseButton(0)) holdTimer++;
			if (Input.GetMouseButton(0) && holdTimer > 3 && TrueOrbit == true)
			{
				holdTimer ++;
				xAxis = Input.GetAxis("Mouse X");
				speed = xAxis;
			} 
			else 
			{
				speed = Mathf.Lerp(0, 0, 0);
			}
			if (Input.GetMouseButtonUp(0))
			{
				holdTimer = 0;
			}
			transform.Rotate(0.0f, speed * 5.0f, 0.0f,  Space.World);
		} 
		else 
		{
			lastTouch = Input.touchCount;
			speed = 0;
		}
	}
}
