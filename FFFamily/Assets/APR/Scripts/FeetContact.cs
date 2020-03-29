using UnityEngine;

public class FeetContact : MonoBehaviour
{
	public APRController APR_Player;
	
    void OnCollisionEnter(Collision col)
	{
		APR_Player.OnFeetContact();
	}
}
