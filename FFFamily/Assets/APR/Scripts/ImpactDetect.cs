using UnityEngine;

public class ImpactDetect : MonoBehaviour
{
	public APRController APR_Player;
    public float ImpactForce;
	public float KnockoutForce;
    
    public AudioClip[] Impacts;
    public AudioClip[] Hits;
    public AudioSource SoundSource;

	void OnCollisionEnter(Collision col)
	{
        
        //Knockout by impact
		if(col.relativeVelocity.magnitude > KnockoutForce)
		{
			APR_Player.ActivateRagdoll();
            
            if(!SoundSource.isPlaying)
            {
                int i = Random.Range(0, Hits.Length);
                SoundSource.clip = Hits[i];
                SoundSource.Play();
            }
		}
        
        //Sound on impact
        if(col.relativeVelocity.magnitude > ImpactForce)
        {
            if(!SoundSource.isPlaying)
            {
                int i = Random.Range(0, Impacts.Length);
                SoundSource.clip = Impacts[i];
                SoundSource.Play();
            }
        }
	}
}
