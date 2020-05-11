using UnityEngine;

public class VertPivot : MonoBehaviour 
{
	private float holdTimer = 0f;
	private float yaxis;
	private float xAxis;
	private float vertSpeed;
	private float horiSpeed;
	private float rotationY = 0f;
	private Quaternion originalRotation;
	private float originalXRot;
	private float originalYRot;
	public bool TrueOrbit = true;

	void Start ()
	{
		originalRotation = transform.localRotation;
		originalXRot = transform.localEulerAngles.x;
		originalYRot = transform.localEulerAngles.y;
	}
	
	void Update () 
	{
		if(Input.GetMouseButton(0)) holdTimer++;
		if(Input.GetMouseButton(0) && holdTimer > 3 && TrueOrbit == true) 
		{
			yaxis = -Input.GetAxis("Mouse Y");
			vertSpeed = yaxis;
			xAxis = Input.GetAxis("Mouse X");
			horiSpeed = xAxis;
		} 
		else 
		{
			vertSpeed = Mathf.Lerp(0, 0, 0);
			horiSpeed = Mathf.Lerp(0, 0, 0);
		}

		if(Input.GetMouseButtonUp(0)) 
		{
			holdTimer = 0;
		}
		rotationY += vertSpeed * -5.0f * 0.8f;
		rotationY = Mathf.Clamp (rotationY, -90, 90);
		var yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
		transform.localRotation = originalRotation * yQuaternion;
		transform.Rotate(0.0f, horiSpeed * 5.0f ,0.0f,  Space.World);
	}
}