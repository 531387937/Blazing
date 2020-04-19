using UnityEngine;

public class FeetContact : MonoBehaviour
{
	public APRController APR_Player;
	
    void OnCollisionEnter(Collision col)
	{
        if(col.gameObject.tag=="Ground")
		APR_Player.OnFeetContact();
	}
}
