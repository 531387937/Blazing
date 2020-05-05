using UnityEngine;

public class FeetContact : MonoBehaviour
{
	public APRController APR_Player;
	
    void OnCollisionEnter(Collision col)
	{
        if (col.gameObject.tag == "Ground")
        {
            APR_Player.OnFeetContact();
            APR_Player.footOnGround = true;
        }
	}
    private void OnCollisionStay(Collision collision)
    {
        APR_Player.footOnGround = true;
    }
}
