using UnityEngine;
     
public class SmoothCamera : MonoBehaviour
{
	public Transform target;
	public float followSpeed;
    public Vector3 Offset;
    private bool follow;
	
    void Start()
	{
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
    
    void Update()
    {
        if(Input.GetKeyDown("c"))
        {
            if(!follow)
            {
                follow = true;
            }
            
            else if(follow)
            {
                follow = false;
            }
        }
    }
    
	void FixedUpdate()
	{
		if (target && follow)
		{
			transform.position = Vector3.Lerp(transform.position, target.position + Offset, followSpeed);
		}
	}
}
